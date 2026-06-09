using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows one card in the hand UI.
/// It only displays card data and reports clicks to the parent view.
/// </summary>
public class CardView : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text costText;
    [SerializeField] private Text attackText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button button;

    private Card card;
    private Action<Card> onClicked;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    public void SetCard(Card card, Action<Card> onClicked)
    {
        this.card = card;
        this.onClicked = onClicked;

        Refresh();
    }

    public void Refresh()
    {
        if (card == null || card.CardData == null)
        {
            Clear();
            return;
        }

        SetText(nameText, card.CardData.CardName);
        SetText(costText, card.CurrentCost.ToString());
        SetText(attackText, card.CardData.Attack.ToString());
        SetText(healthText, card.CardData.Health.ToString());
        SetText(descriptionText, card.CardData.Description);

        if (button != null)
        {
            button.interactable = true;
        }
    }

    public void Clear()
    {
        card = null;
        onClicked = null;

        SetText(nameText, "");
        SetText(costText, "");
        SetText(attackText, "");
        SetText(healthText, "");
        SetText(descriptionText, "");

        if (button != null)
        {
            button.interactable = false;
        }
    }

    private void HandleClick()
    {
        if (card == null) return;

        onClicked?.Invoke(card);
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }
}
