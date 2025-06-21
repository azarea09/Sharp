namespace SharpEngine.Animation;

/// <summary>
/// 回転フィルター
/// </summary>
class RotationFilter : ExoFilter
{
    /// <summary>
    /// 回転
    /// </summary>
    public ExoRotation Rotation { get; set; } = new ExoRotation();

}
