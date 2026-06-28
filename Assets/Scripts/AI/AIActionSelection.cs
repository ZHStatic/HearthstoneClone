/// <summary>
/// AI 动作选择结果。
/// 它把最终动作、选择原因和可选的模拟评分放在一起，方便 AIController 打印可读日志。
/// </summary>
public class AIActionSelection
{
    /// <summary>
    /// AI 最终选择的动作。
    /// </summary>
    public GameAction Action { get; private set; }

    /// <summary>
    /// AI 选择该动作的原因。
    /// </summary>
    public AIActionSelectionReason Reason { get; private set; }

    /// <summary>
    /// 被选动作在快照模拟后的评分。
    /// 如果动作没有走快照评分选择，比如斩杀或兜底选择，这里可以为空。
    /// </summary>
    public EvaluationResult SimulatedEvaluation { get; private set; }

    public AIActionSelection(GameAction action, AIActionSelectionReason reason)
        : this(action, reason, null)
    {
    }

    public AIActionSelection(GameAction action, AIActionSelectionReason reason, EvaluationResult simulatedEvaluation)
    {
        Action = action;
        Reason = action == null ? AIActionSelectionReason.None : reason;
        SimulatedEvaluation = simulatedEvaluation;
    }
}
