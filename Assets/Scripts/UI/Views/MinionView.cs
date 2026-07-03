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

    // 用背景颜色显示选中、合法目标和非法目标状态。没有绑定时会自动尝试从当前物体上获取 Image。
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.9f, 0.35f, 1f);
    [SerializeField] private Color validTargetColor = new Color(0.45f, 1f, 0.45f, 1f);
    [SerializeField] private Color invalidTargetColor = new Color(1f, 0.55f, 0.55f, 1f);

    // 接收点击的按钮组件。
    // 如果 Inspector 没有绑定，Awake 会尝试从当前物体上自动获取。
    [SerializeField] private Button button;

    // 当前这张 UI 正在显示的运行时随从。
    private Minion minion;

    // 点击回调：MinionView 不自己处理攻击，只把被点击的 Minion 交给上层。
    private Action<Minion> onClicked;
    private TargetHighlightState highlightState = TargetHighlightState.None;

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
        ApplyHighlightState();

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
        SetHighlightState(isSelected ? TargetHighlightState.Selected : TargetHighlightState.None);
    }

    /// <summary>
    /// 设置这个随从 UI 的目标高亮状态。
    /// MinionView 只根据传入状态改变显示，不判断这个目标是否合法。
    /// </summary>
    public void SetHighlightState(TargetHighlightState highlightState)
    {
        this.highlightState = highlightState;
        ApplyHighlightState();
    }

    /// <summary>
    /// 清空当前显示内容。
    /// </summary>
    public void Clear()
    {
        minion = null;
        onClicked = null;
        highlightState = TargetHighlightState.None;

        SetText(nameText, "");
        SetText(attackText, "");
        SetText(healthText, "");
        SetText(canAttackText, "");
        ApplyHighlightState();

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
    /// 根据高亮状态刷新背景颜色。
    /// </summary>
    private void ApplyHighlightState()
    {
        if (backgroundImage == null) return;

        backgroundImage.color = highlightState switch
        {
            TargetHighlightState.Selected => selectedColor,
            TargetHighlightState.Valid => validTargetColor,
            TargetHighlightState.Invalid => invalidTargetColor,
            _ => normalColor,
        };
    }

    /// <summary>
    /// 生成随从状态文字。
    /// 当前复用 canAttackText，同时显示 Ready 和关键词，避免改 Prefab。
    /// </summary>
    private string GetStatusText(Minion minion)
    {
        if (minion == null) return "";

        string statusText = "";

        AddStatusText(ref statusText, minion.CanAttack ? "Ready" : "");
        AddStatusText(ref statusText, GetKeywordsText(minion));
        AddStatusText(ref statusText, GetDeathrattleText(minion));

        return statusText;
    }

    /// <summary>
    /// 把随从当前关键词列表转成 UI 文本。
    /// </summary>
    private string GetKeywordsText(Minion minion)
    {
        if (minion == null) return "";

        return KeywordTextFormatter.BuildKeywordsText(minion.Keywords);
    }

    /// <summary>
    /// 把随从的亡语配置转成 UI 文本。
    /// </summary>
    private string GetDeathrattleText(Minion minion)
    {
        if (minion == null || minion.CardData == null) return "";
        if (!minion.CardData.HasDeathrattle) return "";

        return minion.CardData.DeathrattleType switch
        {
            DeathrattleType.DealDamageToEnemyHero => $"亡语:{minion.CardData.DeathrattleValue}",
            _ => "",
        };
    }

    /// <summary>
    /// 向状态文本中拼接一段非空内容。
    /// </summary>
    private void AddStatusText(ref string statusText, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        if (!string.IsNullOrWhiteSpace(statusText))
        {
            statusText += " ";
        }

        statusText += text;
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
