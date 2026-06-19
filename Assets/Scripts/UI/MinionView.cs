using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单个随从 UI。
/// 负责显示随从数据，并在被点击时通知上层。
/// 它不直接引用 GameManager，也不判断攻击是否合法。
/// </summary>
public class MinionView : MonoBehaviour
{
    // 在 Inspector 中绑定的文字组件。
    // 它们显示随从名、攻击、生命和是否可以攻击。
    [SerializeField] private Text nameText;
    [SerializeField] private Text attackText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text canAttackText;

    // 用背景颜色显示选中状态。没有绑定时会自动尝试从当前物体上获取 Image。
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.9f, 0.35f, 1f);

    // 接收点击的按钮组件。
    // 如果 Inspector 没有绑定，Awake 会尝试从当前物体上自动获取。
    [SerializeField] private Button button;

    // 当前这张 UI 正在显示的运行时随从。
    private Minion minion;

    // 点击回调：MinionView 不自己处理攻击，只把被点击的 Minion 交给上层。
    private Action<Minion> onClicked;
    private bool isSelected;

    // Unity 生命周期方法：对象创建后注册按钮点击事件。
    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

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
    /// 设置这张 UI 要显示的随从。
    /// </summary>
    public void SetMinion(Minion minion)
    {
        SetMinion(minion, null);
    }

    /// <summary>
    /// 设置这张 UI 要显示的随从，以及点击后要通知谁。
    /// </summary>
    public void SetMinion(Minion minion, Action<Minion> onClicked)
    {
        this.minion = minion;
        this.onClicked = onClicked;

        Refresh();
    }

    /// <summary>
    /// 把当前 Minion 的数据刷新到 UI 文本上。
    /// </summary>
    public void Refresh()
    {
        if (minion == null || minion.CardData == null)
        {
            Clear();
            return;
        }

        SetText(nameText, minion.CardData.CardName);
        SetText(attackText, minion.Attack.ToString());
        SetText(healthText, minion.CurrentHealth.ToString());
        SetText(canAttackText, GetStatusText(minion));
        ApplySelectedState();

        if (button != null)
        {
            button.interactable = true;
        }
    }

    /// <summary>
    /// 设置这个随从 UI 是否处于选中状态。
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        ApplySelectedState();
    }

    /// <summary>
    /// 清空当前显示内容。
    /// </summary>
    public void Clear()
    {
        minion = null;
        onClicked = null;
        isSelected = false;

        SetText(nameText, "");
        SetText(attackText, "");
        SetText(healthText, "");
        SetText(canAttackText, "");
        ApplySelectedState();

        if (button != null)
        {
            button.interactable = false;
        }
    }

    /// <summary>
    /// 处理按钮点击。
    /// 真正的攻击逻辑不在这里做，而是交给 onClicked 的接收者。
    /// </summary>
    private void HandleClick()
    {
        if (minion == null) return;

        onClicked?.Invoke(minion);
    }

    /// <summary>
    /// 根据选中状态刷新背景颜色。
    /// </summary>
    private void ApplySelectedState()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }

    /// <summary>
    /// 生成随从状态文字。
    /// 当前复用 canAttackText，同时显示 Ready 和关键词，避免改 Prefab。
    /// </summary>
    private string GetStatusText(Minion minion)
    {
        if (minion == null) return "";

        string statusText = minion.CanAttack ? "Ready" : "";
        string keywordsText = GetKeywordsText(minion);

        if (string.IsNullOrWhiteSpace(statusText)) return keywordsText;
        if (string.IsNullOrWhiteSpace(keywordsText)) return statusText;

        return $"{statusText} {keywordsText}";
    }

    /// <summary>
    /// 把随从当前关键词列表转成 UI 文本。
    /// </summary>
    private string GetKeywordsText(Minion minion)
    {
        if (minion == null || minion.Keywords == null) return "";

        string text = "";

        foreach (KeywordType keyword in minion.Keywords)
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
