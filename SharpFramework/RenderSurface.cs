namespace SharpFramework
{
    /// <summary>
    /// ゲームの表示領域のサイズを持つクラス。
    /// </summary>
    public class RenderSurface
    {
        public static int Width = 1280;
        public static int Height = 720;

        public static void Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
