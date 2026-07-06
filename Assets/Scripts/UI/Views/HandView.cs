using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手牌区域 UI。
/// 根据传入的 Card 列表刷新多张 CardView。
/// 它只负责显示手牌，不判断卡牌是否能打出。
/// </summary>
public class HandView : MonoBehaviour
{
    // cardContainer 是所有 CardView 的父物体。
    // cardViewPrefab 是单张卡牌 UI 的模板。
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardViewPrefab;

    // 当前已经生成出来的手牌 UI 列表。
    // 刷新时优先复用已有 View，不够再创建，多余的隐藏起来。
    private readonly List<CardView> cardViews = new List<CardView>();

    // 单张卡牌被点击后，要通知给上层的方法。
    private Action<Card> onCardClicked;

    /// <summary>
    /// 设置当前要显示的手牌列表，以及卡牌点击后的回调。
    /// </summary>
    public void SetHand(IReadOnlyList<Card> cards, Action<Card> onCardClicked)
    {
        this.onCardClicked = onCardClicked;
        Refresh(cards);
    }

    /// <summary>
    /// 刷新手牌 UI。
    /// 已有 CardView 够用就复用，不够时才创建新的。
    /// </summary>
    public void Refresh(IReadOnlyList<Card> cards)
    {
        if (cards == null || cardContainer == null || cardViewPrefab == null)
        {
            HideUnusedViews(0);
            return;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            CardView cardView = GetOrCreateCardView(i);

            if (cardView == null) continue;

            cardView.gameObject.SetActive(true);
            cardView.SetCard(card, onCardClicked);
        }

        HideUnusedViews(cards.Count);
    }

    /// <summary>
    /// 清空当前显示的所有 CardView。
    /// View 会被隐藏并保留，供后续刷新复用。
    /// </summary>
    public void Clear()
    {
        HideUnusedViews(0);
    }

    /// <summary>
    /// 获取指定位置的 CardView。
    /// 已经创建过就复用，不够时才创建新的。
    /// </summary>
    private CardView GetOrCreateCardView(int index)
    {
        if (index < cardViews.Count)
        {
            CardView existingView = cardViews[index];
            if (existingView != null)
            {
                return existingView;
            }

            CardView replacementView = Instantiate(cardViewPrefab, cardContainer);
            cardViews[index] = replacementView;
            return replacementView;
        }

        CardView newView = Instantiate(cardViewPrefab, cardContainer);
        cardViews.Add(newView);
        return newView;
    }

    /// <summary>
    /// 隐藏本次刷新没有用到的 CardView。
    /// </summary>
    private void HideUnusedViews(int usedCount)
    {
        for (int i = usedCount; i < cardViews.Count; i++)
        {
            CardView cardView = cardViews[i];
            if (cardView == null) continue;

            cardView.Clear();
            cardView.gameObject.SetActive(false);
        }
    }
}
