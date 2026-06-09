using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the current player's hand UI from runtime Card objects.
/// It does not decide whether a card can be played.
/// </summary>
public class HandView : MonoBehaviour
{
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardViewPrefab;

    private readonly List<CardView> cardViews = new List<CardView>();
    private Action<Card> onCardClicked;

    public void SetHand(IReadOnlyList<Card> cards, Action<Card> onCardClicked)
    {
        this.onCardClicked = onCardClicked;
        Refresh(cards);
    }

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
