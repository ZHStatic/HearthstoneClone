using System.Collections.Generic;

/// <summary>
/// 根据当前局面枚举合法动作。
/// 它只读取 Core 状态并创建 GameAction，不执行动作，也不做 AI 决策。
/// 换句话说，它回答“当前玩家能做什么”，不回答“应该做什么”。
/// </summary>
public static class GameActionGenerator
{
    /// <summary>
    /// 生成当前行动玩家可以执行的动作。
    /// 返回的新列表只包含动作数据，调用方之后可以自己选择其中一条动作。
    /// 如果对局无效、游戏已结束或没有当前玩家，会返回空列表。
    /// </summary>
    public static List<GameAction> GenerateLegalActions(GameManager gameManager)
    {
        List<GameAction> actions = new List<GameAction>();

        if (gameManager == null) return actions;
        if (gameManager.IsGameOver) return actions;

        Player actor = gameManager.CurrentPlayer;
        if (actor == null) return actions;

        AddPlayableCardActions(actions, gameManager, actor);
        AddAttackActions(actions, gameManager, actor);
        AddHeroSkillActions(actions, gameManager, actor);
        AddEndTurnAction(actions, actor);

        return actions;
    }

    /// <summary>
    /// 扫描当前玩家手牌，生成可以打出的随从牌动作，以及可以选择合法目标的法术动作。
    /// 只调用 GameManager 的验证方法，不在这里复制规则。
    /// </summary>
    private static void AddPlayableCardActions(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor)
    {
        if (actions == null) return;
        if (gameManager == null) return;
        if (actor == null) return;

        foreach (Card card in actor.Hand)
        {
            if (gameManager.ValidatePlayMinionCard(card).Success)
            {
                actions.Add(GameAction.CreatePlayMinionCard(actor, card));
                continue;
            }

            if (!gameManager.ValidatePlaySpellCard(card).Success) continue;

            AddSpellTargetActions(actions, gameManager, actor, card);
        }
    }

    /// <summary>
    /// 为一张可以释放的法术牌生成所有合法目标动作。
    /// 当前支持目标随从和目标英雄，不处理无目标法术、AOE 或随机目标。
    /// </summary>
    private static void AddSpellTargetActions(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor,
        Card card)
    {
        if (actions == null) return;
        if (gameManager == null) return;
        if (actor == null || card == null || card.CardData == null) return;

        AddSpellTargetMinionActions(actions, gameManager, actor, card, actor);

        Player opponent = gameManager.GetOpponent(actor);
        AddSpellTargetMinionActions(actions, gameManager, actor, card, opponent);

        AddSpellTargetHeroAction(actions, gameManager, actor, card, actor.Hero);
        AddSpellTargetHeroAction(actions, gameManager, actor, card, opponent?.Hero);
    }

    /// <summary>
    /// 扫描一名玩家场上的随从，把能被当前法术选择的目标加入动作列表。
    /// targetOwner 可以是当前玩家，也可以是对手。
    /// </summary>
    private static void AddSpellTargetMinionActions(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor,
        Card card,
        Player targetOwner)
    {
        if (gameManager == null || gameManager.Board == null) return;
        if (actor == null) return;
        if (card == null || card.CardData == null) return;
        if (targetOwner == null) return;

        IReadOnlyList<Minion> minions = gameManager.Board.GetMinions(targetOwner);
        if (minions == null) return;

        foreach (Minion target in minions)
        {
            if (!gameManager.ValidateSpellTargetMinion(card.CardData, target).Success) continue;

            actions.Add(GameAction.CreatePlaySpellOnMinion(actor, card, target));
        }
    }

    /// <summary>
    /// 如果某个英雄是当前法术的合法目标，就加入对应的施法动作。
    /// </summary>
    private static void AddSpellTargetHeroAction(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor,
        Card card,
        Hero targetHero)
    {
        if (actions == null) return;
        if (gameManager == null) return;
        if (actor == null) return;
        if (card == null || card.CardData == null) return;
        if (!gameManager.ValidateSpellTargetHero(card.CardData, targetHero).Success) return;

        actions.Add(GameAction.CreatePlaySpellOnHero(actor, card, targetHero));
    }

