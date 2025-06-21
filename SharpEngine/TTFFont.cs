using Raylib_cs;

namespace SharpEngine
{
    public class TTFFont
    {
        public Font Font { get; private set; } = new Font();

        public TTFFont(string fontPath, bool includeKanji, int fontSize = 48, int[] customCodePoints = null)
        {
            LoadFont(fontPath, includeKanji, fontSize, customCodePoints);
        }

        public void DrawText(string text, double x, double y, float fontSize, Color color)
        {
            Raylib.DrawTextEx(Font, text, new System.Numerics.Vector2((float)x, (float)y), fontSize, 1.0f, color);
        }

        private void LoadFont(string fontPath, bool includeKanji, int fontSize, int[] customCodePoints = null)
        {
            var codepoints = Enumerable.Range(0x0020, 0x007F - 0x0020)   // ASCII
                .Concat(Enumerable.Range(0x3040, 0x309F - 0x3040 + 1))   // ひらがな
                .Concat(Enumerable.Range(0x30A0, 0x30FF - 0x30A0 + 1));  // カタカナ

            if (includeKanji)
            {
                codepoints = codepoints.Concat(Enumerable.Range(0x4E00, 0x9FFF - 0x4E00 + 1)); // 常用漢字
            }

            if (customCodePoints != null)
            {
                codepoints = customCodePoints;
            }

            var codepointsArray = codepoints.ToArray();

            Font = Raylib.LoadFontEx(fontPath, fontSize, codepointsArray, codepointsArray.Length);
        }
    }
}
