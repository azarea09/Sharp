using Raylib_cs;
using SharpFramework.Audio;
using SharpFramework.Core;
using SharpFramework.Graphics;
using SharpFramework.Internal.Platform.Windows;
using SharpFramework.Utils;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;


namespace SharpFramework.Internal
{
    internal static class RaylibWrapper
    {
        internal static bool ShouldClose() => Raylib.WindowShouldClose();
        internal static bool IsInitWindow = false;

        private static RenderTexture2D _target;
        private static bool _isFullScreen = false;
        private static int _monitorWidth; // モニターのサイズ
        private static int _monitorHeight;
        private static Counter _fpsCounter = new(0, 1, 1000, true); // 1000msごとに1回更新

        // キャッシュ用変数
        private static bool _isDirectDraw = false;
        private static Rectangle _sourceRect;
        private static Rectangle _destRect;

        internal unsafe static void Init(bool isWasapiExclusive = false)
        {
            // ----------------------------
            // プロセスの優先度、電源管理を設定にする
            // ----------------------------
            SetProcessPriority();
            DisablePowerSavingMode();

            // ----------------------------
            // ConfigFlags設定
            // ----------------------------
            if (Window.IsResizable)
            {
                Raylib.SetWindowState(ConfigFlags.ResizableWindow);
            }
            if (Sharp.TargetFrameRate > 0)
            {
                Raylib.SetTargetFPS(Sharp.TargetFrameRate);
            }
            if (Sharp.Vsync)
            {
                Raylib.SetWindowState(ConfigFlags.VSyncHint);
            }

            Raylib.InitWindow(Window.Width, Window.Height, $"{Window.Title} | FPS {Raylib.GetFPS()} | W {Window.Width}x{Window.Height} | RS {RenderSurface.Width}x{RenderSurface.Height}");

            // ----------------------------
            // レンダーサーフェス設定
            // ----------------------------
            _target = Raylib.LoadRenderTexture(RenderSurface.Width, RenderSurface.Height);
            Raylib.SetTextureFilter(_target.Texture, (TextureFilter)RenderSurface.Filter);

            // ----------------------------
            // モニターの情報取得
            // ----------------------------
            int monitor = Raylib.GetCurrentMonitor();
            _monitorWidth = Raylib.GetMonitorWidth(monitor);
            _monitorHeight = Raylib.GetMonitorHeight(monitor);

            // ----------------------------
            // 描画モードを事前計算
            // ----------------------------
            CalculateRenderMode();

            // ----------------------------
            // ダークモード適応 
            // ----------------------------
            if (Window.DarkMode && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                nint hwnd = (nint)Raylib.GetWindowHandle();
                DarkTitlebar.SetDarkModeTitleBar(hwnd, true);
                DarkTitlebar.RefreshWindowLayout(hwnd);
            }

            // ----------------------------
            // FPS更新タイマーを開始
            // ----------------------------
            _fpsCounter.Looped += (s, e) => SetTitleWithFPS();
            _fpsCounter.Start();

            // ----------------------------
            // フォント読み込み
            // ----------------------------
            double scaleRatio = 1920.0 / RenderSurface.Width;
            Sharp.DefaultFontBig = new TTFFont("SharpFramework.Resources.Fonts.NotoSansCJKjp-Regular.ttf", true, (int)Math.Ceiling(64 / scaleRatio), null, true);
            Sharp.DefaultFontMid = new TTFFont("SharpFramework.Resources.Fonts.NotoSansCJKjp-Regular.ttf", true, (int)Math.Ceiling(36 / scaleRatio), null, true);
            Sharp.DefaultFontSmall = new TTFFont("SharpFramework.Resources.Fonts.NotoSansCJKjp-Regular.ttf", true, (int)Math.Ceiling(26 / scaleRatio), null, true);

            AudioManager.Init(isWasapiExclusive);
            IsInitWindow = true;
        }

        internal static void End()
        {
            AudioManager.Free();
            Raylib.UnloadRenderTexture(_target);
            Raylib.CloseWindow();
        }


