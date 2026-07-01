using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameManager 的 AI 调试桥接。
/// 这是阶段性简化，不是成熟项目最终边界：当前仍由 GameManager 在回合开始时触发 Enemy AI，
/// 但 AI 控制器、快照评分和 AI 手牌日志集中放在 AI 目录，避免主 Core 文件直接承载 AI 细节。
/// </summary>
public partial class GameManager
{
    // 调试开关：Enemy 回合开始时打印 AI 手牌和当前法力，方便验证 ActionSelector 的选择。
    [SerializeField] private bool logAIHandOnTurnStart = false;
    // 调试开关：回合开始时对比真实局面评分和快照局面评分，验证快照复制是否漏字段。
    [SerializeField] private bool logSnapshotEvaluationOnTurnStart = false;
    // 调试开关：回合开始时打印每个可映射动作的快照模拟后评分。
    [SerializeField] private bool logSnapshotSimulationOnTurnStart = false;

    // 阶段 3 第一版 AI：只控制 Enemy，复用 GameActionGenerator 和 ExecuteAction。
    [SerializeField] private bool enableEnemyAI = true;
    [SerializeField] private int maxAIActionsPerTurn = 20;

    private AIController enemyAIController;

    private void InitializeEnemyAI()
    {
        enemyAIController = new AIController(this, Enemy, maxAIActionsPerTurn);
    }

    private void TryRunEnemyAI()
    {
        if (!enableEnemyAI) return;
        if (enemyAIController == null) return;
        if (IsGameOver) return;
        if (CurrentPlayer != Enemy) return;

        enemyAIController.TakeTurn();
    }

    /// <summary>
    /// 调试用：对比真实局面评分和快照局面评分。
    /// 这只用于验证 GameStateSnapshot 是否正确复制当前局面，不影响 AI 决策。
    /// </summary>
    private void LogSnapshotEvaluationForCurrentPlayer()
    {
        if (!logSnapshotEvaluationOnTurnStart) return;
        if (CurrentPlayer == null) return;

        Evaluator evaluator = new Evaluator();
        Player opponent = GetOpponent(CurrentPlayer);
        EvaluationResult realEvaluation = evaluator.EvaluateDetailed(CurrentPlayer, opponent, Board);

        GameStateSnapshot snapshot = GameStateSnapshot.FromGameManager(this);
        int currentPlayerIndex = CurrentPlayer == Enemy
            ? GameStateSnapshot.EnemyIndex
            : GameStateSnapshot.PlayerIndex;
        EvaluationResult snapshotEvaluation = evaluator.EvaluateDetailed(snapshot, currentPlayerIndex);
        bool isMatch = IsSameEvaluation(realEvaluation, snapshotEvaluation);

        Debug.Log($"Snapshot Evaluation - {GetPlayerLogName(CurrentPlayer)}: Real[{realEvaluation.ToDebugText()}] Snapshot[{snapshotEvaluation.ToDebugText()}] Match={isMatch}");
    }

    private bool IsSameEvaluation(EvaluationResult left, EvaluationResult right)
    {
        if (left == null || right == null) return false;

        return left.TotalScore == right.TotalScore
            && left.HeroHealthScore == right.HeroHealthScore
            && left.HandScore == right.HandScore
            && left.BoardScore == right.BoardScore;
    }

    /// <summary>
    /// 调试用：打印当前合法动作在快照模拟后的评分。
    /// 这只帮助观察模拟器结果，不影响 AI 当前选择策略。
    /// </summary>
    private void LogSnapshotSimulationForCurrentPlayer()
    {
        if (!logSnapshotSimulationOnTurnStart) return;
        if (CurrentPlayer == null) return;

        List<GameAction> legalActions = GameActionGenerator.GenerateLegalActions(this);
        GameStateSnapshot snapshot = GameStateSnapshot.FromGameManager(this);
        Evaluator evaluator = new Evaluator();
        int currentPlayerIndex = CurrentPlayer == Enemy
            ? GameStateSnapshot.EnemyIndex
            : GameStateSnapshot.PlayerIndex;

        Debug.Log($"Snapshot Simulation - {GetPlayerLogName(CurrentPlayer)}: {legalActions.Count} legal actions");

        for (int i = 0; i < legalActions.Count; i++)
        {
            GameAction action = legalActions[i];
            string actionText = GetActionDebugText(action);

            if (!SnapshotActionMapper.TryMap(action, this, out SnapshotAction snapshotAction))
            {
                Debug.Log($"Snapshot Simulation - Skip: {actionText} | 原因：当前模拟层暂不支持此动作或无法定位索引。");
                continue;
            }

            GameStateSnapshot simulatedState = SnapshotSimulator.Simulate(snapshot, snapshotAction);
            EvaluationResult evaluation = evaluator.EvaluateDetailed(simulatedState, currentPlayerIndex);

            Debug.Log($"Snapshot Simulation - {actionText} | 模拟后评分：{evaluation.ToDebugText()}");
        }
    }

    /// <summary>
    /// 调试用：Enemy 回合开始时打印 AI 手牌。
    /// 这不是正式 UI，只用于阶段 3.3 验证 AI 是否按预期选择出牌。
    /// </summary>
    private void LogAIHandForCurrentPlayer()
    {
        if (!logAIHandOnTurnStart) return;
        if (CurrentPlayer != Enemy) return;
        if (Enemy == null) return;

        string handText = BuildHandDebugText(Enemy);
        Debug.Log($"AI 手牌：{handText}，当前法力：{Enemy.CurrentMana}/{Enemy.MaxMana}");
    }

    private string BuildHandDebugText(Player player)
    {
        if (player == null || player.Hand == null || player.Hand.Count == 0)
        {
            return "无";
        }

        List<string> cardTexts = new List<string>();
        foreach (Card card in player.Hand)
        {
            if (card == null)
            {
                cardTexts.Add("未知卡牌");
                continue;
            }

            cardTexts.Add($"{card.CurrentCost}费 {GetCardLogName(card)}");
        }

        return string.Join(" | ", cardTexts);
    }
}
