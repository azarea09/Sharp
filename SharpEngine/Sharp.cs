using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SharpEngine
{
    /// <summary>
    /// Raylibを使いやすくラップした物を提供するクラス。
    /// </summary>
    public static class Sharp
    {
        public static int WindowWidth; // ウィンドウのサイズ
        public static int WindowHeight;
        public static int SceneWidth; // ゲームの基本解像度
        public static int SceneHeight;

        private static RenderTexture2D target;
        private static bool _isFullScreen = false;
        private static int _monitorWidth; // モニターのサイズ
        private static int _monitorHeight;

        /// <summary>
        /// Sharp と Raylibの初期化をする。必ず Sharp の使用前に呼び出す必要がある。
        /// </summary>
        public static void Init(Action beforeInit, Action afterInit, Vector2 WindowSize, Vector2 SceneSize, string WindowTitle, bool isWasapiExclusive = false)
        {
            SetProcessPriority(); // プロセスの優先度を設定する
            DisablePowerSavingMode(); // 電源管理を無効にする

            beforeInit?.Invoke();

            WindowWidth = (int)WindowSize.X;
            WindowHeight = (int)WindowSize.Y;
            Raylib.InitWindow(WindowWidth, WindowHeight, WindowTitle);

            SceneWidth = (int)SceneSize.X;
            SceneHeight = (int)SceneSize.Y;
            target = Raylib.LoadRenderTexture(SceneWidth, SceneHeight);
            Raylib.SetTextureFilter(target.Texture, TextureFilter.Bilinear);

            int monitor = Raylib.GetCurrentMonitor();
            _monitorWidth = Raylib.GetMonitorWidth(monitor);
            _monitorHeight = Raylib.GetMonitorHeight(monitor);

            AudioManager.Init(isWasapiExclusive);

            afterInit?.Invoke();
        }

        /// <summary>
        /// Sharp と Raylibの終了処理をする。
        /// </summary>
        public static void End()
        {
            AudioManager.Free();
            Raylib.UnloadRenderTexture(target);
            Raylib.CloseWindow();
        }

        /// <summary>
        /// ループ直後に呼び出すメソッド。(drawActionに実際に描画したいコードを書く)
        /// </summary>
        public static void Loop(Action drawAction)
        {
            AudioManager.Update();
            if (Raylib.IsKeyPressed(KeyboardKey.F11) || Raylib.IsKeyPressed(KeyboardKey.Enter) && (Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt)))
            {
                ToggleFullScreen();
            }

            // ゲームの基本解像度とモニターのサイズが一緒かつ、フルスクリーンの場合は仮想画面を使わずに直接描画する。
            // それ以外の場合は仮想画面に一旦描画してからスケーリングしてウィンドウに描画する。
            if (_isFullScreen && (_monitorWidth == SceneWidth && _monitorHeight == SceneHeight))
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                drawAction?.Invoke();
                Raylib.EndDrawing();
            }
            else
            {
                Raylib.BeginTextureMode(target);
                Raylib.ClearBackground(Color.Black);
                drawAction?.Invoke();
                Raylib.EndTextureMode();

                Raylib.BeginDrawing();
                DrawWindow(target);
                Raylib.EndDrawing();
            }
        }

        #region [private]
        /// <summary>
        /// 仮想画面を実画面に描画します。
        /// </summary>
        private static void DrawWindow(RenderTexture2D target)
        {
            float scale = Math.Min((float)Raylib.GetScreenWidth() / SceneWidth, (float)Raylib.GetScreenHeight() / SceneHeight);

            Rectangle source = new Rectangle(0, 0, SceneWidth, -SceneHeight); // 上下反転
            Rectangle dest = new Rectangle(
                (Raylib.GetScreenWidth() - SceneWidth * scale) * 0.5f,
                (Raylib.GetScreenHeight() - SceneHeight * scale) * 0.5f,
                SceneWidth * scale,
                SceneHeight * scale
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
                Raylib.SetWindowSize(WindowWidth, WindowHeight);
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
        #endregion
    }
}
