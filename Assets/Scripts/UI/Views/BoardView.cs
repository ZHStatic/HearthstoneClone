using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单方战场 UI。
/// 根据传入的 Minion 列表生成多个 MinionView。
/// 玩家战场和敌方战场各使用一个 BoardView。
/// </summary>
public class BoardView : MonoBehaviour
{
    // minionContainer 是所有 MinionView 的父物体。
    // minionViewPrefab 是单个随从 UI 的模板。
    [SerializeField] private Transform minionContainer;
    [SerializeField] private MinionView minionViewPrefab;

    // 当前已经生成出来的随从 UI 列表，用于刷新时统一清理。
    private readonly List<MinionView> minionViews = new List<MinionView>();
    // 当前场上随从和 UI View 的对应关系。
    // BoardView 仍然不判断规则，只提供“这个随从现在显示在哪个 View 上”的查询能力。
    private readonly Dictionary<Minion, MinionView> minionViewByMinion = new Dictionary<Minion, MinionView>();

    /// <summary>
    /// 重新生成这一方战场的随从 UI。
    /// </summary>
    public void Refresh(IReadOnlyList<Minion> minions)
    {
        Refresh(minions, null);
    }

    /// <summary>
    /// 重新生成这一方战场的随从 UI，并给每个随从绑定点击回调。
    /// </summary>
    public void Refresh(IReadOnlyList<Minion> minions, Action<Minion> onMinionClicked)
    {
        Refresh(minions, onMinionClicked, (Minion)null);
    }

    /// <summary>
    /// 重新生成这一方战场的随从 UI，并根据 selectedMinion 显示选中高亮。
    /// </summary>
    public void Refresh(IReadOnlyList<Minion> minions, Action<Minion> onMinionClicked, Minion selectedMinion)
    {
        Refresh(
            minions,
            onMinionClicked,
            minion => minion == selectedMinion ? TargetHighlightState.Selected : TargetHighlightState.None);
    }

    /// <summary>
    /// 重新生成这一方战场的随从 UI，并由上层决定每个随从的高亮状态。
    /// BoardView 只负责转交状态，不判断目标是否合法。
    /// </summary>
    public void Refresh(
        IReadOnlyList<Minion> minions,
        Action<Minion> onMinionClicked,
        Func<Minion, TargetHighlightState> getHighlightState)
    {
        Clear();

        if (minions == null) return;
        if (minionContainer == null) return;
        if (minionViewPrefab == null) return;

        foreach (Minion minion in minions)
        {
            MinionView minionView = Instantiate(minionViewPrefab, minionContainer);
            minionView.SetMinion(minion, onMinionClicked);
            TargetHighlightState highlightState = getHighlightState != null
                ? getHighlightState(minion)
                : TargetHighlightState.None;

            minionView.SetHighlightState(highlightState);
            minionViews.Add(minionView);
            minionViewByMinion[minion] = minionView;
        }
    }

    /// <summary>
    /// 尝试找到某个运行时随从当前对应的 UI View。
    /// 给表现层使用，例如 Core 结算后让受击随从播放一次脉冲。
    /// </summary>
    public bool TryGetMinionView(Minion minion, out MinionView minionView)
    {
        if (minion == null)
        {
            minionView = null;
            return false;
        }

        return minionViewByMinion.TryGetValue(minion, out minionView) && minionView != null;
    }

    /// <summary>
    /// 清理当前生成的所有 MinionView。
    /// </summary>
    public void Clear()
    {
        foreach (MinionView minionView in minionViews)
        {
            if (minionView != null)
            {
                minionView.gameObject.SetActive(false);
                Destroy(minionView.gameObject);
            }
        }

        minionViews.Clear();
        minionViewByMinion.Clear();
    }
}
