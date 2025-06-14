using Raylib_cs;
using System.Numerics;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Sharp
{
    public class Texture : IDisposable
    {
        public bool IsEnable { get; private set; }
        public string FileName { get; private set; }
        public Texture2D RayTexture { get; private set; }

        private double _rotation;
        private double _scaleX;
        private double _scaleY;
        private bool _reversedX;
        private bool _reversedY;
        private Color _color;
        private ReferencePoint _referencePoint;
        private Rectangle? _sourceRect = null;
        private BlendState _nowBlendState;
        private BlendState _prevBlendState;


        public double GetScaleX() => _scaleX;
        public double GetScaleY() => _scaleY;
        public double GetRotation() => _rotation;
        public bool IsReversedX() => _reversedX;
        public bool IsReversedY() => _reversedY;
        public Color GetColor() => _color;

        public Texture()
        {
            _rotation = 0.0f;
            _scaleX = 1.0;
            _scaleY = 1.0;
            _referencePoint = ReferencePoint.TopLeft;
            _nowBlendState = BlendState.Alpha;
            _prevBlendState = BlendState.Alpha;
            _color = Color.White;
        }

        public Texture(string fileName) : this()
        {
            RayTexture = Raylib.LoadTexture(fileName);
            if (RayTexture.Id != 0)
            {
                IsEnable = true;
            }
            FileName = fileName;
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

        public Texture Colored(int r, int g, int b, int a)
        {
            this._color = Color.FromArgb(a, r, g, b);
            return this;
        }

        public Texture Colored(int a)
        {
            this._color = Color.FromArgb(a, 255, 255, 255);
            return this;
        }

        public Texture Origined(ReferencePoint refPoint)
        {
            this._referencePoint = refPoint;
            return this;
        }

        public Texture Blended(BlendState blendState)
        {
            this._nowBlendState = blendState;
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
            var color = new Raylib_cs.Color(_color.R, _color.G, _color.B, _color.A);

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

            if (_nowBlendState != _prevBlendState)
            {
                switch (_prevBlendState)
                {
                    case BlendState.Alpha:
                        Rlgl.SetBlendFactorsSeparate(Rlgl.SRC_ALPHA, Rlgl.ONE_MINUS_SRC_ALPHA, Rlgl.ONE, Rlgl.ONE_MINUS_SRC_ALPHA, Rlgl.FUNC_ADD, Rlgl.MAX);
                        break;
                    case BlendState.Additive:
                        Rlgl.SetBlendFactorsSeparate(Rlgl.SRC_ALPHA, Rlgl.ONE, Rlgl.SRC_ALPHA, Rlgl.ONE, Rlgl.FUNC_ADD, Rlgl.MAX);
                        break;
                    case BlendState.Subtract:
                        Rlgl.SetBlendFactorsSeparate(Rlgl.SRC_ALPHA, Rlgl.ONE, Rlgl.SRC_ALPHA, Rlgl.ONE, Rlgl.FUNC_REVERSE_SUBTRACT, Rlgl.MAX);
                        break;
                }
                Rlgl.SetBlendMode(BlendMode.CustomSeparate);
                // ブレンド状態が変わった場合は、前の状態を保存
                _prevBlendState = _nowBlendState;
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
    }
}
