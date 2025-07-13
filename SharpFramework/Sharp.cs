namespace SharpFramework
{
    /// <summary>
    /// Sharpの機能を提供するクラス
    /// </summary>
    public static class Sharp
    {
        public static int FrameLimit = 0;
        public static bool Vsync = true;

        public static void SetFrameLimit(int frameLimit)
        {
            FrameLimit = frameLimit;
        }

        public static void SetVsync(bool IsUseVsync)
        {
            Vsync = IsUseVsync;
        }
    }
}