    /// <summary>
    /// 扫描当前玩家场上的可攻击随从，生成攻击随从和攻击英雄动作。
    /// 这里只创建动作，不调用 GameManager.TryAttack...，所以不会造成伤害或消耗攻击机会。
    /// </summary>
    private static void AddAttackActions(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor)
    {
        if (actions == null) return;
        if (gameManager == null || gameManager.Board == null) return;
        if (actor == null) return;

        IReadOnlyList<Minion> attackers = gameManager.Board.GetMinions(actor);
        if (attackers == null) return;

        Player opponent = gameManager.GetOpponent(actor);
        if (opponent == null) return;

        foreach (Minion attacker in attackers)
        {
            if (!gameManager.ValidateAttack(attacker).Success) continue;

            AddAttackMinionActions(actions, gameManager, actor, attacker, opponent);
            AddAttackHeroAction(actions, gameManager, actor, attacker, opponent.Hero);
        }
    }

    /// <summary>
    /// 为一个可攻击随从生成所有合法的“攻击敌方随从”动作。
    /// 嘲讽限制由 GameManager.ValidateAttackTarget 处理。
    /// </summary>
    private static void AddAttackMinionActions(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor,
        Minion attacker,
        Player opponent)
    {
        if (gameManager == null || gameManager.Board == null) return;
        if (actor == null || opponent == null) return;

        IReadOnlyList<Minion> targets = gameManager.Board.GetMinions(opponent);
        if (targets == null) return;

        foreach (Minion target in targets)
        {
            if (!gameManager.ValidateAttackTarget(attacker, target).Success) continue;

            actions.Add(GameAction.CreateAttackMinion(actor, attacker, target));
        }
    }

    /// <summary>
    /// 如果敌方英雄可以被攻击，就生成“攻击英雄”动作。
    /// 当敌方有活着的嘲讽随从时，GameManager 会拒绝这个动作。
    /// </summary>
    private static void AddAttackHeroAction(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor,
        Minion attacker,
        Hero targetHero)
    {
        if (actions == null) return;
        if (gameManager == null) return;
        if (actor == null) return;
        if (!gameManager.ValidateAttackHeroTarget(attacker, targetHero).Success) return;

        actions.Add(GameAction.CreateAttackHero(actor, attacker, targetHero));
    }

    /// <summary>
    /// 生成当前玩家可以使用的英雄技能动作。
    /// 第一版英雄技能只能选择敌方随从或敌方英雄，不受嘲讽限制。
    /// </summary>
    private static void AddHeroSkillActions(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor)
    {
        if (actions == null) return;
        if (gameManager == null) return;
        if (actor == null) return;
        if (!gameManager.ValidateHeroSkill().Success) return;

        Player opponent = gameManager.GetOpponent(actor);
        if (opponent == null) return;

        AddHeroSkillTargetMinionActions(actions, gameManager, actor, opponent);
        AddHeroSkillTargetHeroAction(actions, gameManager, actor, opponent.Hero);
    }

    /// <summary>
    /// 为英雄技能生成所有合法的敌方随从目标动作。
    /// </summary>
    private static void AddHeroSkillTargetMinionActions(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor,
        Player opponent)
    {
        if (actions == null) return;
        if (gameManager == null || gameManager.Board == null) return;
        if (actor == null || opponent == null) return;

        IReadOnlyList<Minion> targets = gameManager.Board.GetMinions(opponent);
        if (targets == null) return;

        foreach (Minion target in targets)
        {
            if (!gameManager.ValidateHeroSkillTargetMinion(actor, target).Success) continue;

            actions.Add(GameAction.CreateUseHeroSkillOnMinion(actor, target));
        }
    }

    /// <summary>
    /// 如果敌方英雄是合法目标，就生成英雄技能打英雄动作。
    /// </summary>
    private static void AddHeroSkillTargetHeroAction(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor,
        Hero targetHero)
    {
        if (actions == null) return;
        if (gameManager == null) return;
        if (actor == null) return;
        if (!gameManager.ValidateHeroSkillTargetHero(actor, targetHero).Success) return;

        actions.Add(GameAction.CreateUseHeroSkillOnHero(actor, targetHero));
    }

    /// <summary>
    /// 结束回合始终是一个合法动作。
    /// 这里同样只生成动作，不调用 GameManager.EndTurn()。
    /// </summary>
    private static void AddEndTurnAction(List<GameAction> actions, Player actor)
    {
        if (actions == null) return;
        if (actor == null) return;

        actions.Add(GameAction.CreateEndTurn(actor));
    }
}
