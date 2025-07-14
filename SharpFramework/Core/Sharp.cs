using Raylib_cs;
using SharpFramework.Graphics;

namespace SharpFramework.Core
{
    /// <summary>
    /// Sharpの機能を提供するクラス
    /// </summary>
    public static class Sharp
    {
        /// <summary> 最大フレームレート </summary>
        public static int TargetFrameRate = 0;

        /// <summary> 垂直同期 </summary>
        public static bool Vsync = true;

        /// <summary> 背景色 </summary>
        public static Color BackGroundColor = Color.RayWhite;

        /// <summary> デフォルトフォント 大 (NotoSansCJKjp-Regular) </summary>
        public static TTFFont? DefaultFontBig;

        /// <summary> デフォルトフォント 中 (NotoSansCJKjp-Regular) </summary>
        public static TTFFont? DefaultFontMid;

        /// <summary> デフォルトフォント 小 (NotoSansCJKjp-Regular) </summary>
        public static TTFFont? DefaultFontSmall;

        /// <summary> フォントサイズ </summary>
        public enum FontSize
        {
            Big, Medium, Small
        }

        /// <summary> 最大フレームレートを設定する </summary>
        public static void SetTargetFrameRate(int targetFrameRate)
        {
            TargetFrameRate = targetFrameRate;
        }

        /// <summary> Vsyncを設定する (デフォルト true) </summary>
        public static void SetVsync(bool IsUseVsync)
        {
            Vsync = IsUseVsync;
        }

        /// <summary> 背景色を変更する (デフォルト RayWhite)。 </summary>
        public static void ChangeBackGroundColor(Color color)
        {
            BackGroundColor = color;
        }

        /// <summary> デフォルトフォントを使用してテキストを描画する。 解像度に合わせて自動でスケーリングされます。</summary>
        public static void DrawText(string text, int posX, int posY, FontSize fontSize = FontSize.Medium, ReferencePoint referencePoint = ReferencePoint.TopLeft, Color? color = null, float spacing = 2)
        {
            switch (fontSize)
            {
                case FontSize.Big:
                    DefaultFontBig?.DrawText(text, posX, posY, referencePoint, color, spacing);
                    break;
                case FontSize.Medium:
                    DefaultFontMid?.DrawText(text, posX, posY, referencePoint, color, spacing);
                    break;
                case FontSize.Small:
                    DefaultFontSmall?.DrawText(text, posX, posY, referencePoint, color, spacing);
                    break;
            }
        }

    }
}
