using System.Collections.Generic;

/// <summary>
/// AI 动作选择器。
/// 它只从已经生成好的合法动作里挑一条，不判断规则，也不执行动作。
/// </summary>
public class ActionSelector
{
    /// <summary>
    /// 从合法动作中选择一条动作。
    /// 第一版使用简单优先级：攻击英雄 > 攻击随从 > 施法 > 出随从 > 结束回合。
    /// </summary>
    public GameAction SelectAction(IReadOnlyList<GameAction> legalActions)
    {
        if (legalActions == null || legalActions.Count == 0) return null;

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
