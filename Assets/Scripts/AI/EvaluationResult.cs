/// <summary>
/// AI 局面评估的明细结果。
/// 它只保存一次评分的各部分分数，方便日志解释和后续调权重。
/// </summary>
public class EvaluationResult
{
    /// <summary>
    /// 英雄血量差贡献的分数。
    /// </summary>
    public int HeroHealthScore { get; private set; }

    /// <summary>
    /// 手牌数量差贡献的分数。
    /// </summary>
    public int HandScore { get; private set; }

    /// <summary>
    /// 场面随从价值差贡献的分数。
    /// </summary>
    public int BoardScore { get; private set; }

    /// <summary>
    /// 当前局面的总分。
    /// </summary>
    public int TotalScore { get; private set; }

    public EvaluationResult(int heroHealthScore, int handScore, int boardScore)
    {
        HeroHealthScore = heroHealthScore;
        HandScore = handScore;
        BoardScore = boardScore;
        TotalScore = heroHealthScore + handScore + boardScore;
    }

    /// <summary>
    /// 转成适合 Unity Console 单行显示的短文本。
    /// </summary>
    public string ToDebugText()
    {
        return $"总分 {TotalScore}，英雄 {HeroHealthScore}，手牌 {HandScore}，场面 {BoardScore}";
    }
}
