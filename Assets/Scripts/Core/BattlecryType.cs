/// <summary>
/// 战吼类型。
/// 当前阶段先只做最小战吼链路，用来理解“随从被召唤后触发效果”的流程。
/// </summary>
public enum BattlecryType
{
    /// <summary>
    /// 没有战吼。
    /// </summary>
    None,

    /// <summary>
    /// 战吼：对敌方英雄造成伤害。
    /// </summary>
    DealDamageToEnemyHero,

    /// <summary>
    /// 战吼：为出牌者抽牌。
    /// </summary>
    DrawCard
}
