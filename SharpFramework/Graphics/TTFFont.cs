using Raylib_cs;
using System.Numerics;
using System.Reflection;

namespace SharpFramework.Graphics
{
    /// <summary>
    /// TrueTypeフォント（TTF）を読み込み、描画を提供するクラス。
    /// </summary>
    public class TTFFont : IDisposable
    {
        private byte[] _fontData; // 埋め込みリソースから読み込んだフォントデータ

        /// <summary> 読み込まれたRaylibのフォントオブジェクト </summary>
        public Font Font { get; private set; }

        /// <summary> フォントサイズ（ピクセル単位） </summary>
        public int FontSize { get; private set; }

        /// <summary>
        /// TTFFont の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="ttfPath">フォントファイルのパス</param>
        /// <param name="includeKanji">漢字を含めるかどうか</param>
        /// <param name="fontSize">フォントサイズ（デフォルトは48）</param>
        /// <param name="customCodePoints">追加で読み込むコードポイント</param>
        public TTFFont(string ttfPath, bool includeKanji, int fontSize = 48, int[] customCodePoints = null)
        {
            LoadFont(ttfPath, includeKanji, fontSize, customCodePoints);
            FontSize = fontSize;
        }

        internal TTFFont(string source, bool includeKanji, int fontSize = 48, int[] customCodePoints = null, bool isResource = false)
        {
            if (isResource)
                LoadFontFromResource(source, includeKanji, fontSize, customCodePoints);
            else
                LoadFont(source, includeKanji, fontSize, customCodePoints);

            FontSize = fontSize;
        }

        public void Dispose()
        {
            if (Font.BaseSize != 0)
            {
                Raylib.UnloadFont(Font);
            }
        }

        /// <summary>
        /// 指定位置にテキストを描画します。
        /// </summary>
        /// <param name="text">描画する文字列</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="referencePoint">基準点</param>
        /// <param name="color">文字色（省略時は黒）</param>
        /// <param name="spacing">文字間隔</param>
        public void DrawText(string text, float x, float y, ReferencePoint referencePoint = ReferencePoint.TopLeft, Color? color = null, float spacing = 2)
        {
            var actualColor = color ?? Color.Black;
            Vector2 size = MeasureText(text, spacing);
            Vector2 position = AdjustPositionByReferencePoint(x, y, size, referencePoint);
            Raylib.DrawTextEx(Font, text, position, FontSize, spacing, actualColor);
        }

        /// <summary>
        /// 指定したテキストのサイズ（幅と高さ）を測定します。
        /// </summary>
        public Vector2 MeasureText(string text, float spacing = 2)
        {
            return Raylib.MeasureTextEx(Font, text, FontSize, spacing);
        }

        /// <summary>
        /// テキスト描画の基準点に応じて描画位置を調整します。
        /// </summary>
        private Vector2 AdjustPositionByReferencePoint(float x, float y, Vector2 size, ReferencePoint referencePoint)
        {
            switch (referencePoint)
            {
                case ReferencePoint.TopLeft:
                    return new Vector2(x, y);
                case ReferencePoint.TopCenter:
                    return new Vector2(x - size.X / 2, y);
                case ReferencePoint.TopRight:
                    return new Vector2(x - size.X, y);
                case ReferencePoint.CenterLeft:
                    return new Vector2(x, y - size.Y / 2);
                case ReferencePoint.Center:
                    return new Vector2(x - size.X / 2, y - size.Y / 2);
                case ReferencePoint.CenterRight:
                    return new Vector2(x - size.X, y - size.Y / 2);
                case ReferencePoint.BottomLeft:
                    return new Vector2(x, y - size.Y);
                case ReferencePoint.BottomCenter:
                    return new Vector2(x - size.X / 2, y - size.Y);
                case ReferencePoint.BottomRight:
                    return new Vector2(x - size.X, y - size.Y);
                default:
                    return new Vector2(x, y);
            }
        }

        /// <summary>
        /// 外部ファイルからフォントを読み込みます。
        /// </summary>
        private void LoadFont(string fontPath, bool includeKanji, int fontSize, int[] customCodePoints = null)
        {
            var codepoints = GetCodepoints(includeKanji, customCodePoints);
            var codepointsArray = codepoints.ToArray();
            Font = Raylib.LoadFontEx(fontPath, fontSize, codepointsArray, codepointsArray.Length);
        }

        /// <summary>
        /// 埋め込みリソースからフォントを読み込みます。
        /// </summary>
        private void LoadFontFromResource(string resourceName, bool includeKanji, int fontSize, int[] customCodePoints = null)
        {
            _fontData = LoadEmbeddedResource(resourceName);

            if (_fontData == null)
            {
                throw new FileNotFoundException($"埋め込みリソース '{resourceName}' が見つかりません");
            }

            var codepoints = GetCodepoints(includeKanji, customCodePoints);
            var codepointsArray = codepoints.ToArray();

            unsafe
            {
                fixed (byte* fontDataPtr = _fontData)
                fixed (int* codepointsPtr = codepointsArray)
                {
                    var fileType = System.Text.Encoding.UTF8.GetBytes(".ttf\0");
                    fixed (byte* fileTypePtr = fileType)
                    {
                        Font = Raylib.LoadFontFromMemory((sbyte*)fileTypePtr, fontDataPtr, _fontData.Length, fontSize, codepointsPtr, codepointsArray.Length);
                    }
                }
            }
        }

        /// <summary>
        /// 指定された設定に応じて使用するUnicodeコードポイントを取得します。
        /// </summary>
        private IEnumerable<int> GetCodepoints(bool includeKanji, int[] customCodePoints)
        {
            var codepoints = Enumerable.Range(0x0020, 0x007F - 0x0020)       // ASCII
                .Concat(Enumerable.Range(0x3040, 0x309F - 0x3040 + 1))       // ひらがな
                .Concat(Enumerable.Range(0x30A0, 0x30FF - 0x30A0 + 1))       // カタカナ
                .Concat(Enumerable.Range(0xFF00, 0xFFEF - 0xFF00 + 1))       // 全角英数字・記号
                .Concat(new[] { 0x3005, 0x30FC });                           // 々、ーなど追加

            if (includeKanji)
            {
                codepoints = codepoints.Concat(Enumerable.Range(0x4E00, 0x9FFF - 0x4E00 + 1)); // 常用漢字
            }

            if (customCodePoints != null)
            {
                codepoints = codepoints.Concat(customCodePoints);
            }

            return codepoints;
        }

        /// <summary>
        /// 埋め込みリソースからバイト配列としてデータを取得します。
        /// </summary>
        private byte[] LoadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    // リソース名が見つからない場合、利用可能なリソースを確認
                    var availableResources = assembly.GetManifestResourceNames();
                    var fontResources = availableResources.Where(r => r.EndsWith(".ttf") || r.EndsWith(".otf"));

                    if (fontResources.Any())
                    {
                        // 最初に見つかったフォントリソースを使用
                        var firstFont = fontResources.First();
                        using (var fallbackStream = assembly.GetManifestResourceStream(firstFont))
                        {
                            if (fallbackStream != null)
                            {
                                byte[] resourceBuffer = new byte[fallbackStream.Length];
                                fallbackStream.Read(resourceBuffer, 0, resourceBuffer.Length);
                                return resourceBuffer;
                            }
                        }
                    }

                    return null;
                }

                byte[] streamBuffer = new byte[stream.Length];
                stream.Read(streamBuffer, 0, streamBuffer.Length);
                return streamBuffer;
            }
        }
    }
}
