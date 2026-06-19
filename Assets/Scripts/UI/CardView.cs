using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单张手牌 UI。
/// 只负责显示一张 Card 的数据，并在被点击时通知上层。
/// 它不直接引用 GameManager，也不判断这张牌能不能打出。
/// </summary>
public class CardView : MonoBehaviour
{
    // 在 Inspector 中绑定的文字和按钮组件。
    // 这些组件来自 CardViewPrefab 下面的子物体。
    [SerializeField] private Text nameText;
    [SerializeField] private Text costText;
    [SerializeField] private Text attackText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button button;

    // 当前这张 UI 正在显示的运行时卡牌。
    private Card card;

    // 点击回调：CardView 不自己处理规则，只把被点击的 Card 交给上层。
    private Action<Card> onClicked;

    // Unity 生命周期方法：对象创建后注册按钮点击事件。
    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    // 对象销毁时移除监听，避免按钮还保存着已经销毁对象的方法引用。
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    /// <summary>
    /// 设置这张 UI 要显示的卡牌，以及点击后要通知谁。
    /// </summary>
    public void SetCard(Card card, Action<Card> onClicked)
    {
        this.card = card;
        this.onClicked = onClicked;

        Refresh();
    }

    /// <summary>
    /// 把当前 Card 的数据刷新到 UI 文本上。
    /// </summary>
    public void Refresh()
    {
        if (card == null || card.CardData == null)
        {
            Clear();
            return;
        }

        SetText(nameText, card.CardData.CardName);
        SetText(costText, card.CurrentCost.ToString());
        SetCardStatsText(card.CardData);
        SetText(descriptionText, GetDescriptionText(card.CardData));

        if (button != null)
        {
            button.interactable = true;
        }
    }

    /// <summary>
    /// 清空当前显示内容，并让按钮不可点击。
    /// </summary>
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

    /// <summary>
    /// 处理按钮点击。
    /// 真正的出牌逻辑不在这里做，而是交给 onClicked 的接收者。
    /// </summary>
    private void HandleClick()
    {
        if (card == null) return;

        onClicked?.Invoke(card);
    }

    /// <summary>
    /// 根据卡牌类型显示数值。
    /// 随从显示攻击/生命，法术暂时用攻击位置显示伤害值。
    /// </summary>
    private void SetCardStatsText(CardData cardData)
    {
        if (cardData == null)
        {
            SetText(attackText, "");
            SetText(healthText, "");
            return;
        }

        if (cardData.CardType == CardType.Spell)
        {
            SetText(attackText, cardData.SpellDamage.ToString());
            SetText(healthText, "");
            return;
        }

        SetText(attackText, cardData.Attack.ToString());
        SetText(healthText, cardData.Health.ToString());
    }

    /// <summary>
    /// 生成卡牌描述区显示的文字。
    /// 当前先复用 descriptionText，同时显示关键词和卡牌描述，避免改 Prefab。
    /// </summary>
    private string GetDescriptionText(CardData cardData)
    {
        if (cardData == null) return "";

        string keywordsText = GetKeywordsText(cardData);
        string description = cardData.Description;

        if (string.IsNullOrWhiteSpace(keywordsText)) return description;
        if (string.IsNullOrWhiteSpace(description)) return keywordsText;

        // 如果手写描述里已经包含关键词，就不重复拼一遍。
        if (description.Contains(keywordsText)) return description;

        return $"{keywordsText}\n{description}";
    }

    /// <summary>
    /// 把卡牌模板上的关键词列表转成 UI 文本。
    /// 多个关键词之间先用空格分隔，之后做正式 UI 时可以换成图标或独立标签。
    /// </summary>
    private string GetKeywordsText(CardData cardData)
    {
        if (cardData == null || cardData.Keywords == null) return "";

        string text = "";

        foreach (KeywordType keyword in cardData.Keywords)
        {
            string keywordText = GetKeywordText(keyword);
            if (string.IsNullOrWhiteSpace(keywordText)) continue;

            if (!string.IsNullOrWhiteSpace(text))
            {
                text += " ";
            }

            text += keywordText;
        }

        return text;
    }

    /// <summary>
    /// 把关键词枚举转成玩家能看懂的中文。
    /// </summary>
    private string GetKeywordText(KeywordType keyword)
    {
        switch (keyword)
        {
            case KeywordType.Charge:
                return "冲锋";
            default:
                return "";
        }
    }

    /// <summary>
    /// 安全设置 Text 内容。
    /// 如果某个 Text 没有在 Inspector 中绑定，就直接跳过。
    /// </summary>
    private void SetText(Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }
}
