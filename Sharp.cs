using Raylib_cs;
using System.Numerics;

namespace Sharp
{
    public static class Sharp
    {
        private static RenderTexture2D target;
        private static bool _isFullScreen = false;
        private static int _monitorWidth; // モニターのサイズ
        private static int _monitorHeight;
        private static int _windowWidth; // ウィンドウのサイズ
        private static int _windowHeight;
        private static int _sceneWidth; // ゲームの基本解像度
        private static int _scenelHeight;

        /// <summary>
        /// Sharp と Raylibの初期化をする。必ず Sharp の使用前に呼び出す必要がある。
        /// </summary>
        public static void Init(Action beforeInit, Action afterInit, Vector2 WindowSize, Vector2 SceneSize, string WindowTitle)
        {
            beforeInit?.Invoke();

            _windowWidth = (int)WindowSize.X;
            _windowHeight = (int)WindowSize.Y;
            Raylib.InitWindow(_windowWidth, _windowHeight, WindowTitle);

            _sceneWidth = (int)SceneSize.X;
            _scenelHeight = (int)SceneSize.Y;
            target = Raylib.LoadRenderTexture(_sceneWidth, _scenelHeight);
            Raylib.SetTextureFilter(target.Texture, TextureFilter.Bilinear);

            int monitor = Raylib.GetCurrentMonitor();
            _monitorWidth = Raylib.GetMonitorWidth(monitor);
            _monitorHeight = Raylib.GetMonitorHeight(monitor);

            AudioManager.Init();

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
            if (Raylib.IsKeyPressed(KeyboardKey.F11) || Raylib.IsKeyPressed(KeyboardKey.Enter) && (Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt)))
            {
                ToggleFullScreen();
            }

            AudioManager.Update();

            // ゲームの基本解像度とモニターのサイズが一緒かつ、フルスクリーンの場合は仮想画面を使わずに直接描画する。
            // それ以外の場合は仮想画面に一旦描画してからスケーリングしてウィンドウに描画する。
            if (_isFullScreen && (_monitorWidth == _sceneWidth && _monitorHeight == _scenelHeight))
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                drawAction?.Invoke();
                Raylib.EndDrawing();
            }
            else
            {
                Raylib.BeginTextureMode(target);
                drawAction?.Invoke();
                Raylib.EndTextureMode();

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
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
            float scale = Math.Min((float)Raylib.GetScreenWidth() / _sceneWidth, (float)Raylib.GetScreenHeight() / _scenelHeight);

            Rectangle source = new Rectangle(0, 0, _sceneWidth, -_scenelHeight); // 上下反転
            Rectangle dest = new Rectangle(
                (Raylib.GetScreenWidth() - _sceneWidth * scale) * 0.5f,
                (Raylib.GetScreenHeight() - _scenelHeight * scale) * 0.5f,
                _sceneWidth * scale,
                _scenelHeight * scale
            );

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
                Raylib.SetWindowSize(_windowWidth, _windowHeight);
            }
            else
            {
                Raylib.ToggleBorderlessWindowed();
                Raylib.SetWindowSize(_monitorWidth, _monitorHeight);
            }

            _isFullScreen = !_isFullScreen;
        }
        #endregion
    }
}
