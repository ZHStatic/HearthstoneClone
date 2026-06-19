/// <summary>
/// 卡牌关键词类型。
/// 当前阶段已经支持冲锋和嘲讽，用来验证“关键词数据 -> 随从生成 -> 规则生效”的最小链路。
/// </summary>
public enum KeywordType
{
    /// <summary>
    /// 没有关键词，用作默认值或占位值，不会作为有效关键词保存。
    /// </summary>
    None,

    /// <summary>
    /// 冲锋：随从被召唤后可以立刻攻击。
    /// </summary>
    Charge,

    /// <summary>
    /// 嘲讽：对方攻击时，必须优先攻击拥有嘲讽的随从。
    /// </summary>
    Taunt
}
