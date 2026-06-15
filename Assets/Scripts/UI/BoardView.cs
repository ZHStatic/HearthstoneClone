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
        Refresh(minions, onMinionClicked, null);
    }

    /// <summary>
    /// 重新生成这一方战场的随从 UI，并根据 selectedMinion 显示选中高亮。
    /// </summary>
    public void Refresh(IReadOnlyList<Minion> minions, Action<Minion> onMinionClicked, Minion selectedMinion)
    {
        Clear();

        if (minions == null) return;
        if (minionContainer == null) return;
        if (minionViewPrefab == null) return;

        foreach (Minion minion in minions)
        {
            MinionView minionView = Instantiate(minionViewPrefab, minionContainer);
            minionView.SetMinion(minion, onMinionClicked);
            minionView.SetSelected(minion == selectedMinion);
            minionViews.Add(minionView);
        }
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
    }
}
