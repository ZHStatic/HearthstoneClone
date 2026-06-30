using System.Collections.Generic;

/// <summary>
/// 在快照层粗略评估同一回合内的后续攻击收益。
/// 当前阶段模拟少量同回合出牌和攻击，不模拟对手回合。
/// </summary>
public static class SnapshotFollowUpEvaluator
{
    private const int MaxFollowUpDepth = 2;
    private const int MaxMinionsPerSide = 7;

    /// <summary>
    /// 从指定玩家视角评估一份快照，并尝试继续模拟少量同回合攻击动作。
    /// 返回当前快照和后续攻击分支中的最高评分。
    /// </summary>
    public static EvaluationResult EvaluateBestContinuation(
        GameStateSnapshot state,
        int playerIndex,
        Evaluator evaluator)
    {
        if (evaluator == null) return new EvaluationResult(0, 0, 0);

        int normalizedPlayerIndex = NormalizePlayerIndex(playerIndex);
        return EvaluateBestContinuation(state, normalizedPlayerIndex, evaluator, MaxFollowUpDepth);
    }

    private static EvaluationResult EvaluateBestContinuation(
        GameStateSnapshot state,
        int playerIndex,
        Evaluator evaluator,
        int remainingDepth)
    {
        EvaluationResult bestEvaluation = evaluator.EvaluateDetailed(state, playerIndex);
        if (state == null) return bestEvaluation;
        if (remainingDepth <= 0) return bestEvaluation;
        if (state.CurrentPlayerIndex != playerIndex) return bestEvaluation;

        List<SnapshotAction> followUpActions = GenerateFollowUpActions(state, playerIndex);
        for (int i = 0; i < followUpActions.Count; i++)
        {
            GameStateSnapshot simulatedState = SnapshotSimulator.Simulate(state, followUpActions[i]);
            EvaluationResult candidateEvaluation = EvaluateBestContinuation(
                simulatedState,
                playerIndex,
                evaluator,
                remainingDepth - 1);

            if (IsBetter(candidateEvaluation, bestEvaluation))
            {
                bestEvaluation = candidateEvaluation;
            }
        }

        return bestEvaluation;
    }

    private static List<SnapshotAction> GenerateFollowUpActions(GameStateSnapshot state, int actorIndex)
    {
        List<SnapshotAction> actions = new List<SnapshotAction>();
        if (state == null || state.Board == null) return actions;

        IReadOnlyList<MinionSnapshot> attackers = state.Board.GetMinions(actorIndex);
        IReadOnlyList<MinionSnapshot> targets = state.Board.GetMinions(GetOpponentIndex(actorIndex));
        PlayerSnapshot actor = GetPlayer(state, actorIndex);
        if (actor == null) return actions;

        AddPlayableCardActions(actions, state, actor, attackers, targets, actorIndex);
        AddAttackMinionActions(actions, attackers, targets, actorIndex);
        AddAttackHeroActions(actions, state, attackers, targets, actorIndex);
        AddHeroSkillActions(actions, state, actor, targets, actorIndex);

        return actions;
    }

    private static void AddPlayableCardActions(
        List<SnapshotAction> actions,
        GameStateSnapshot state,
        PlayerSnapshot actor,
        IReadOnlyList<MinionSnapshot> actorMinions,
        IReadOnlyList<MinionSnapshot> targets,
        int actorIndex)
    {
        if (actions == null) return;
        if (actor == null || actor.HandCards == null) return;

        for (int handCardIndex = 0; handCardIndex < actor.HandCards.Count; handCardIndex++)
        {
            CardSnapshot card = actor.HandCards[handCardIndex];
            if (!CanPlayCard(actor, card)) continue;

            switch (card.CardType)
            {
                case CardType.Minion:
                    AddPlayMinionAction(actions, card, actorMinions, actorIndex, handCardIndex);
                    break;
                case CardType.Spell:
                    AddSpellActions(actions, state, card, targets, actorIndex, handCardIndex);
                    break;
            }
        }
    }

