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
    private readonly Player controlledPlayer;
    private readonly int maxActionsPerTurn;

    public AIController(GameManager gameManager, Player controlledPlayer, int maxActionsPerTurn = 20)
    {
        this.gameManager = gameManager;
        this.controlledPlayer = controlledPlayer;
        this.maxActionsPerTurn = maxActionsPerTurn;
        actionSelector = new ActionSelector();
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

        Debug.LogWarning("AI reached max actions this turn. Force ending turn to avoid an infinite loop.");
        gameManager.ExecuteAction(GameAction.CreateEndTurn(controlledPlayer));
    }

    private GameAction ExecuteNextAction()
    {
        List<GameAction> legalActions = GameActionGenerator.GenerateLegalActions(gameManager);
        GameAction selectedAction = actionSelector.SelectAction(legalActions);

        if (selectedAction == null)
        {
            Debug.Log("AI found no legal actions.");
            return null;
        }

        Debug.Log($"AI selected action: {GetActionDebugText(selectedAction)}");

        GameActionResult result = gameManager.ExecuteAction(selectedAction);
        if (result.Failed)
        {
            Debug.LogWarning($"AI action failed: {result.Message}");
            return null;
        }

        if (!string.IsNullOrEmpty(result.Message))
        {
            Debug.Log($"AI action result: {result.Message}");
        }

        return selectedAction;
    }

    private string GetActionDebugText(GameAction action)
    {
        if (action == null) return "None";

        return action.ActionType switch
        {
            GameActionType.PlayMinionCard => $"PlayMinionCard: {GetCardName(action.Card)}",
            GameActionType.PlaySpellOnMinion => $"PlaySpellOnMinion: {GetCardName(action.Card)} -> {GetMinionName(action.TargetMinion)}",
            GameActionType.PlaySpellOnHero => $"PlaySpellOnHero: {GetCardName(action.Card)} -> Hero",
            GameActionType.AttackMinion => $"AttackMinion: {GetMinionName(action.Attacker)} -> {GetMinionName(action.TargetMinion)}",
            GameActionType.AttackHero => $"AttackHero: {GetMinionName(action.Attacker)} -> Hero",
            GameActionType.EndTurn => "EndTurn",
            _ => action.ActionType.ToString(),
        };
    }

    private string GetCardName(Card card)
    {
        if (card == null || card.CardData == null) return "Unknown Card";
        return card.CardData.CardName;
    }

    private string GetMinionName(Minion minion)
    {
        if (minion == null || minion.CardData == null) return "Unknown Minion";
        return minion.CardData.CardName;
    }
}
