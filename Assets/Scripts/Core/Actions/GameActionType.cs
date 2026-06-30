/// <summary>
/// 游戏动作类型。
/// 只描述“这是什么动作”，不负责判断动作是否合法，也不负责执行动作。
/// 当前只覆盖项目已经实现的核心动作：出牌、施法、攻击和结束回合。
/// </summary>
public enum GameActionType
{
    /// <summary>
    /// 未设置动作，通常作为默认值或占位。
    /// 生成合法动作时一般不会主动生成 None。
    /// </summary>
    None,

    /// <summary>
    /// 打出一张随从牌。
    /// 具体是否有法力、手牌归属是否正确、战场是否有空位，由规则判断决定。
    /// </summary>
    PlayMinionCard,

    /// <summary>
    /// 对一个随从释放法术。
    /// 当前用于单目标法术，目标保存在 GameAction.TargetMinion。
    /// </summary>
    PlaySpellOnMinion,

    /// <summary>
    /// 对一个英雄释放法术。
    /// 当前用于单目标法术，目标保存在 GameAction.TargetHero。
    /// </summary>
    PlaySpellOnHero,

    /// <summary>
    /// 让一个随从攻击另一个随从。
    /// 攻击者保存在 GameAction.Attacker，目标保存在 GameAction.TargetMinion。
    /// </summary>
    AttackMinion,

    /// <summary>
    /// 让一个随从攻击英雄。
    /// 攻击者保存在 GameAction.Attacker，目标保存在 GameAction.TargetHero。
    /// </summary>
    AttackHero,

    /// <summary>
    /// 对一个随从使用英雄技能。
    /// 当前第一版英雄技能是 2 费、每回合一次、对敌方角色造成 1 点伤害。
    /// 目标保存在 GameAction.TargetMinion。
    /// </summary>
    UseHeroSkillOnMinion,

    /// <summary>
    /// 对一个英雄使用英雄技能。
    /// 当前第一版英雄技能是 2 费、每回合一次、对敌方角色造成 1 点伤害。
    /// 目标保存在 GameAction.TargetHero。
    /// </summary>
    UseHeroSkillOnHero,

    /// <summary>
    /// 结束当前玩家的回合。
    /// 当前只记录动作意图，真正结束回合仍然由 GameManager.EndTurn() 执行。
    /// </summary>
    EndTurn
}
