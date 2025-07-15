namespace SharpFramework.Core
{
    /// <summary>
    /// ゲームの表示領域のクラス。
    /// </summary>
    public static class RenderSurface
    {
        public static int Width = 1920;
        public static int Height = 1080;
        public static RenderSurfaceFilter Filter = RenderSurfaceFilter.Bilinear;
        public static bool UseRenderSurface = true;

        public static void SetUseRenderSurface(bool useRenderSurface)
        {
            UseRenderSurface = useRenderSurface;
        }

        /// <summary>
        /// 指定したサイズの解像度に設定します。
        /// </summary>
        public static void Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// ウィンドウの拡縮の際の補完方法を設定します。
        /// </summary>
        public static void SetFilter(RenderSurfaceFilter renderSurfaceFilter)
        {
            Filter = renderSurfaceFilter;
        }
    }

    public enum RenderSurfaceFilter
    {
        /// <summary>
        /// 補完なし。拡縮した際にジャギーが出ることがあります。
        /// </summary>
        Point,

        /// <summary>
        /// リニア補完
        /// </summary>
        Bilinear,

        /// <summary>
        /// トライリニア補完
        /// </summary>
        Trilinear
    }
}
