using System.Numerics;

namespace SharpEngine.Animation;

/// <summary>
/// 位置、拡大率、角度、透明度を表すクラス
/// </summary>
public class Transfrom
{
    /// <summary>
    /// 座標
    /// </summary>
    public Vector2 Position { get; set; } = new Vector2(0.0f, 0.0f);

    /// <summary>
    /// 拡大率
    /// </summary>
    public double ScaleX { get; set; } = 0.0f;
    public double ScaleY { get; set; } = 0.0f;

    /// <summary>
    /// 回転角度
    /// </summary>
    public double Rotation { get; set; } = 0.0f;

    /// <summary>
    /// 透明度
    /// </summary>
    public double Opacity { get; set; } = 1.0f;

    /// <summary>
    /// 反転するか X
    /// </summary>
    public bool ReverseX { get; set; } = false;

    /// <summary>
    /// 反転するか Y
    /// </summary>
    public bool ReverseY { get; set; } = false;
}
