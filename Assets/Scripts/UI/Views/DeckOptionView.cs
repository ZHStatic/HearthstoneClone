using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单个套牌选项 UI。
/// 它负责显示一套 DeckData 的名称、说明、卡牌数量，并把点击事件通知给 DeckSelectionController。
/// 它不负责开始对局，也不直接调用 GameManager。
/// </summary>
public class DeckOptionView : MonoBehaviour
{
    // 当前选项的按钮。通常就是挂在同一个物体上的 Button。
    [SerializeField] private Button button;

    // 玩家在套牌选择界面看到的套牌名称。
    [SerializeField] private Text deckNameText;

    // 套牌说明，例如“低费随从压制”。
    [SerializeField] private Text deckDescriptionText;

    // 有效卡牌数量。数据来自 DeckData.CardCount。
    [SerializeField] private Text deckCountText;

    // 按钮颜色属于视觉参数，所以保留在 Inspector 中可调。
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = new Color(1f, 0.9f, 0.35f, 1f);
    [SerializeField] private Color disabledButtonColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    private int optionIndex = -1;
    private DeckData deckData;
    private Action<int> onSelected;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(HandleClicked);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    /// <summary>
    /// 设置这个 UI 选项对应哪套 DeckData，以及点击后通知谁。
    /// </summary>
    public void SetOption(int optionIndex, DeckData deckData, Action<int> onSelected)
    {
        this.optionIndex = optionIndex;
        this.deckData = deckData;
        this.onSelected = onSelected;
    }

    /// <summary>
    /// 刷新名称、说明、数量和按钮颜色。
    /// </summary>
    public void Refresh(bool isSelected)
    {
        bool hasDeck = deckData != null;

        SetText(deckNameText, hasDeck ? deckData.DeckName : "未配置套牌");
        SetText(deckDescriptionText, hasDeck ? deckData.Description : "请在 Inspector 中绑定 DeckData。");
        SetText(deckCountText, hasDeck ? $"{deckData.CardCount} 张" : "0 张");
        ApplyButtonState(hasDeck, isSelected);
    }

    private void HandleClicked()
    {
        if (deckData == null) return;

        onSelected?.Invoke(optionIndex);
    }

    private void ApplyButtonState(bool hasDeck, bool isSelected)
    {
        if (button == null) return;

        button.interactable = hasDeck;

        Graphic targetGraphic = button.targetGraphic;
        if (targetGraphic == null) return;

        if (!hasDeck)
        {
            targetGraphic.color = disabledButtonColor;
            return;
        }

        targetGraphic.color = isSelected ? selectedButtonColor : normalButtonColor;
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value ?? "";
        }
    }
}
