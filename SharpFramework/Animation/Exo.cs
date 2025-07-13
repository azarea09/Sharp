using SharpFramework.Graphics;
using SharpFramework.Utils;

namespace SharpFramework.Animation
{
    /// <summary>
    /// AviutlのExo形式のアニメーションを扱うクラス。
    /// </summary>
    public class Exo : IDisposable
    {
        private List<Texture> Textures = new List<Texture>();
        private AnimationData animationData { get; set; } = new AnimationData();
        private DrawData[][] framesByNumber;
        private Counter Counter;

        public void Dispose()
        {
            if (Textures != null)
            {
                foreach (var texture in Textures)
                {
                    texture.Dispose();
                }
                Textures.Clear();
            }
        }

        public void Load(string exoPath)
        {
            #region [exo読み込み→nuulmオブジェクト生成]
            ExoParser exo = new ExoParser(exoPath);
            AnimationData animation = new AnimationData();
            animation.nuunlm = new List<FrameData>();

            animation.TextureFileNames = exo.textureFileNames;
            animation.FrameLength = exo.FrameLength - 1;

            for (int i = 0; i < exo.FrameLength; i++)
            {
                FrameData frame = new FrameData();
                frame.FrameNumber = i;
                frame.DrawObjects = new List<DrawData>();

                for (int j = 0; j < exo.imageObjects.Count; j++)
                {
                    var imageObject = exo.imageObjects[j];

                    if (i + 1 < imageObject.StartFrame || i + 1 > imageObject.EndFrame)
                    {
                        continue;
                    }

                    exo.UpdateTransform(imageObject, i + 1);
                    exo.ApplyFilter(imageObject, i + 1);
                    exo.ApplyGroupObject(imageObject, i + 1);

                    DrawData drawData = new DrawData();
                    drawData.TextureIdx = animation.TextureFileNames.IndexOf(imageObject.FilePath);
                    drawData.X = 960 + imageObject.Transfrom.Position.X;
                    drawData.Y = 540 + imageObject.Transfrom.Position.Y;
                    drawData.ScaleX = imageObject.Transfrom.ScaleX;
                    drawData.ScaleY = imageObject.Transfrom.ScaleY;
                    drawData.Rotation = imageObject.Transfrom.Rotation * (MathF.PI / 180);
                    drawData.Opacity = 255 * imageObject.Transfrom.Opacity;
                    drawData.ReverseX = imageObject.Transfrom.ReverseX;
                    drawData.ReverseY = imageObject.Transfrom.ReverseY;
                    drawData.BlendMode = imageObject.BlendMode;

                    frame.DrawObjects.Add(drawData);
                }


                animation.nuunlm.Add(frame);
            }

            #endregion

            // テクスチャの読み込み
            foreach (var textureFileName in animation.TextureFileNames)
            {
                string filePath = Path.Combine(Directory.GetParent(exoPath).FullName, textureFileName);
                var texture = new Texture(filePath);
                Textures.Add(texture);
            }

            // 一時的に List<DrawData>[] を使って構築
            var tempFrames = new List<DrawData>[animation.FrameLength + 1];
            for (int i = 0; i <= animation.FrameLength; i++)
            {
                tempFrames[i] = new List<DrawData>();
            }

            // 描画データの追加
            foreach (var frame in animation.nuunlm)
            {
                tempFrames[frame.FrameNumber].AddRange(frame.DrawObjects);
            }

            // 最終的な配列に変換
            framesByNumber = new DrawData[animation.FrameLength + 1][];
            for (int i = 0; i <= animation.FrameLength; i++)
            {
                framesByNumber[i] = tempFrames[i].ToArray();
            }

            animationData = animation; // アニメーションデータを設定
        }

        /// <summary>
        /// アニメーションの再生を開始します。
        /// </summary>
        /// <param name="isLoop">ループするか否か</param>
        public void Start(bool isLoop)
        {
            Counter = new Counter(0, animationData.FrameLength, 16.6666, isLoop);
            Counter.Start();
        }

        /// <summary>
        /// アニメーションの再生を停止します。
        /// </summary>
        public void Stop()
        {
            if (Counter == null) return;
            Counter.Stop();
            Counter.Reset();
        }

        /// <summary>
        /// アニメーションの更新を行います。
        /// </summary>
        public void Update()
        {
            if (Counter == null) return;
            Counter.Tick();
        }

        /// <summary>
        /// アニメーションを描画します。
        /// </summary>
        public void Draw(double offsetX = 0.0, double offsetY = 0.0, double opacity = 255, Texture texture = null)
        {
            if (Counter == null || !IsPlaying() || opacity <= 0.0) return;

            int currentFrame = (int)Counter.Value;

            // 該当フレームの描画データを取得して描画
            foreach (var drawData in framesByNumber[currentFrame])
            {
                texture = Textures[drawData.TextureIdx];
                if (drawData.ScaleX != 1.0 || drawData.ScaleY != 1.0) texture.Scaled(drawData.ScaleX, drawData.ScaleY);
                if (drawData.Rotation != 0.0) texture.Rotated(drawData.Rotation);
                texture.Blended(GetBlendMode(drawData.BlendMode));

                texture.Colored((int)(drawData.Opacity * (opacity / 255.0)));
                texture.Origined(ReferencePoint.Center);
                texture.Draw(drawData.X + offsetX, drawData.Y + offsetY);
            }
        }

        /// <summary>
        /// 今のフレーム番号を取得します。
        /// </summary>
        /// <returns></returns>
        public int GetNowFrame()
        {
            if (Counter == null) return 0;
            return (int)Counter.Value;
        }

        /// <summary>
        /// 再生しているかどうか。
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            if (Counter == null) return false;
            return Counter.Value < Counter.End && Counter.State == TimerState.Started;
        }

        private BlendState GetBlendMode(int blend)
        {
            return blend switch
            {
                0 => BlendState.Alpha,
                1 => BlendState.Additive,
                2 => BlendState.Subtract,
                _ => BlendState.Alpha
            };
        }
    }
    struct AnimationData
    {
        public List<FrameData> nuunlm { get; set; }
        public List<string> TextureFileNames { get; set; }
        public int FrameLength { get; set; }
    }

    struct FrameData
    {
        public int FrameNumber { get; set; }
        public List<DrawData> DrawObjects { get; set; }
    }

    struct DrawData
    {
        public int TextureIdx { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double Rotation { get; set; }
        public double Opacity { get; set; }
        public bool ReverseX { get; set; }
        public bool ReverseY { get; set; }
        public int BlendMode { get; set; }
    }
}
