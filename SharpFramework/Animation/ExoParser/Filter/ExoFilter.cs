namespace SharpFramework.Animation;

/// <summary>
/// フィルターの基底クラス
/// </summary>
public class ExoFilter
{
    public ExoFilterType FilterType { get; set; }
}

public enum ExoFilterType
{
    None,
    Scale,
    Rotation,
    Opacity,
    Reverse
}
