using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI 回合控制器。
/// 它负责生成合法动作、交给 ActionSelector 选择，并通过 GameManager.ExecuteAction 执行动作。
/// </summary>
public class AIController
{
    private readonly GameManager gameManager;
    private readonly ActionSelector actionSelector;
    private readonly Evaluator evaluator;
    private readonly Player controlledPlayer;
    private readonly int maxActionsPerTurn;

    public AIController(GameManager gameManager, Player controlledPlayer, int maxActionsPerTurn = 20)
    {
        this.gameManager = gameManager;
        this.controlledPlayer = controlledPlayer;
        this.maxActionsPerTurn = maxActionsPerTurn;
        actionSelector = new ActionSelector();
        evaluator = new Evaluator();
    }

    /// <summary>
    /// 执行 AI 的完整回合。
    /// 第一版直接连续执行动作，直到选择结束回合、游戏结束或达到安全上限。
    /// </summary>
    public void TakeTurn()
    {
        if (gameManager == null || controlledPlayer == null) return;
        if (gameManager.IsGameOver) return;
        if (gameManager.CurrentPlayer != controlledPlayer) return;

        for (int i = 0; i < maxActionsPerTurn; i++)
        {
            GameAction executedAction = ExecuteNextAction();

            if (executedAction == null) return;
            if (executedAction.ActionType == GameActionType.EndTurn) return;
            if (gameManager.IsGameOver) return;
            if (gameManager.CurrentPlayer != controlledPlayer) return;
        }

        Debug.LogWarning("AI 达到本回合行动上限，强制结束回合，避免死循环。");
        gameManager.ExecuteAction(GameAction.CreateEndTurn(controlledPlayer));
    }

    private GameAction ExecuteNextAction()
    {
        List<GameAction> legalActions = GameActionGenerator.GenerateLegalActions(gameManager);
        AIActionSelection selection = actionSelector.SelectAction(legalActions, gameManager, evaluator);
        GameAction selectedAction = selection?.Action;

        if (selectedAction == null)
        {
            Debug.Log("AI 没有找到可执行动作。");
            return null;
        }

        string actionText = GetActionDebugText(selectedAction);
        string reasonText = GetSelectionReasonText(selection.Reason);
        EvaluationResult scoreBeforeAction = GetCurrentEvaluationResult();

        GameActionResult result = gameManager.ExecuteAction(selectedAction);
        EvaluationResult scoreAfterAction = GetCurrentEvaluationResult();
        string logText = BuildActionLogText(
            actionText,
            reasonText,
            GetActionResultText(result),
            selection.SimulatedEvaluation,
            scoreBeforeAction,
            scoreAfterAction);
        if (result.Failed)
        {
            Debug.LogWarning(logText);
            return null;
        }

        Debug.Log(logText);

        return selectedAction;
    }

    /// <summary>
    /// 构建单条 AI 日志。
    /// Unity Console 列表对多行日志显示不稳定，所以这里保持一行展示。
    /// </summary>
    private string BuildActionLogText(
        string actionText,
        string reasonText,
        string resultText,
        EvaluationResult simulatedEvaluation,
        EvaluationResult scoreBeforeAction,
        EvaluationResult scoreAfterAction)
    {
        return $"AI 行动：{actionText} | 理由：{reasonText} | 模拟评分：{GetEvaluationDebugText(simulatedEvaluation)} | 真实评分：{GetEvaluationDebugText(scoreBeforeAction)} -> {GetEvaluationDebugText(scoreAfterAction)} | 结果：{resultText}";
    }

    /// <summary>
    /// 从当前 AI 控制的玩家视角读取局面评分明细。
    /// 当前评分用于日志观察；ActionSelector 也会用同一个 Evaluator 评估快照模拟结果。
    /// </summary>
    private EvaluationResult GetCurrentEvaluationResult()
    {
        if (gameManager == null || controlledPlayer == null) return new EvaluationResult(0, 0, 0);

        Player opponent = gameManager.GetOpponent(controlledPlayer);
        return evaluator.EvaluateDetailed(controlledPlayer, opponent, gameManager.Board);
    }

    private string GetEvaluationDebugText(EvaluationResult result)
    {
        return result != null ? result.ToDebugText() : "无评分";
    }

    /// <summary>
    /// 把 AI 选择原因转换成适合 Console 阅读的中文文本。
    /// 这里只解释 AI 当前选择动作的直接原因。
    /// </summary>
    private string GetSelectionReasonText(AIActionSelectionReason reason)
    {
        return reason switch
        {
            AIActionSelectionReason.Lethal => "这个动作可以击杀敌方英雄。",
            AIActionSelectionReason.HighestEvaluationScore => "这个动作不降低评分，并且在候选动作里模拟后评分最高。",
            AIActionSelectionReason.AcceptableScoreLoss => "这个动作会小幅降低评分，但仍在可接受范围内，用来换取节奏。",
            AIActionSelectionReason.NoProfitableActionEndTurn => "没有找到评分在可接受范围内的主动动作，选择结束回合。",
            AIActionSelectionReason.FallbackPriority => "没有命中特殊策略，按固定优先级选择。",
            _ => "没有明确选择原因。",
        };
    }

    /// <summary>
    /// 把 AI 选择的动作转换成适合 Console 阅读的中文文本。
    /// 这里只负责展示，不参与动作合法性判断或执行。
    /// </summary>
    private string GetActionDebugText(GameAction action)
    {
        if (action == null) return "无动作";

        return action.ActionType switch
        {
            GameActionType.PlayMinionCard => $"打出随从牌 {GetCardName(action.Card)}",
            GameActionType.PlaySpellOnMinion => $"对 {GetMinionName(action.TargetMinion)} 释放 {GetCardName(action.Card)}",
            GameActionType.PlaySpellOnHero => $"对 {GetHeroName(action.TargetHero)} 释放 {GetCardName(action.Card)}",
            GameActionType.AttackMinion => $"{GetMinionName(action.Attacker)} 攻击 {GetMinionName(action.TargetMinion)}",
            GameActionType.AttackHero => $"{GetMinionName(action.Attacker)} 攻击 {GetHeroName(action.TargetHero)}",
            GameActionType.EndTurn => "结束回合",
            _ => $"未知动作：{action.ActionType}",
        };
    }

    /// <summary>
    /// 把 Core 返回的操作结果转换成稳定的日志文本。
    /// Core 仍然负责真正的结算消息，这里只处理空消息和空结果的兜底显示。
    /// </summary>
    private string GetActionResultText(GameActionResult result)
    {
        if (result == null) return "没有返回操作结果。";
        if (!string.IsNullOrEmpty(result.Message)) return result.Message;

        return result.Success ? "行动执行成功。" : "行动执行失败。";
    }

    private string GetCardName(Card card)
    {
        if (card == null || card.CardData == null) return "未知卡牌";
        return card.CardData.CardName;
    }

    private string GetHeroName(Hero hero)
    {
        if (hero == null) return "未知英雄";
        return hero.Name;
    }

    private string GetMinionName(Minion minion)
    {
        if (minion == null || minion.CardData == null) return "未知随从";
        return minion.CardData.CardName;
    }
}
