using System.Numerics;

namespace SharpEngine.Animation;

/// <summary>
/// 画像オブジェクト
/// </summary>
public class ExoImageObject : ExoObject
{
    public ExoImageObject(ExoObject exoObject)
    {
        StartFrame = exoObject.StartFrame;
        EndFrame = exoObject.EndFrame;
        Layer = exoObject.Layer;
        Transfrom = new Transfrom();
    }

    public string FilePath { get; set; }

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
    /// 透明度
    /// </summary>
    public ExoOpacity Opacity { get; set; } = new ExoOpacity();

    /// <summary>
    /// ブレンドモード
    /// </summary>
    public int BlendMode { get; set; }

    /// <summary>
    /// グループ制御のリスト
    /// </summary>
    public List<ExoGroupObject> GroupObjects { get; set; } = new List<ExoGroupObject>();

    /// <summary>
    /// 上位グループ制御の影響を受けるかどうかのフラグ
    /// </summary>
    public bool IsAffectUpperGroup { get; set; }
}