    private static void AddPlayMinionAction(
        List<SnapshotAction> actions,
        CardSnapshot card,
        IReadOnlyList<MinionSnapshot> actorMinions,
        int actorIndex,
        int handCardIndex)
    {
        if (actions == null) return;
        if (card == null) return;
        if (actorMinions != null && actorMinions.Count >= MaxMinionsPerSide) return;

        actions.Add(SnapshotAction.CreatePlayMinion(
            actorIndex,
            card.Cost,
            handCardIndex,
            card.Attack,
            card.Health,
            card.HasTaunt,
            card.HasDivineShield,
            card.HasCharge,
            card.BattlecryType,
            card.BattlecryValue,
            card.DeathrattleType,
            card.DeathrattleValue));
    }

    private static void AddSpellActions(
        List<SnapshotAction> actions,
        GameStateSnapshot state,
        CardSnapshot card,
        IReadOnlyList<MinionSnapshot> targets,
        int actorIndex,
        int handCardIndex)
    {
        if (actions == null) return;
        if (card == null) return;
        if (card.SpellDamage <= 0) return;

        AddSpellOnMinionActions(actions, card, targets, actorIndex, handCardIndex);
        AddSpellOnHeroAction(actions, state, card, actorIndex, handCardIndex);
    }

    private static void AddSpellOnMinionActions(
        List<SnapshotAction> actions,
        CardSnapshot card,
        IReadOnlyList<MinionSnapshot> targets,
        int actorIndex,
        int handCardIndex)
    {
        if (actions == null) return;
        if (card == null || targets == null) return;
        if (!CanTargetEnemyMinion(card.SpellTargetType)) return;

        for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
        {
            MinionSnapshot target = targets[targetIndex];
            if (!IsAlive(target)) continue;

            actions.Add(SnapshotAction.CreateSpellOnMinion(
                actorIndex,
                card.Cost,
                handCardIndex,
                card.SpellDamage,
                targetIndex));
        }
    }

    private static void AddSpellOnHeroAction(
        List<SnapshotAction> actions,
        GameStateSnapshot state,
        CardSnapshot card,
        int actorIndex,
        int handCardIndex)
    {
        if (actions == null) return;
        if (card == null) return;
        if (!CanTargetEnemyHero(card.SpellTargetType)) return;
        if (state != null && !IsHeroAlive(GetPlayer(state, GetOpponentIndex(actorIndex)))) return;

        actions.Add(SnapshotAction.CreateSpellOnHero(
            actorIndex,
            card.Cost,
            handCardIndex,
            card.SpellDamage));
    }

    private static void AddAttackMinionActions(
        List<SnapshotAction> actions,
        IReadOnlyList<MinionSnapshot> attackers,
        IReadOnlyList<MinionSnapshot> targets,
        int actorIndex)
    {
        if (actions == null) return;
        if (attackers == null || targets == null) return;

        bool hasTaunt = HasAliveTauntMinion(targets);
        for (int attackerIndex = 0; attackerIndex < attackers.Count; attackerIndex++)
        {
            MinionSnapshot attacker = attackers[attackerIndex];
            if (!CanAttack(attacker)) continue;

            for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
            {
                MinionSnapshot target = targets[targetIndex];
                if (!IsAlive(target)) continue;
                if (hasTaunt && !target.HasTaunt) continue;

                actions.Add(SnapshotAction.CreateAttackMinion(actorIndex, attackerIndex, targetIndex));
            }
        }
    }

    private static void AddAttackHeroActions(
        List<SnapshotAction> actions,
        GameStateSnapshot state,
        IReadOnlyList<MinionSnapshot> attackers,
        IReadOnlyList<MinionSnapshot> targets,
        int actorIndex)
    {
        if (actions == null) return;
        if (state == null) return;
        if (attackers == null) return;
        if (HasAliveTauntMinion(targets)) return;
        if (!IsHeroAlive(GetPlayer(state, GetOpponentIndex(actorIndex)))) return;

        for (int attackerIndex = 0; attackerIndex < attackers.Count; attackerIndex++)
        {
            MinionSnapshot attacker = attackers[attackerIndex];
            if (!CanAttack(attacker)) continue;

            actions.Add(SnapshotAction.CreateAttackHero(actorIndex, attackerIndex));
        }
    }

