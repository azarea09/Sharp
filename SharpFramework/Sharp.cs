using Raylib_cs;

namespace SharpFramework
{
    /// <summary>
    /// Sharpの機能を提供するクラス
    /// </summary>
    public static class Sharp
    {
        public static int TargetFrameRate = 0;
        public static bool Vsync = true;
        public static Color BackGroundColor = Color.RayWhite;

        public static void SetTargetFrameRate(int targetFrameRate)
        {
            TargetFrameRate = targetFrameRate;
        }

        public static void SetVsync(bool IsUseVsync)
        {
            Vsync = IsUseVsync;
        }

        public static void ChangeBackGroundColor(Color color)
        {
            BackGroundColor = color;
        }
    }
}
