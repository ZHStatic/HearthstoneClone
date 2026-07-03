/// <summary>
/// UI 目标高亮状态。
/// 它只描述显示效果，不代表最终规则结论；真正能否执行仍由 Core 验证。
/// </summary>
public enum TargetHighlightState
{
    /// <summary>
    /// 普通状态。
    /// </summary>
    None,

    /// <summary>
    /// 当前操作下的合法目标。
    /// </summary>
    Valid,

    /// <summary>
    /// 当前操作下的非法目标。
    /// </summary>
    Invalid,

    /// <summary>
    /// 当前已经选中的对象。
    /// </summary>
    Selected
}
