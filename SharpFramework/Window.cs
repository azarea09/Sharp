using Raylib_cs;

namespace SharpFramework
{
    /// <summary>
    /// ウィンドウの設定（タイトル、サイズ、外観など）を管理するクラス。
    /// </summary>
    public static class Window
    {
        private static string _title = "MyGame";

        /// <summary>
        /// ウィンドウのタイトル。
        /// 設定後、ウィンドウが初期化されていれば即座に反映されます。
        /// </summary>
        public static string Title
        {
            get => _title;
            set
            {
                _title = value;
                if (RaylibWrapper.IsInitWindow)
                    Raylib.SetWindowTitle(_title);
            }
        }

        /// <summary>ウィンドウの幅。</summary>
        public static int Width { get; set; } = 1280;

        /// <summary>ウィンドウの高さ。</summary>
        public static int Height { get; set; } = 720;

        /// <summary>ダークモードを使用するかどうか。</summary>
        public static bool DarkMode { get; set; } = true;

        /// <summary>ウィンドウのリサイズを許可するかどうか。</summary>
        public static bool IsResizable { get; set; } = true;

        /// <summary>
        /// ウィンドウサイズを指定された幅と高さに変更します。
        /// </summary>
        public static void Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// タイトルの設定を行います。
        /// </summary>
        public static void SetTitle(string title) => Title = title;

        /// <summary>
        /// ダークモードの設定を行います。
        /// </summary>
        public static void SetDarkMode(bool isDarkMode) => DarkMode = isDarkMode;

        /// <summary>
        /// ウィンドウのリサイズ設定を行います。
        /// </summary>
        public static void SetResizable(bool isResizable) => IsResizable = isResizable;
    }
}
