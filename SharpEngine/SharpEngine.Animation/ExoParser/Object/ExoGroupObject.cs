namespace SharpEngine.Animation;

/// <summary>
/// グループ制御オブジェクト
/// </summary>
public class ExoGroupObject : ExoObject
{
    public ExoGroupObject(ExoObject exoObject)
    {
        StartFrame = exoObject.StartFrame;
        EndFrame = exoObject.EndFrame;
        Layer = exoObject.Layer;
        Transfrom = new Transfrom();
    }
    /// <summary>
    /// 位置
    /// </summary>
    public ExoPosition Position { get; set; } = new ExoPosition();

    /// <summary>
    /// 拡大率
    /// </summary>
    public ExoScale Scale { get; set; } = new ExoScale();

    /// <summary>
    /// 回転
    /// </summary>
    public ExoRotation Rotation { get; set; } = new ExoRotation();

    /// <summary>
    /// 上位グループ制御の影響を受けるか
    /// </summary>
    public bool AffectUpperGroup { get; set; }

    /// <summary>
    /// グループ制御の適応範囲
    /// </summary>
    public int Range { get; set; }
}
