using Raylib_cs;
using System.Numerics;

namespace SharpEngine
{
    public class TTFFont : IDisposable
    {
        public Font Font { get; private set; }

        public TTFFont(string fontPath, bool includeKanji, int fontSize = 48, int[] customCodePoints = null)
        {
            LoadFont(fontPath, includeKanji, fontSize, customCodePoints);
        }

        public void Dispose()
        {
            if (Font.BaseSize != 0)
            {
                Raylib.UnloadFont(Font);
            }
        }

        public void DrawText(string text, float x, float y, float fontSize, float spacing, Color color)
        {
            Raylib.DrawTextEx(Font, text, new System.Numerics.Vector2(x, y), fontSize, spacing, color);
        }

        public Vector2 MeasureText(string text, float fontSize, float spacing)
        {
            return Raylib.MeasureTextEx(Font, text, fontSize, spacing);
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
