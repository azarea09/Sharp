namespace SharpEngine.Animation;

/// <summary>
/// 透明度フィルター
/// </summary>
class OpacityFilter : ExoFilter
{
    /// <summary>
    /// 透明度
    /// </summary>
    public ExoOpacity Opacity { get; set; } = new ExoOpacity();
}
