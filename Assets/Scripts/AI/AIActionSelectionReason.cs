/// <summary>
/// AI 选择某个动作的原因。
/// 当前只描述规则型 AI 的简单决策来源，不代表局面评分。
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
    /// 选择这个动作是因为它可以击杀敌方随从。
    /// </summary>
    KillEnemyMinion,

    /// <summary>
    /// 选择这个动作是因为当前没有击杀机会，先打出一张可用手牌。
    /// </summary>
    PlayCard,

    /// <summary>
    /// 没有命中特殊策略，按固定动作优先级兜底选择。
    /// </summary>
    FallbackPriority
}
