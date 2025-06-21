namespace SharpEngine.Animation;

/// <summary>
/// オブジェクトの基底クラス
/// </summary>
public class ExoObject
{
    /// <summary>
    /// 開始フレーム
    /// </summary>
    public int StartFrame { get; set; }

    /// <summary>
    /// 終了フレーム
    /// </summary>
    public int EndFrame { get; set; }

    /// <summary>
    /// レイヤー
    /// </summary>
    public int Layer { get; set; }

    /// <summary>
    /// フィルターのリスト
    /// </summary>
    public List<ExoFilter> Filters { get; set; } = new List<ExoFilter>();

    /// <summary>
    /// 位置、拡大率、角度
    /// </summary>
    public Transfrom Transfrom { get; set; }
}
