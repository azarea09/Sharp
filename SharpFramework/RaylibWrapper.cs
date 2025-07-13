using Raylib_cs;
using SharpFramework.Audio;
using SharpFramework.Platform.Windows;
using SharpFramework.Utils;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Un4seen.Bass.Misc;


namespace SharpFramework
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
            if (Sharp.FrameLimit > 0)
            {
                Raylib.SetTargetFPS(Sharp.FrameLimit);
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
            Raylib.SetTextureFilter(_target.Texture, TextureFilter.Bilinear);

            // ----------------------------
            // モニターの情報取得
            // ----------------------------
            int monitor = Raylib.GetCurrentMonitor();
            _monitorWidth = Raylib.GetMonitorWidth(monitor);
            _monitorHeight = Raylib.GetMonitorHeight(monitor);

            // ----------------------------
            // ダークモード適応 
            // ----------------------------
            if (Window.IsDrakMode && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr hwnd = (nint)Raylib.GetWindowHandle();
                DarkTitlebar.SetDarkModeTitleBar(hwnd, true);
                DarkTitlebar.RefreshWindowLayout(hwnd);
            }

            // ----------------------------
            // FPS更新タイマーを開始
            // ----------------------------
            _fpsCounter.Looped += (s, e) => SetTitleWithFPS();
            _fpsCounter.Start();

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
            if (_isFullScreen && (_monitorWidth == RenderSurface.Width && _monitorHeight == RenderSurface.Height))
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                drawAction?.Invoke();
                Raylib.EndDrawing();
            }
            else
            {
                Raylib.BeginTextureMode(_target);
                Raylib.ClearBackground(Color.Black);
                drawAction?.Invoke();
                Raylib.EndTextureMode();

                Raylib.BeginDrawing();
                DrawWindow(_target);
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
        /// 仮想画面を実画面に描画します。
        /// </summary>
        private static void DrawWindow(RenderTexture2D target)
        {
            float scale = Math.Min((float)Raylib.GetScreenWidth() / RenderSurface.Width, (float)Raylib.GetScreenHeight() / RenderSurface.Height);

            Rectangle source = new Rectangle(0, 0, RenderSurface.Width, -RenderSurface.Height); // 上下反転
            Rectangle dest = new Rectangle(
                (Raylib.GetScreenWidth() - RenderSurface.Width * scale) * 0.5f,
                (Raylib.GetScreenHeight() - RenderSurface.Height * scale) * 0.5f,
                RenderSurface.Width * scale,
                RenderSurface.Height * scale
            );

            Rlgl.SetBlendMode(BlendMode.Alpha);
            Raylib.DrawTexturePro(target.Texture, source, dest, Vector2.Zero, 0, Color.White);
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
            PowerSetActiveScheme(IntPtr.Zero, IntPtr.Zero);
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