        internal static void Update()
        {
            AudioManager.Update();
            _fpsCounter.Tick();

            // ----------------------------
            // ウィンドウサイズ変更の検出と同期
            // ----------------------------
            if (Raylib.IsWindowResized())
            {
                Window.Resize(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                SetTitleWithFPS();
                CalculateRenderMode();
            }

            // ----------------------------
            // フルスクリーン切り替え
            // ----------------------------
            if (Raylib.IsKeyPressed(KeyboardKey.F11) || Raylib.IsKeyPressed(KeyboardKey.Enter) && (Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt)))
            {
                ToggleFullScreen();
            }
        }

        internal static void Draw(Action drawAction)
        {
            // ----------------------------
            // ゲームの基本解像度とモニターのサイズが一緒かつ、フルスクリーンの場合は仮想画面を使わずに直接描画する。
            // それ以外の場合は仮想画面に一旦描画してからスケーリングしてウィンドウに描画する。
            // ----------------------------
            if (_isDirectDraw)
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Sharp.BackGroundColor);
                drawAction?.Invoke();
                Raylib.EndDrawing();
            }
            else
            {
                Raylib.BeginTextureMode(_target);
                Raylib.ClearBackground(Sharp.BackGroundColor);
                drawAction?.Invoke();
                Raylib.EndTextureMode();

                Raylib.BeginDrawing();
                DrawWindow();
                Raylib.EndDrawing();
            }
        }

        internal static void SetWindowSize(int width, int height)
        {
            if (!_isFullScreen)
            {
                Raylib.SetWindowSize(Window.Width, Window.Height);
            }
        }

        #region [private]

        /// <summary>
        /// 描画モードを事前計算してキャッシュします。
        /// </summary>
        private static void CalculateRenderMode()
        {
            // 直接描画可能かどうかの判定をキャッシュ
            _isDirectDraw = (_isFullScreen && _monitorWidth == RenderSurface.Width && _monitorHeight == RenderSurface.Height) ||
                (_isFullScreen && _monitorWidth == RenderSurface.Width && _monitorHeight == RenderSurface.Height);

            if (!_isDirectDraw)
            {
                // スケーリング描画用の値を事前計算
                float scale = Math.Min((float)Raylib.GetScreenWidth() / RenderSurface.Width,
                                (float)Raylib.GetScreenHeight() / RenderSurface.Height);

                _sourceRect = new Rectangle(0, 0, RenderSurface.Width, -RenderSurface.Height);
                _destRect = new Rectangle(
                    (float)Math.Floor((Raylib.GetScreenWidth() - RenderSurface.Width * scale) * 0.5f),
                    (float)Math.Floor((Raylib.GetScreenHeight() - RenderSurface.Height * scale) * 0.5f),
                    (float)Math.Floor(RenderSurface.Width * scale),
                    (float)Math.Floor(RenderSurface.Height * scale)
                );
            }
        }

        /// <summary>
        /// 仮想画面を実画面に描画します。
        /// </summary>
        private static void DrawWindow()
        {
            Rlgl.SetBlendMode(BlendMode.Alpha);
            Raylib.DrawTexturePro(_target.Texture, _sourceRect, _destRect, Vector2.Zero, 0, Color.White);
        }

        /// <summary>
        /// フルスクリーンモードを切り替えます。
        /// </summary>
        private static void ToggleFullScreen()
        {
            if (_isFullScreen)
            {
                Raylib.ToggleBorderlessWindowed();
                Raylib.SetWindowSize(Window.Width, Window.Height);
            }
            else
            {
                Raylib.ToggleBorderlessWindowed();
                Raylib.SetWindowSize(_monitorWidth, _monitorHeight);
            }

            _isFullScreen = !_isFullScreen;
            CalculateRenderMode();
        }

        /// <summary>
        /// プロセスの優先度を設定する。
        /// </summary>
        private static void SetProcessPriority()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        }

        /// <summary>
        /// 電源管理を無効にする。
        /// </summary>
        private static void DisablePowerSavingMode()
        {
            PowerSetActiveScheme(nint.Zero, nint.Zero);
        }

        [DllImport("powrprof.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool PowerSetActiveScheme(nint UserRootPowerKey, nint ActivePolicyGuid);

        private static void SetTitleWithFPS()
        {
            Raylib.SetWindowTitle($"{Window.Title} | FPS {Raylib.GetFPS()} | W {Window.Width}x{Window.Height} | RS {RenderSurface.Width}x{RenderSurface.Height}");
        }
        #endregion
    }
}
