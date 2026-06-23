/// <summary>
/// 一次游戏操作的结果。
/// 它让 Core 层除了返回成功/失败，还能说明失败原因和 UI 可显示的反馈文本。
/// </summary>
public class GameActionResult
{
    public bool Success { get; private set; }
    public GameActionFailureReason FailureReason { get; private set; }
    public string Message { get; private set; }
    public BattleLogEntry LogEntry { get; private set; }

    public bool Failed => !Success;

    private GameActionResult(
        bool success,
        GameActionFailureReason failureReason,
        string message,
        BattleLogEntry logEntry)
    {
        Success = success;
        FailureReason = success ? GameActionFailureReason.None : failureReason;
        Message = message ?? "";
        LogEntry = logEntry;
    }

    /// <summary>
    /// 创建一个成功结果。
    /// </summary>
    public static GameActionResult Succeeded(string message = "", BattleLogEntry logEntry = null)
    {
        return new GameActionResult(
            true,
            GameActionFailureReason.None,
            message,
            logEntry);
    }

    /// <summary>
    /// 创建一个失败结果。
    /// 如果调用方误传 None，则自动改成 Unknown，避免出现“失败但没有失败原因”的矛盾状态。
    /// </summary>
    public static GameActionResult FailedWith(GameActionFailureReason reason, string message = "")
    {
        GameActionFailureReason failureReason = reason == GameActionFailureReason.None
            ? GameActionFailureReason.Unknown
            : reason;

        return new GameActionResult(
            false,
            failureReason,
            message,
            null);
    }
}
