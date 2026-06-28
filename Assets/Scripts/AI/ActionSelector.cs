using System.Collections.Generic;

/// <summary>
/// AI 动作选择器。
/// 它只从已经生成好的合法动作里挑一条，不执行真实动作。
/// </summary>
public class ActionSelector
{
    private const int AllowedScoreLoss = 3;

    /// <summary>
    /// 从合法动作中选择一条动作。
    /// 当前策略：先保留斩杀硬规则；其余主动动作可以接受小幅亏分，避免 AI 过于保守。
    /// 这里接收的动作必须已经由 GameActionGenerator 验证过合法性。
    /// </summary>
    public AIActionSelection SelectAction(
        IReadOnlyList<GameAction> legalActions,
        GameManager gameManager,
        Evaluator evaluator)
    {
        if (legalActions == null || legalActions.Count == 0) return null;

        if (TryFindLethalAction(legalActions, out GameAction lethalAction))
        {
            return new AIActionSelection(lethalAction, AIActionSelectionReason.Lethal);
        }

        if (TryFindBestEvaluatedAction(
            legalActions,
            gameManager,
            evaluator,
            out GameAction evaluatedAction,
            out EvaluationResult simulatedEvaluation,
            out AIActionSelectionReason evaluatedReason))
        {
            return new AIActionSelection(
                evaluatedAction,
                evaluatedReason,
                simulatedEvaluation);
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
    /// 用当前局面的快照逐个模拟合法动作，选择模拟后评分最高且不超过亏分阈值的主动动作。
    /// 如果没有任何主动动作进入可接受范围，就选择结束回合。
    /// 这是阶段性简化，不是成熟项目最终做法：当前只看一步，不搜索后续回合，也不完整模拟战吼和亡语。
    /// </summary>
    private bool TryFindBestEvaluatedAction(
        IReadOnlyList<GameAction> legalActions,
        GameManager gameManager,
        Evaluator evaluator,
        out GameAction selectedAction,
        out EvaluationResult selectedEvaluation,
        out AIActionSelectionReason selectedReason)
    {
        selectedAction = null;
        selectedEvaluation = null;
        selectedReason = AIActionSelectionReason.None;

        if (legalActions == null || legalActions.Count == 0) return false;
        if (gameManager == null || evaluator == null) return false;

        GameStateSnapshot snapshot = GameStateSnapshot.FromGameManager(gameManager);
        if (snapshot == null) return false;

        int playerIndex = GetCurrentPlayerIndex(gameManager);
        EvaluationResult currentEvaluation = evaluator.EvaluateDetailed(snapshot, playerIndex);
        int currentEvaluationScore = currentEvaluation != null ? currentEvaluation.TotalScore : int.MinValue;
        int selectedEvaluationScore = int.MinValue;
        int selectedTieBreakerScore = int.MinValue;
        GameAction endTurnAction = null;
        EvaluationResult endTurnEvaluation = null;

        for (int i = 0; i < legalActions.Count; i++)
        {
            GameAction action = legalActions[i];
            if (!SnapshotActionMapper.TryMap(action, gameManager, out SnapshotAction snapshotAction))
            {
                continue;
            }

            GameStateSnapshot simulatedState = SnapshotSimulator.Simulate(snapshot, snapshotAction);
            EvaluationResult evaluation = evaluator.EvaluateDetailed(simulatedState, playerIndex);
            int evaluationScore = evaluation != null ? evaluation.TotalScore : int.MinValue;
            int tieBreakerScore = GetFallbackActionScore(action);

            if (action.ActionType == GameActionType.EndTurn)
            {
                endTurnAction = action;
                endTurnEvaluation = evaluation;
                continue;
            }

            if (evaluationScore < currentEvaluationScore - AllowedScoreLoss)
            {
                continue;
            }

            if (selectedAction == null
                || evaluationScore > selectedEvaluationScore
                || (evaluationScore == selectedEvaluationScore && tieBreakerScore > selectedTieBreakerScore))
            {
                selectedAction = action;
                selectedEvaluation = evaluation;
                selectedEvaluationScore = evaluationScore;
                selectedTieBreakerScore = tieBreakerScore;
            }
        }

        if (selectedAction != null)
        {
            selectedReason = selectedEvaluationScore < currentEvaluationScore
                ? AIActionSelectionReason.AcceptableScoreLoss
                : AIActionSelectionReason.HighestEvaluationScore;
            return true;
        }

        if (endTurnAction != null)
        {
            selectedAction = endTurnAction;
            selectedEvaluation = endTurnEvaluation;
            selectedReason = AIActionSelectionReason.NoProfitableActionEndTurn;
            return true;
        }

        return false;
    }

    private int GetCurrentPlayerIndex(GameManager gameManager)
    {
        if (gameManager == null) return GameStateSnapshot.PlayerIndex;

        return gameManager.CurrentPlayer == gameManager.Enemy
            ? GameStateSnapshot.EnemyIndex
            : GameStateSnapshot.PlayerIndex;
    }

    /// <summary>
    /// 兜底选择：如果快照模拟无法选出动作，就按固定优先级选一条动作。
    /// 这样可以保证 AI 至少会从合法动作中选到一个确定结果。
    /// </summary>
    private GameAction SelectHighestPriorityAction(IReadOnlyList<GameAction> legalActions)
    {
        GameAction selectedAction = null;
        int selectedScore = int.MinValue;

        for (int i = 0; i < legalActions.Count; i++)
        {
            GameAction candidate = legalActions[i];
            int candidateScore = GetFallbackActionScore(candidate);

            if (selectedAction == null || candidateScore > selectedScore)
            {
                selectedAction = candidate;
                selectedScore = candidateScore;
            }
        }

        return selectedAction;
    }

    /// <summary>
    /// 返回兜底阶段使用的动作分数。
    /// 动作类型优先级仍然是主体，同类型动作再用少量附加分排序。
    /// </summary>
    private int GetFallbackActionScore(GameAction action)
    {
        int actionPriority = GetActionPriority(action);
        if (actionPriority == int.MinValue) return int.MinValue;

        return actionPriority * 1000 + GetAttackActionScore(action);
    }

    /// <summary>
    /// 给攻击动作一个兜底附加分。
    /// 能打英雄时仍然优先打英雄；如果只能攻击随从，则优先攻击攻击力高的敌方随从。
    /// </summary>
    private int GetAttackActionScore(GameAction action)
    {
        if (action == null) return 0;

        return action.ActionType switch
        {
            GameActionType.AttackMinion => action.TargetMinion != null ? action.TargetMinion.Attack : 0,
            _ => 0,
        };
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
    /// 返回动作的兜底优先级。
    /// 分数只用于兜底和同分排序，不代表局面评分。
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
