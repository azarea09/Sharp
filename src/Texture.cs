using Raylib_cs;
using System.Drawing;
using System.Numerics;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace SharpEngine
{
    public class Texture : IDisposable
    {
        public bool IsEnable { get; private set; }
        public string FileName { get; private set; }
        public Texture2D RayTexture { get; private set; }
        public double GetScaleX() => _scaleX;
        public double GetScaleY() => _scaleY;
        public double GetRotation() => _rotation;
        public double GetOpacity() => _color.A;
        public ReferencePoint GetReferencePoint() => _referencePoint;
        public Rectangle? GetSourceRect() => _sourceRect;
        public BlendState GetBlendState() => _blendState;
        public bool IsReversedX() => _reversedX;
        public bool IsReversedY() => _reversedY;
        public Color GetColor() => _color;

        private double _rotation;
        private double _scaleX;
        private double _scaleY;
        private bool _reversedX;
        private bool _reversedY;
        private Color _color;
        private ReferencePoint _referencePoint;
        private Rectangle? _sourceRect = null;
        private BlendState _blendState;

        public Texture()
        {
            _rotation = 0.0f;
            _scaleX = 1.0;
            _scaleY = 1.0;
            _referencePoint = ReferencePoint.TopLeft;
            _blendState = BlendState.Alpha;
            _color = Color.White;
        }

        public Texture(string fileName) : this()
        {
            RayTexture = Raylib.LoadTexture(fileName);
            if (RayTexture.Id != 0)
            {
                IsEnable = true;
                FileName = fileName;
            }
        }

        public Texture(RenderTexture2D renderTexture2D) : this()
        {
            RayTexture = renderTexture2D.Texture;
            if (RayTexture.Id != 0)
            {
                IsEnable = true;
                _reversedY = true;
            }
        }

        public void Dispose()
        {
            if (IsEnable)
            {
                Raylib.UnloadTexture(RayTexture);
                IsEnable = false;
            }
        }


        public Texture Scaled(double value)
        {
            this._scaleX = value;
            this._scaleY = value;
            return this;
        }
        public Texture Scaled(double x, double y)
        {
            this._scaleX = x;
            this._scaleY = y;
            return this;
        }

        public Texture ScaledX(double x)
        {
            this._scaleX = x;
            return this;
        }
        public Texture ScaledY(double y)
        {
            this._scaleY = y;
            return this;
        }

        public Texture Rotated(double angle)
        {
            this._rotation = angle;
            return this;
        }

        public Texture Reversed(bool reverseX, bool reverseY)
        {
            this._reversedX = reverseX;
            this._reversedY = reverseY;
            return this;
        }

        public Texture Croped(double x, double y, double width, double height)
        {
            this._sourceRect = new Rectangle((int)x, (int)y, (int)width, (int)height);
            return this;
        }

        public Texture Croped(Raylib_cs.Rectangle rectangle)
        {
            this._sourceRect = new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
            return this;
        }

        public Texture Croped(Rectangle rectangle)
        {
            this._sourceRect = rectangle;
            return this;
        }

        public Texture Colored(Color color, double opacity)
        {
            int alpha = Math.Clamp((int)opacity, 0, 255);
            this._color = Color.FromArgb(alpha, color.R, color.G, color.B);
            return this;
        }

        public Texture Colored(Color color)
        {
            this._color = Color.FromArgb(this._color.A, color.R, color.G, color.B);
            return this;
        }

        public Texture Colored(Raylib_cs.Color color, double opacity)
        {
            int alpha = Math.Clamp((int)opacity, 0, 255);
            this._color = Color.FromArgb(alpha, color.R, color.G, color.B);
            return this;
        }

        public Texture Colored(Raylib_cs.Color color)
        {
            this._color = Color.FromArgb(this._color.A, color.R, color.G, color.B);
            return this;
        }

        public Texture Colored(int r, int g, int b, int a)
        {
            int alpha = Math.Clamp((int)a, 0, 255);
            this._color = Color.FromArgb(alpha, r, g, b);
            return this;
        }

        public Texture Colored(double opacity)
        {
            int alpha = Math.Clamp((int)opacity, 0, 255);
            this._color = Color.FromArgb(alpha, this._color.R, this._color.G, this._color.B);
            return this;
        }

        public Texture Origined(ReferencePoint refPoint)
        {
            this._referencePoint = refPoint;
            return this;
        }

        public Texture Blended(BlendState blendState)
        {
            this._blendState = blendState;
            return this;
        }


        /// <summary>
        /// テクスチャを描画する (左上が X0, Y0 の座標系)
        /// </summary>
        public void Draw(double x, double y, Vector2? drawOrigin = null)
        {
            if (!IsEnable) return;

            var sourceRect = _sourceRect is Rectangle sr
                ? new Raylib_cs.Rectangle(sr.X, sr.Y, sr.Width, sr.Height)
                : new Raylib_cs.Rectangle(0, 0, RayTexture.Width, RayTexture.Height);

            // 反転処理
            if (_reversedX) sourceRect.Width = -sourceRect.Width;
            if (_reversedY) sourceRect.Height = -sourceRect.Height;

            // 色の変換
            var color = new Raylib_cs.Color(_color.R / 255f, _color.G / 255f, _color.B / 255f, _color.A / 255f);

            // origin の算出とスケール考慮
            Vector2 origin;
            if (drawOrigin.HasValue)
            {
                origin = drawOrigin.Value;
                origin.X *= (float)_scaleX;
                origin.Y *= (float)_scaleY;
            }
            else
            {
                origin = GetReferencePoint(sourceRect);
                origin.X *= (float)_scaleX;
                origin.Y *= (float)_scaleY;
            }

            switch (_blendState)
            {
                case BlendState.Alpha:
                    Rlgl.SetBlendMode(BlendMode.Alpha);
                    break;
                case BlendState.Additive:
                    Rlgl.SetBlendMode(BlendMode.Additive);
                    break;
                case BlendState.Subtract:
                    Rlgl.SetBlendMode(BlendMode.SubtractColors);
                    break;
                case BlendState.Multiply:
                    Rlgl.SetBlendMode(BlendMode.Multiplied);
                    break;
                case BlendState.Screen:
                    Rlgl.SetBlendFactorsSeparate(Rlgl.SRC_ALPHA, Rlgl.ONE, Rlgl.SRC_ALPHA, Rlgl.ONE_MINUS_SRC_ALPHA, Rlgl.FUNC_ADD, Rlgl.MAX);
                    Rlgl.SetBlendMode(BlendMode.CustomSeparate);
                    break;
                case BlendState.PMA_Alpha:
                    Rlgl.SetBlendMode(BlendMode.AlphaPremultiply);
                    break;
            }

            bool isSimpleDraw = _scaleX == 1.0 && _scaleY == 1.0 && _rotation == 0.0;

            if (isSimpleDraw)
            {
                Raylib.DrawTextureRec(RayTexture, sourceRect, new Vector2((float)x - origin.X, (float)y - origin.Y), color);
            }
            else
            {
                var destRect = new Raylib_cs.Rectangle((float)x, (float)y, sourceRect.Width * (float)_scaleX, sourceRect.Height * (float)_scaleY);
                Raylib.DrawTexturePro(RayTexture, sourceRect, destRect, origin, (float)_rotation, color);
            }

            Rlgl.SetBlendMode(BlendMode.Alpha);
        }

        /// <summary>
        /// 画面中央を基準に描画 (AviutlやYMM4の座標系)
        /// </summary>
        public void DrawAt(double x, double y, Vector2? drawOrigin = null)
        {
            _referencePoint = ReferencePoint.Center;
            Draw(1920 / 2 + x, 1080 / 2 + y, drawOrigin);
        }

        private Vector2 GetReferencePoint(Raylib_cs.Rectangle rect)
        {
            return _referencePoint switch
            {
                ReferencePoint.TopLeft => new Vector2(0, 0),
                ReferencePoint.TopCenter => new Vector2(rect.Width / 2, 0),
                ReferencePoint.TopRight => new Vector2(rect.Width, 0),
                ReferencePoint.CenterLeft => new Vector2(0, rect.Height / 2),
                ReferencePoint.Center => new Vector2(rect.Width / 2, rect.Height / 2),
                ReferencePoint.CenterRight => new Vector2(rect.Width, rect.Height / 2),
                ReferencePoint.BottomLeft => new Vector2(0, rect.Height),
                ReferencePoint.BottomCenter => new Vector2(rect.Width / 2, rect.Height),
                ReferencePoint.BottomRight => new Vector2(rect.Width, rect.Height),
                _ => new Vector2(0, 0),
            };
        }
    }

    /// <summary>
    /// 描画する基準点
    /// </summary>
    public enum ReferencePoint
    {
        TopLeft, TopCenter, TopRight,
        CenterLeft, Center, CenterRight,
        BottomLeft, BottomCenter, BottomRight
    }

    public enum BlendState
    {
        Alpha,
        Additive,
        Subtract,
        Screen,
        Multiply,
        PMA_Alpha,
    }
}
