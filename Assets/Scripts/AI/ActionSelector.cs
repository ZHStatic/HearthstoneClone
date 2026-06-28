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
    /// 这里接收的动作必须已经由 GameActionGenerator 验证过合法性。
    /// </summary>
    public AIActionSelection SelectAction(IReadOnlyList<GameAction> legalActions)
    {
        if (legalActions == null || legalActions.Count == 0) return null;

        if (TryFindLethalAction(legalActions, out GameAction lethalAction))
        {
            return new AIActionSelection(lethalAction, AIActionSelectionReason.Lethal);
        }

        if (TryFindKillMinionAction(legalActions, out GameAction killMinionAction))
        {
            return new AIActionSelection(killMinionAction, AIActionSelectionReason.KillEnemyMinion);
        }

        if (TryFindPlayableCardAction(legalActions, out GameAction playableCardAction))
        {
            return new AIActionSelection(playableCardAction, AIActionSelectionReason.PlayCard);
        }

        GameAction fallbackAction = SelectHighestPriorityAction(legalActions);
        return new AIActionSelection(fallbackAction, AIActionSelectionReason.FallbackPriority);
    }

    /// <summary>
    /// 查找能直接击杀敌方英雄的动作。
    /// 当前只做伤害和血量的简单比较，不模拟圣盾、治疗或后续连锁效果。
    /// </summary>
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

    /// <summary>
    /// 查找能直接击杀敌方随从的动作。
    /// 这是阶段性简化：只判断本次动作能否造成足够伤害，不计算随从交换是否亏赚。
    /// </summary>
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

    /// <summary>
    /// 查找可以打出的手牌动作。
    /// 费用、目标、战场格子等合法性已经由 GameActionGenerator 处理。
    /// AI 选择时还要排除“合法但明显伤害自己”的目标，例如把伤害法术打到自己英雄或自己随从。
    /// 如果有多张牌可以打，优先选择费用更高的牌，让 AI 更接近“尽量利用当前法力”的行为。
    /// </summary>
    private bool TryFindPlayableCardAction(IReadOnlyList<GameAction> legalActions, out GameAction selectedAction)
    {
        selectedAction = null;
        int selectedScore = int.MinValue;

        for (int i = 0; i < legalActions.Count; i++)
        {
            GameAction action = legalActions[i];
            if (action == null) continue;
            if (!IsUsefulPlayableCardAction(action)) continue;

            int actionScore = GetPlayableCardScore(action);
            if (selectedAction == null || actionScore > selectedScore)
            {
                selectedAction = action;
                selectedScore = actionScore;
            }
        }

        return selectedAction != null;
    }

    /// <summary>
    /// 判断一条出牌动作是否值得 AI 主动选择。
    /// 当前阶段只支持随从牌和伤害法术，所以伤害法术默认只打敌方目标。
    /// </summary>
    private bool IsUsefulPlayableCardAction(GameAction action)
    {
        if (action == null) return false;

        return action.ActionType switch
        {
            GameActionType.PlayMinionCard => true,
            GameActionType.PlaySpellOnHero => !IsTargetOwnedByActor(action),
            GameActionType.PlaySpellOnMinion => !IsTargetOwnedByActor(action),
            _ => false,
        };
    }

    /// <summary>
    /// 给可出牌动作一个简单分数。
    /// 当前不是完整评估函数，只用费用近似节奏价值；同费用时优先打随从。
    /// 对非击杀伤害法术，优先打敌方英雄，避免因为动作生成顺序而随便打高血随从。
    /// </summary>
    private int GetPlayableCardScore(GameAction action)
    {
        if (action == null || action.Card == null) return int.MinValue;

        int costScore = action.Card.CurrentCost * 10;
        int typeScore = action.ActionType == GameActionType.PlayMinionCard ? 3 : 0;
        int targetScore = GetPlayableTargetScore(action);

        return costScore + typeScore + targetScore;
    }

    /// <summary>
    /// 给出牌动作的目标一个简单分数。
    /// 斩杀和击杀随从已经在更高优先级处理；这里主要处理“不能击杀时打谁更合理”。
    /// </summary>
    private int GetPlayableTargetScore(GameAction action)
    {
        if (action == null) return 0;

        return action.ActionType switch
        {
            GameActionType.PlaySpellOnHero => 2,
            GameActionType.PlaySpellOnMinion => 1,
            _ => 0,
        };
    }

    /// <summary>
    /// 兜底选择：如果没有命中斩杀、解场或出牌策略，就按固定优先级选一条动作。
    /// 这样可以保证 AI 至少会从合法动作中选到一个确定结果。
    /// </summary>
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

    /// <summary>
    /// 判断这条动作是否能击杀目标英雄。
    /// 如果目标是自己的英雄，直接返回 false，避免未来“任意目标法术”误判为斩杀动作。
    /// </summary>
    private bool CanKillHero(GameAction action)
    {
        if (action == null || action.TargetHero == null) return false;
        if (IsTargetOwnedByActor(action)) return false;

        int damage = GetHeroDamage(action);
        return damage > 0 && damage >= action.TargetHero.CurrentHealth;
    }

    /// <summary>
    /// 判断这条动作是否能击杀目标随从。
    /// 如果目标是自己的随从，直接返回 false，避免 AI 把自伤当成解场。
    /// </summary>
    private bool CanKillMinion(GameAction action)
    {
        if (action == null || action.TargetMinion == null) return false;
        if (IsTargetOwnedByActor(action)) return false;

        int damage = GetMinionDamage(action);
        return damage > 0 && damage >= action.TargetMinion.CurrentHealth;
    }

    /// <summary>
    /// 判断动作目标是否属于动作发起者。
    /// 用于避免 AI 把伤害打到自己英雄或自己随从身上。
    /// </summary>
    private bool IsTargetOwnedByActor(GameAction action)
    {
        if (action == null || action.Actor == null) return false;

        if (action.TargetHero != null && action.TargetHero == action.Actor.Hero) return true;
        if (action.TargetMinion != null && action.TargetMinion.Owner == action.Actor) return true;

        return false;
    }

    /// <summary>
    /// 估算动作对英雄造成的伤害。
    /// 当前只覆盖随从攻击英雄和单体伤害法术打英雄两类动作。
    /// </summary>
    private int GetHeroDamage(GameAction action)
    {
        if (action == null) return 0;

        return action.ActionType switch
        {
            GameActionType.AttackHero => action.Attacker != null ? action.Attacker.Attack : 0,
            GameActionType.PlaySpellOnHero => action.Card != null && action.Card.CardData != null
                ? action.Card.CardData.SpellDamage
                : 0,
            _ => 0,
        };
    }

    /// <summary>
    /// 估算动作对随从造成的伤害。
    /// 当前只覆盖随从攻击随从和单体伤害法术打随从两类动作。
    /// </summary>
    private int GetMinionDamage(GameAction action)
    {
        if (action == null) return 0;

        return action.ActionType switch
        {
            GameActionType.AttackMinion => action.Attacker != null ? action.Attacker.Attack : 0,
            GameActionType.PlaySpellOnMinion => action.Card != null && action.Card.CardData != null
                ? action.Card.CardData.SpellDamage
                : 0,
            _ => 0,
        };
    }

    /// <summary>
    /// 返回动作的兜底优先级。
    /// 分数只用于排序，不代表局面评分；真正的评估函数会在阶段 3.3 再做。
    /// </summary>
    private int GetActionPriority(GameAction action)
    {
        if (action == null) return int.MinValue;

        return action.ActionType switch
        {
            GameActionType.AttackHero => 50,
            GameActionType.AttackMinion => 40,
            GameActionType.PlaySpellOnHero => IsTargetOwnedByActor(action) ? -10 : 30,
            GameActionType.PlaySpellOnMinion => IsTargetOwnedByActor(action) ? -10 : 25,
            GameActionType.PlayMinionCard => 20,
            GameActionType.EndTurn => 0,
            _ => -1,
        };
    }
}
