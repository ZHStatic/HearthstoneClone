/// <summary>
/// AI 动作选择结果。
/// 它把最终动作和选择原因放在一起，方便 AIController 打印可读日志。
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

    public AIActionSelection(GameAction action, AIActionSelectionReason reason)
    {
        Action = action;
        Reason = action == null ? AIActionSelectionReason.None : reason;
    }
}
