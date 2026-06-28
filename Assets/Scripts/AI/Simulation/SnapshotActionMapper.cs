using System.Collections.Generic;

/// <summary>
/// 把真实 GameAction 转成快照模拟用的 SnapshotAction。
/// 它只做数据映射，不验证动作是否合法，也不执行动作。
/// 第一版只映射当前模拟器能表达的动作；如果动作需要友方目标、复杂效果或找不到索引，会返回 false。
/// </summary>
public static class SnapshotActionMapper
{
    /// <summary>
    /// 尝试把一条真实动作映射成快照动作。
    /// 当前只映射第一版模拟器能表达的动作；无法表达时返回 false。
    /// </summary>
    public static bool TryMap(GameAction action, GameManager gameManager, out SnapshotAction snapshotAction)
    {
        snapshotAction = null;

        if (action == null) return false;
        if (gameManager == null) return false;
        if (!TryGetPlayerIndex(action.Actor, gameManager, out int actorIndex)) return false;

        switch (action.ActionType)
        {
            case GameActionType.PlayMinionCard:
                return TryMapPlayMinion(action, actorIndex, out snapshotAction);
            case GameActionType.PlaySpellOnMinion:
                return TryMapSpellOnMinion(action, gameManager, actorIndex, out snapshotAction);
            case GameActionType.PlaySpellOnHero:
                return TryMapSpellOnHero(action, gameManager, actorIndex, out snapshotAction);
            case GameActionType.AttackMinion:
                return TryMapAttackMinion(action, gameManager, actorIndex, out snapshotAction);
            case GameActionType.AttackHero:
                return TryMapAttackHero(action, gameManager, actorIndex, out snapshotAction);
            case GameActionType.EndTurn:
                snapshotAction = SnapshotAction.CreateEndTurn(actorIndex);
                return true;
            default:
                return false;
        }
    }

    private static bool TryMapPlayMinion(GameAction action, int actorIndex, out SnapshotAction snapshotAction)
    {
        snapshotAction = null;

        if (action.Card == null || action.Card.CardData == null) return false;

        // 召唤模拟只需要费用、攻击和生命值；战吼、亡语和关键词模拟后续再补。
        snapshotAction = SnapshotAction.CreatePlayMinion(
            actorIndex,
            action.Card.CurrentCost,
            action.Card.CardData.Attack,
            action.Card.CardData.Health);
        return true;
    }

    private static bool TryMapSpellOnMinion(GameAction action, GameManager gameManager, int actorIndex, out SnapshotAction snapshotAction)
    {
        snapshotAction = null;

        if (action.Card == null || action.Card.CardData == null) return false;
        if (action.TargetMinion == null) return false;

        // SnapshotAction 当前只记录“目标在对手场面中的索引”，所以友方目标暂时无法表达。
        Player opponent = gameManager.GetOpponent(action.Actor);
        int targetIndex = FindMinionIndex(gameManager.Board, opponent, action.TargetMinion);
        if (targetIndex < 0) return false;

        // 第一版只模拟伤害法术，直接复制 CardData.SpellDamage。
        snapshotAction = SnapshotAction.CreateSpellOnMinion(
            actorIndex,
            action.Card.CurrentCost,
            action.Card.CardData.SpellDamage,
            targetIndex);
        return true;
    }

    private static bool TryMapSpellOnHero(GameAction action, GameManager gameManager, int actorIndex, out SnapshotAction snapshotAction)
    {
        snapshotAction = null;

        if (action.Card == null || action.Card.CardData == null) return false;
        if (action.TargetHero == null) return false;

        // SnapshotAction 当前只支持把英雄目标理解为“敌方英雄”。
        // 如果以后加入治疗或友方 Buff，再扩展目标阵营字段。
        Player opponent = gameManager.GetOpponent(action.Actor);
        if (opponent == null || action.TargetHero != opponent.Hero) return false;

        snapshotAction = SnapshotAction.CreateSpellOnHero(
            actorIndex,
            action.Card.CurrentCost,
            action.Card.CardData.SpellDamage);
        return true;
    }

    private static bool TryMapAttackMinion(GameAction action, GameManager gameManager, int actorIndex, out SnapshotAction snapshotAction)
    {
        snapshotAction = null;

        if (action.Attacker == null || action.TargetMinion == null) return false;

        // 攻击模拟需要两个索引：攻击者在己方场面，目标在对手场面。
        Player opponent = gameManager.GetOpponent(action.Actor);
        int attackerIndex = FindMinionIndex(gameManager.Board, action.Actor, action.Attacker);
        int targetIndex = FindMinionIndex(gameManager.Board, opponent, action.TargetMinion);
        if (attackerIndex < 0 || targetIndex < 0) return false;

        snapshotAction = SnapshotAction.CreateAttackMinion(actorIndex, attackerIndex, targetIndex);
        return true;
    }

    private static bool TryMapAttackHero(GameAction action, GameManager gameManager, int actorIndex, out SnapshotAction snapshotAction)
    {
        snapshotAction = null;

        if (action.Attacker == null || action.TargetHero == null) return false;

        Player opponent = gameManager.GetOpponent(action.Actor);
        if (opponent == null || action.TargetHero != opponent.Hero) return false;

        // 攻击英雄只需要找到攻击者在己方场面里的索引。
        int attackerIndex = FindMinionIndex(gameManager.Board, action.Actor, action.Attacker);
        if (attackerIndex < 0) return false;

        snapshotAction = SnapshotAction.CreateAttackHero(actorIndex, attackerIndex);
        return true;
    }

    private static bool TryGetPlayerIndex(Player player, GameManager gameManager, out int playerIndex)
    {
        playerIndex = GameStateSnapshot.PlayerIndex;

        if (player == null || gameManager == null) return false;

        // 快照层用 0/1 表示双方，不能直接保存真实 Player 引用。
        if (player == gameManager.Player)
        {
            playerIndex = GameStateSnapshot.PlayerIndex;
            return true;
        }

        if (player == gameManager.Enemy)
        {
            playerIndex = GameStateSnapshot.EnemyIndex;
            return true;
        }

        return false;
    }

    private static int FindMinionIndex(Board board, Player owner, Minion target)
    {
        if (board == null || owner == null || target == null) return -1;

        IReadOnlyList<Minion> minions = board.GetMinions(owner);
        if (minions == null) return -1;

        // 用引用相等查找真实随从在当前真实场面中的位置，再把位置交给快照动作。
        for (int i = 0; i < minions.Count; i++)
        {
            if (minions[i] == target)
            {
                return i;
            }
        }

        return -1;
    }
}
