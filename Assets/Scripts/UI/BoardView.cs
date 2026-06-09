using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds one side of the board UI from runtime Minion objects.
/// </summary>
public class BoardView : MonoBehaviour
{
    [SerializeField] private Transform minionContainer;
    [SerializeField] private MinionView minionViewPrefab;

    private readonly List<MinionView> minionViews = new List<MinionView>();

    public void Refresh(IReadOnlyList<Minion> minions)
    {
        Clear();

        if (minions == null) return;
        if (minionContainer == null) return;
        if (minionViewPrefab == null) return;

        foreach (Minion minion in minions)
        {
            MinionView minionView = Instantiate(minionViewPrefab, minionContainer);
            minionView.SetMinion(minion);
            minionViews.Add(minionView);
        }
    }

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