    private static void AddHeroSkillActions(
        List<SnapshotAction> actions,
        GameStateSnapshot state,
        PlayerSnapshot actor,
        IReadOnlyList<MinionSnapshot> targets,
        int actorIndex)
    {
        if (actions == null) return;
        if (actor == null) return;
        if (actor.HasUsedHeroSkillThisTurn) return;
        if (actor.CurrentMana < Player.HeroSkillCost) return;

        AddHeroSkillOnMinionActions(actions, targets, actorIndex);
        AddHeroSkillOnHeroAction(actions, state, actorIndex);
    }

    private static void AddHeroSkillOnMinionActions(
        List<SnapshotAction> actions,
        IReadOnlyList<MinionSnapshot> targets,
        int actorIndex)
    {
        if (actions == null) return;
        if (targets == null) return;

        for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
        {
            MinionSnapshot target = targets[targetIndex];
            if (!IsAlive(target)) continue;

            actions.Add(SnapshotAction.CreateHeroSkillOnMinion(
                actorIndex,
                Player.HeroSkillDamage,
                targetIndex));
        }
    }

    private static void AddHeroSkillOnHeroAction(
        List<SnapshotAction> actions,
        GameStateSnapshot state,
        int actorIndex)
    {
        if (actions == null) return;
        if (state != null && !IsHeroAlive(GetPlayer(state, GetOpponentIndex(actorIndex)))) return;

        actions.Add(SnapshotAction.CreateHeroSkillOnHero(
            actorIndex,
            Player.HeroSkillDamage));
    }

    private static bool HasAliveTauntMinion(IReadOnlyList<MinionSnapshot> minions)
    {
        if (minions == null) return false;

        for (int i = 0; i < minions.Count; i++)
        {
            MinionSnapshot minion = minions[i];
            if (!IsAlive(minion)) continue;

            if (minion.HasTaunt)
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanAttack(MinionSnapshot minion)
    {
        return IsAlive(minion) && minion.CanAttack && minion.Attack > 0;
    }

    private static bool CanPlayCard(PlayerSnapshot actor, CardSnapshot card)
    {
        if (actor == null || card == null) return false;

        return card.Cost <= actor.CurrentMana;
    }

    private static bool CanTargetEnemyMinion(SpellTargetType targetType)
    {
        return targetType == SpellTargetType.AnyCharacter
            || targetType == SpellTargetType.EnemyCharacter
            || targetType == SpellTargetType.Minion
            || targetType == SpellTargetType.EnemyMinion;
    }

    private static bool CanTargetEnemyHero(SpellTargetType targetType)
    {
        return targetType == SpellTargetType.AnyCharacter
            || targetType == SpellTargetType.EnemyCharacter;
    }

    private static bool IsAlive(MinionSnapshot minion)
    {
        return minion != null && !minion.IsDead;
    }

    private static bool IsHeroAlive(PlayerSnapshot player)
    {
        return player != null && player.HeroHealth > 0;
    }

    private static PlayerSnapshot GetPlayer(GameStateSnapshot state, int playerIndex)
    {
        if (state == null) return null;

        return NormalizePlayerIndex(playerIndex) == GameStateSnapshot.EnemyIndex
            ? state.Enemy
            : state.Player;
    }

    private static bool IsBetter(EvaluationResult candidate, EvaluationResult currentBest)
    {
        if (candidate == null) return false;
        if (currentBest == null) return true;

        return candidate.TotalScore > currentBest.TotalScore;
    }

    private static int GetOpponentIndex(int playerIndex)
    {
        return NormalizePlayerIndex(playerIndex) == GameStateSnapshot.EnemyIndex
            ? GameStateSnapshot.PlayerIndex
            : GameStateSnapshot.EnemyIndex;
    }

    private static int NormalizePlayerIndex(int playerIndex)
    {
        return playerIndex == GameStateSnapshot.EnemyIndex
            ? GameStateSnapshot.EnemyIndex
            : GameStateSnapshot.PlayerIndex;
    }
}
