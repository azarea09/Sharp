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
        Point = 0,

        /// <summary>
        /// リニア補完
        /// </summary>
        Bilinear,

        /// <summary>
        /// トライリニア補完
        /// </summary>
        Trilinear,

        /// <summary>
        /// Anisotropic filtering 4x
        /// </summary>
        Anisotropic4X,

        /// <summary>
        /// Anisotropic filtering 8x
        /// </summary>
        Anisotropic8X,

        /// <summary>
        /// Anisotropic filtering 16x
        /// </summary>
        Anisotropic16X,
    }
}
