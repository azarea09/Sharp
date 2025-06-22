using Raylib_cs;
using System.Numerics;
using System.Xml.Linq;
using Color = System.Drawing.Color;

namespace SharpEngine
{
    /// <summary>
    /// 太鼓の達人のアトラスフォントを扱うクラス。
    /// </summary>
    public class TaikoAtlasFont
    {
        private Texture _fontTexture;
        private Dictionary<int, GlyphInfo> GlyphData = new Dictionary<int, GlyphInfo>();
        private Dictionary<(string, double, double, bool, double), (double width, double height)> DimensionCache = new Dictionary<(string, double, double, bool, double), (double width, double height)>();
        private Dictionary<(string, double, Vector2?, Color?, Color?), Texture> TextCache = new();

        public TaikoAtlasFont(string texturePath, string xmlPath)
        {
            _fontTexture = new Texture(texturePath);

            XElement root = XElement.Load(xmlPath);
            var fontElement = root.Element("font")
                ?? throw new Exception($"XMLフォーマットが不正です: {xmlPath}");

            var glyphDict = new Dictionary<int, GlyphInfo>();
            float maxHeight = 0;

            // 並列処理を使用してグリフをロード
            var lockObject = new object();
            Parallel.ForEach(fontElement.Elements("glyph"), glyph =>
            {
                int index = int.Parse(glyph.Attribute("index")?.Value ?? "0");
                int height = int.Parse(glyph.Attribute("height")?.Value ?? "0");

                var glyphInfo = new GlyphInfo
                {
                    OffsetU = int.Parse(glyph.Attribute("offsetU")?.Value ?? "0"),
                    OffsetV = int.Parse(glyph.Attribute("offsetV")?.Value ?? "0"),
                    Width = int.Parse(glyph.Attribute("width")?.Value ?? "0"),
                    Height = height
                };

                lock (lockObject)
                {
                    glyphDict[index] = glyphInfo;
                    maxHeight = Math.Max(maxHeight, height);
                }
            });

            GlyphData = glyphDict;
        }

        /// <summary>
        /// 左上基準で描画
        /// </summary>
        public void Draw(double x, double y, string text, double edge = 0.0, Vector2? scale = null, double opacity = 255, ReferencePoint reference = ReferencePoint.TopLeft, Color? backColor = null, Color? foreColor = null)
        {
            if (text == null) return;

            var cacheKey = (text, edge, scale, backColor, foreColor);
            var (width, height) = GetTextDimensions(text);
            if (!TextCache.TryGetValue(cacheKey, out Texture texture) && width != 0 && height != 0)
            {
                // 初回描画時にキャッシュを作成
                texture = CreateCacheTexture(text);
                Raylib.SetTextureFilter(texture.RayTexture, TextureFilter.Bilinear);
                TextCache[cacheKey] = texture;
            }

            scale ??= new Vector2(1f);
            backColor ??= Color.Black;
            foreColor ??= Color.White;

            // 描画領域の幅と高さを取得
            var (textWidth, textHeight) = GetTextDimensions(text, scale.Value.X, scale.Value.Y, edge > 0.0, edge);

            // 基準に応じて座標を調整
            switch (reference)
            {
                case ReferencePoint.TopRight:
                    x -= textWidth;
                    break;
                case ReferencePoint.TopCenter:
                    x -= textWidth / 2;
                    break;
                case ReferencePoint.Center:
                    x -= textWidth / 2;
                    y -= textHeight / 2;
                    break;
                case ReferencePoint.BottomLeft:
                    y -= textHeight;
                    break;
                case ReferencePoint.BottomRight:
                    x -= textWidth;
                    y -= textHeight;
                    break;
                case ReferencePoint.BottomCenter:
                    x -= textWidth / 2;
                    y -= textHeight;
                    break;
                case ReferencePoint.TopLeft:
                default:
                    break; // そのまま
            }

            if (edge > 0.0)
            {
                for (int i = 0; i < 16; i++)
                {
                    double angle = i / 8.0f * Math.PI;
                    double x1 = Math.Cos(angle) * edge;
                    double y1 = Math.Sin(angle) * edge;

                    // スケール適用
                    x1 *= scale.Value.X;
                    y1 *= scale.Value.Y;

                    double countervalue = ((opacity >= 255.0) ? 255.0 : ((opacity < 0.0) ? 0.0 : opacity));
                    double edge_opacity = Easing.EaseIn(countervalue, 0.0, 255.0, Easing.CalcType.Quart);
                    texture.Colored(backColor.Value, edge_opacity);
                    texture.Scaled(scale.Value.X, scale.Value.Y);
                    texture.Draw(x + x1, y + y1);
                }
            }

            texture.Colored(foreColor.Value, opacity);
            texture.Scaled(scale.Value.X, scale.Value.Y);
            texture.Draw(x, y);
        }

        /// <summary>
        /// 白文字を描画
        /// </summary>
        public void DrawWhiteText(double x, double y, string text, double scaleX = 1.0, double scaleY = 1.0, double opacity = 255, Color? color = null)
        {
            var glyphDict = GlyphData;

            if (text != null)
            {
                // 文字本体を描画
                foreach (char c in text)
                {
                    int unicode = c;
                    if (!glyphDict.TryGetValue(unicode, out var glyph))
                    {
                        continue;
                    }

                    //_fontTexture.DrawMode = DrawState.Nearest;
                    _fontTexture.Colored(color.Value, opacity);
                    _fontTexture.Scaled(scaleX, scaleY);
                    _fontTexture.Blended(BlendState.PMA_Alpha);
                    _fontTexture.Croped(glyph.OffsetU, glyph.OffsetV, glyph.Width, glyph.Height);
                    _fontTexture.Draw(x, y);

                    x += glyph.Width * scaleX; // 次の文字の位置に進む
                }
            }

        }

        private Texture CreateCacheTexture(string text)
        {
            // キャッシュ用のテクスチャを作成
            var (width, height) = GetTextDimensions(text, 1.0, 1.0, false, 0.0);
            RenderTexture2D screen = Raylib.LoadRenderTexture((int)width, (int)height);

            Raylib.BeginTextureMode(screen);
            DrawWhiteText(0, 0, text, 1.0, 1.0, 255.0, Color.White);
            Raylib.EndTextureMode();

            return new Texture(screen);
        }



        /// <summary>
        /// 文字列全体の幅と高さを計算する関数
        /// </summary>
        public (double width, double height) GetTextDimensions(string text, double scaleX = 1.0, double scaleY = 1.0, bool outline = false, double edge = 0.0)
        {
            var key = (text ?? "", scaleX, scaleY, outline, edge);

            if (DimensionCache.TryGetValue(key, out var cachedResult))
            {
                return cachedResult;
            }

            double totalWidth = 0;
            double maxGlyphHeight = 0; // 文字の一番高い部分の高さを保存

            if (!string.IsNullOrEmpty(text))
            {
                var glyphDict = GlyphData;
                foreach (char c in text)
                {
                    if (!glyphDict.TryGetValue(c, out var glyph)) continue;
                    totalWidth += glyph.Width * scaleX;
                    maxGlyphHeight = Math.Max(maxGlyphHeight, glyph.Height);
                }

                if (outline)
                {
                    totalWidth += edge * 2 * scaleX;
                    maxGlyphHeight += edge * 2 * scaleY;
                }
            }

            var result = (totalWidth, maxGlyphHeight);
            DimensionCache[key] = result;

            return result;
        }

    }
}

public class GlyphInfo
{
    public int OffsetU { get; set; }
    public int OffsetV { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
