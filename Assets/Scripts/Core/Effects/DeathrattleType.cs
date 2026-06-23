/// <summary>
/// 亡语类型。
/// 当前阶段先只做最小亡语链路，用来理解“随从死亡后触发效果”的流程。
/// </summary>
public enum DeathrattleType
{
    /// <summary>
    /// 没有亡语。
    /// </summary>
    None,

    /// <summary>
    /// 亡语：对敌方英雄造成伤害。
    /// </summary>
    DealDamageToEnemyHero
}
