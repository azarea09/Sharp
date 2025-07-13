using Raylib_cs;

namespace SharpFramework
{
    /// <summary>
    /// ウィンドウのサイズやタイトルなどを持つクラス。
    /// </summary>
    public static class Window
    {
        public static int Width = 1280;
        public static int Height = 720;
        public static string Title = "MyGame";
        public static bool IsDrakMode = true;
        public static bool IsResizable = true;

        public static void SetTitle(string title)
        {
            Title = title;

            if (RaylibWrapper.IsInitWindow)
                Raylib.SetWindowTitle(Title);
        }

        public static void Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public static void SetDrakMode(bool isDrakMode)
        {
            IsDrakMode = isDrakMode;
        }

        public static void SetResizable(bool isResizeable)
        {
            IsResizable = isResizeable;
        }
    }
}
