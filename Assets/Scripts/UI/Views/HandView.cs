using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手牌区域 UI。
/// 根据传入的 Card 列表生成多张 CardView。
/// 它只负责显示手牌，不判断卡牌是否能打出。
/// </summary>
public class HandView : MonoBehaviour
{
    // cardContainer 是所有 CardView 的父物体。
    // cardViewPrefab 是单张卡牌 UI 的模板。
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardViewPrefab;

    // 当前已经生成出来的手牌 UI 列表，用于刷新时统一清理。
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
    /// 重新生成手牌 UI。
    /// 先清空旧 UI，再根据 cards 创建新的 CardView。
    /// </summary>
    public void Refresh(IReadOnlyList<Card> cards)
    {
        Clear();

        if (cards == null) return;
        if (cardContainer == null) return;
        if (cardViewPrefab == null) return;

        foreach (Card card in cards)
        {
            CardView cardView = Instantiate(cardViewPrefab, cardContainer);
            cardView.SetCard(card, onCardClicked);
            cardViews.Add(cardView);
        }
    }

    /// <summary>
    /// 清理当前生成的所有 CardView。
    /// </summary>
    public void Clear()
    {
        foreach (CardView cardView in cardViews)
        {
            if (cardView != null)
            {
                cardView.gameObject.SetActive(false);
                Destroy(cardView.gameObject);
            }
        }

        cardViews.Clear();
    }
}
