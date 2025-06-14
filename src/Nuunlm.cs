namespace Sharp;

public class Nuunlm
{
    private List<Texture> Textures = new List<Texture>();
    private AnimationData animationData { get; set; } = new AnimationData();
    private DrawData[][] framesByNumber;
    private Counter Counter;

    public void Load(string path)
    {
        animationData = Json.Load<AnimationData>(path);

        // テクスチャの読み込み
        foreach (var textureFileName in animationData.TextureFileNames)
        {
            string filePath = Path.Combine(Directory.GetParent(path).FullName, textureFileName);
            var texture = new Texture(filePath);
            Textures.Add(texture);
        }

        // 一時的に List<DrawData>[] を使って構築
        var tempFrames = new List<DrawData>[animationData.FrameLength + 1];
        for (int i = 0; i <= animationData.FrameLength; i++)
        {
            tempFrames[i] = new List<DrawData>();
        }

        // 描画データの追加
        foreach (var frame in animationData.nuunlm)
        {
            tempFrames[frame.FrameNumber].AddRange(frame.DrawObjects);
        }

        // 最終的な配列に変換
        framesByNumber = new DrawData[animationData.FrameLength + 1][];
        for (int i = 0; i <= animationData.FrameLength; i++)
        {
            framesByNumber[i] = tempFrames[i].ToArray();
        }
    }

    public void Start(bool isLoop)
    {
        Counter = new Counter(0, animationData.FrameLength, 16.6667, isLoop);
        Counter.Start();
    }

    public void Stop()
    {
        if (Counter == null) return;
        Counter.Stop();
        Counter.Reset();
    }

    public void Update()
    {
        if (Counter == null) return;
        Counter.Tick();
    }

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
