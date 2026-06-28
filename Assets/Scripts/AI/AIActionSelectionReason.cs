/// <summary>
/// AI 选择某个动作的原因。
/// 用于让 AIController 把选择依据打印成可读日志。
/// </summary>
public enum AIActionSelectionReason
{
    /// <summary>
    /// 没有明确原因，通常表示没有选中动作。
    /// </summary>
    None,

    /// <summary>
    /// 选择这个动作是因为它可以击杀敌方英雄。
    /// </summary>
    Lethal,

    /// <summary>
    /// 选择这个动作是因为它在不降低评分的主动动作中模拟后评分最高。
    /// </summary>
    HighestEvaluationScore,

    /// <summary>
    /// 选择这个动作是因为它只会造成可接受的小幅评分下降，用来换取节奏。
    /// </summary>
    AcceptableScoreLoss,

    /// <summary>
    /// 没有找到评分在可接受范围内的主动动作，所以选择结束回合。
    /// </summary>
    NoProfitableActionEndTurn,

    /// <summary>
    /// 没有命中特殊策略，按固定动作优先级兜底选择。
    /// </summary>
    FallbackPriority
}
