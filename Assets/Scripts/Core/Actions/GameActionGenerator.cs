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
        AddEndTurnAction(actions, actor);

        return actions;
    }

    /// <summary>
    /// 扫描当前玩家手牌，生成可以打出的随从牌动作，以及可以选择合法目标的法术动作。
    /// 这里只做只读判断，不调用 GameManager.TryPlay...，所以不会扣法力或移除手牌。
    /// </summary>
    private static void AddPlayableCardActions(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor)
    {
        if (actions == null) return;
        if (actor == null) return;

        foreach (Card card in actor.Hand)
        {
            if (CanPlayMinionCard(gameManager, actor, card))
            {
                actions.Add(GameAction.CreatePlayMinionCard(actor, card));
                continue;
            }

            if (!CanPlaySpellCard(gameManager, actor, card)) continue;

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

        AddSpellTargetHeroAction(actions, gameManager, actor, card, actor?.Hero);
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
        if (targetOwner == null) return;

        IReadOnlyList<Minion> minions = gameManager.Board.GetMinions(targetOwner);
        if (minions == null) return;

        foreach (Minion target in minions)
        {
            if (!CanTargetMinion(gameManager, actor, card.CardData, target)) continue;

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
        if (!CanTargetHero(gameManager, actor, card.CardData, targetHero)) return;

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
            if (!CanAttack(gameManager, actor, attacker)) continue;

            AddAttackMinionActions(actions, gameManager, actor, attacker, opponent);
            AddAttackHeroAction(actions, gameManager, actor, attacker, opponent.Hero);
        }
    }

    /// <summary>
    /// 为一个可攻击随从生成所有合法的“攻击敌方随从”动作。
    /// 嘲讽限制会在 IsValidAttackTarget 中处理。
    /// </summary>
    private static void AddAttackMinionActions(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor,
        Minion attacker,
        Player opponent)
    {
        IReadOnlyList<Minion> targets = gameManager.Board.GetMinions(opponent);
        if (targets == null) return;

        foreach (Minion target in targets)
        {
            if (!IsValidAttackTarget(gameManager, actor, attacker, target)) continue;

            actions.Add(GameAction.CreateAttackMinion(actor, attacker, target));
        }
    }

    /// <summary>
    /// 如果敌方英雄可以被攻击，就生成“攻击英雄”动作。
    /// 当敌方有活着的嘲讽随从时，不能生成攻击英雄动作。
    /// </summary>
    private static void AddAttackHeroAction(
        List<GameAction> actions,
        GameManager gameManager,
        Player actor,
        Minion attacker,
        Hero targetHero)
    {
        if (!CanAttack(gameManager, actor, attacker)) return;
        if (targetHero == null) return;
        if (targetHero.IsDead) return;

        Player opponent = gameManager.GetOpponent(actor);
        if (opponent == null) return;
        if (targetHero != opponent.Hero) return;
        if (HasAliveTauntMinion(gameManager, opponent)) return;

        actions.Add(GameAction.CreateAttackHero(actor, attacker, targetHero));
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

    /// <summary>
    /// 出牌通用只读检查。
    /// 检查游戏是否仍在进行、是否轮到 actor、卡牌是否在手牌中，以及法力是否足够。
    /// </summary>
    private static bool CanPlayCard(GameManager gameManager, Player actor, Card card)
    {
        if (gameManager == null) return false;
        if (gameManager.IsGameOver) return false;
        if (actor == null) return false;
        if (actor != gameManager.CurrentPlayer) return false;
        if (card == null || card.CardData == null) return false;
        if (!actor.HasCardInHand(card)) return false;
        if (card.CurrentCost > actor.CurrentMana) return false;

        return true;
    }

    /// <summary>
    /// 检查一张手牌能否作为随从牌打出。
    /// 除通用出牌条件外，还要求卡牌类型是 Minion，且当前玩家战场有空位。
    /// </summary>
    private static bool CanPlayMinionCard(GameManager gameManager, Player actor, Card card)
    {
        if (!CanPlayCard(gameManager, actor, card)) return false;
        if (card.CardData.CardType != CardType.Minion) return false;
        if (gameManager.Board == null) return false;
        if (!gameManager.Board.CanSummon(actor)) return false;

        return true;
    }

    /// <summary>
    /// 检查一张手牌能否作为法术牌释放。
    /// 这里只检查法术牌本身，不检查具体目标。
    /// </summary>
    private static bool CanPlaySpellCard(GameManager gameManager, Player actor, Card card)
    {
        if (!CanPlayCard(gameManager, actor, card)) return false;

        return card.CardData.CardType == CardType.Spell;
    }

    /// <summary>
    /// 检查某个随从是否能成为当前法术的目标。
    /// 规则来自 SpellTargetType，例如任意角色、敌方随从、友方随从等。
    /// </summary>
    private static bool CanTargetMinion(
        GameManager gameManager,
        Player actor,
        CardData spellData,
        Minion target)
    {
        if (gameManager == null) return false;
        if (spellData == null || spellData.CardType != CardType.Spell) return false;
        if (actor == null) return false;
        if (target == null || target.IsDead) return false;

        Player targetOwner = target.Owner;
        if (targetOwner != gameManager.Player && targetOwner != gameManager.Enemy) return false;

        Player opponent = gameManager.GetOpponent(actor);
        switch (spellData.SpellTargetType)
        {
            case SpellTargetType.AnyCharacter:
            case SpellTargetType.Minion:
                return true;
            case SpellTargetType.EnemyCharacter:
            case SpellTargetType.EnemyMinion:
                return targetOwner == opponent;
            case SpellTargetType.FriendlyCharacter:
            case SpellTargetType.FriendlyMinion:
                return targetOwner == actor;
            default:
                return false;
        }
    }

    /// <summary>
    /// 检查某个英雄是否能成为当前法术的目标。
    /// 英雄目标只接受 AnyCharacter、EnemyCharacter 和 FriendlyCharacter。
    /// </summary>
    private static bool CanTargetHero(
        GameManager gameManager,
        Player actor,
        CardData spellData,
        Hero targetHero)
    {
        if (gameManager == null) return false;
        if (spellData == null || spellData.CardType != CardType.Spell) return false;
        if (actor == null) return false;
        if (targetHero == null || targetHero.IsDead) return false;

        Player targetOwner = GetHeroOwner(gameManager, targetHero);
        if (targetOwner == null) return false;

        Player opponent = gameManager.GetOpponent(actor);
        return spellData.SpellTargetType switch
        {
            SpellTargetType.AnyCharacter => true,
            SpellTargetType.EnemyCharacter => targetOwner == opponent,
            SpellTargetType.FriendlyCharacter => targetOwner == actor,
            _ => false,
        };
    }

    /// <summary>
    /// 检查一个随从是否能作为当前动作的攻击者。
    /// 它必须属于当前行动玩家、没有死亡，并且本回合仍然可以攻击。
    /// </summary>
    private static bool CanAttack(GameManager gameManager, Player actor, Minion attacker)
    {
        if (gameManager == null) return false;
        if (gameManager.IsGameOver) return false;
        if (actor == null) return false;
        if (actor != gameManager.CurrentPlayer) return false;
        if (attacker == null) return false;
        if (attacker.Owner != actor) return false;
        if (attacker.IsDead) return false;
        if (!attacker.CanAttack) return false;

        return true;
    }

    /// <summary>
    /// 检查某个敌方随从是否是合法攻击目标。
    /// 如果敌方有活着的嘲讽随从，只能攻击带 Taunt 的随从。
    /// </summary>
    private static bool IsValidAttackTarget(
        GameManager gameManager,
        Player actor,
        Minion attacker,
        Minion target)
    {
        if (!CanAttack(gameManager, actor, attacker)) return false;
        if (target == null || target.IsDead) return false;

        Player opponent = gameManager.GetOpponent(actor);
        if (opponent == null) return false;
        if (target.Owner != opponent) return false;

        if (HasAliveTauntMinion(gameManager, opponent))
        {
            return target.HasKeyword(KeywordType.Taunt);
        }

        return true;
    }

    /// <summary>
    /// 检查指定玩家场上是否存在活着的嘲讽随从。
    /// 这个判断会影响攻击目标枚举，但不影响法术目标枚举。
    /// </summary>
    private static bool HasAliveTauntMinion(GameManager gameManager, Player owner)
    {
        if (gameManager == null || gameManager.Board == null) return false;
        if (owner == null) return false;

        IReadOnlyList<Minion> minions = gameManager.Board.GetMinions(owner);
        if (minions == null) return false;

        foreach (Minion minion in minions)
        {
            if (minion == null) continue;
            if (minion.IsDead) continue;
            if (minion.HasKeyword(KeywordType.Taunt)) return true;
        }

        return false;
    }

    /// <summary>
    /// 根据 Hero 引用反查它属于哪名玩家。
    /// 用于判断法术英雄目标是友方还是敌方。
    /// </summary>
    private static Player GetHeroOwner(GameManager gameManager, Hero hero)
    {
        if (gameManager == null || hero == null) return null;
        if (gameManager.Player != null && hero == gameManager.Player.Hero) return gameManager.Player;
        if (gameManager.Enemy != null && hero == gameManager.Enemy.Hero) return gameManager.Enemy;

        return null;
    }
}
