using System.Collections.Generic;

/// <summary>
/// AI 动作选择器。
/// 它只从已经生成好的合法动作里挑一条，不判断规则，也不执行动作。
/// </summary>
public class ActionSelector
{
    /// <summary>
    /// 从合法动作中选择一条动作。
    /// 第一版策略：能斩杀就斩杀，能击杀随从就解场，再尝试出牌，最后结束回合。
    /// </summary>
    public GameAction SelectAction(IReadOnlyList<GameAction> legalActions)
    {
        if (legalActions == null || legalActions.Count == 0) return null;

        if (TryFindLethalAction(legalActions, out GameAction lethalAction))
        {
            return lethalAction;
        }

        if (TryFindKillMinionAction(legalActions, out GameAction killMinionAction))
        {
            return killMinionAction;
        }

        if (TryFindPlayableCardAction(legalActions, out GameAction playableCardAction))
        {
            return playableCardAction;
        }

        return SelectHighestPriorityAction(legalActions);
    }

    private bool TryFindLethalAction(IReadOnlyList<GameAction> legalActions, out GameAction selectedAction)
    {
        selectedAction = null;

        for (int i = 0; i < legalActions.Count; i++)
        {
            GameAction action = legalActions[i];
            if (!CanKillHero(action)) continue;

            selectedAction = action;
            return true;
        }

        return false;
    }

    private bool TryFindKillMinionAction(IReadOnlyList<GameAction> legalActions, out GameAction selectedAction)
    {
        selectedAction = null;

        for (int i = 0; i < legalActions.Count; i++)
        {
            GameAction action = legalActions[i];
            if (!CanKillMinion(action)) continue;

            selectedAction = action;
            return true;
        }

        return false;
    }

    private bool TryFindPlayableCardAction(IReadOnlyList<GameAction> legalActions, out GameAction selectedAction)
    {
        selectedAction = null;

        for (int i = 0; i < legalActions.Count; i++)
        {
            GameAction action = legalActions[i];
            if (action == null) continue;

            if (action.ActionType == GameActionType.PlayMinionCard ||
                action.ActionType == GameActionType.PlaySpellOnHero ||
                action.ActionType == GameActionType.PlaySpellOnMinion)
            {
                selectedAction = action;
                return true;
            }
        }

        return false;
    }

    private GameAction SelectHighestPriorityAction(IReadOnlyList<GameAction> legalActions)
    {
        GameAction selectedAction = null;
        int selectedPriority = int.MinValue;

        for (int i = 0; i < legalActions.Count; i++)
        {
            GameAction candidate = legalActions[i];
            int candidatePriority = GetActionPriority(candidate);

            if (selectedAction == null || candidatePriority > selectedPriority)
            {
                selectedAction = candidate;
                selectedPriority = candidatePriority;
            }
        }

        return selectedAction;
    }

    private bool CanKillHero(GameAction action)
    {
        if (action == null || action.TargetHero == null) return false;
        if (action.Actor != null && action.TargetHero == action.Actor.Hero) return false;

        int damage = GetHeroDamage(action);
        return damage > 0 && damage >= action.TargetHero.CurrentHealth;
    }

    private bool CanKillMinion(GameAction action)
    {
        if (action == null || action.TargetMinion == null) return false;
        if (action.Actor != null && action.TargetMinion.Owner == action.Actor) return false;

        int damage = GetMinionDamage(action);
        return damage > 0 && damage >= action.TargetMinion.CurrentHealth;
    }

    private int GetHeroDamage(GameAction action)
    {
        if (action == null) return 0;

        switch (action.ActionType)
        {
            case GameActionType.AttackHero:
                return action.Attacker != null ? action.Attacker.Attack : 0;
            case GameActionType.PlaySpellOnHero:
                return action.Card != null && action.Card.CardData != null
                    ? action.Card.CardData.SpellDamage
                    : 0;
            default:
                return 0;
        }
    }

    private int GetMinionDamage(GameAction action)
    {
        if (action == null) return 0;

        switch (action.ActionType)
        {
            case GameActionType.AttackMinion:
                return action.Attacker != null ? action.Attacker.Attack : 0;
            case GameActionType.PlaySpellOnMinion:
                return action.Card != null && action.Card.CardData != null
                    ? action.Card.CardData.SpellDamage
                    : 0;
            default:
                return 0;
        }
    }

    private int GetActionPriority(GameAction action)
    {
        if (action == null) return int.MinValue;

        return action.ActionType switch
        {
            GameActionType.AttackHero => 50,
            GameActionType.AttackMinion => 40,
            GameActionType.PlaySpellOnHero => 30,
            GameActionType.PlaySpellOnMinion => 25,
            GameActionType.PlayMinionCard => 20,
            GameActionType.EndTurn => 0,
            _ => -1,
        };
    }
}
