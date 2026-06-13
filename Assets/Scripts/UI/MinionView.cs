using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单个随从 UI。
/// 当前阶段只负责显示随从数据，攻击点击交互之后再接入。
/// </summary>
public class MinionView : MonoBehaviour
{
    // 在 Inspector 中绑定的文字组件。
    // 它们显示随从名、攻击、生命和是否可以攻击。
    [SerializeField] private Text nameText;
    [SerializeField] private Text attackText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text canAttackText;

    // 当前这张 UI 正在显示的运行时随从。
    private Minion minion;

    /// <summary>
    /// 设置这张 UI 要显示的随从。
    /// </summary>
    public void SetMinion(Minion minion)
    {
        this.minion = minion;
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
        SetText(canAttackText, minion.CanAttack ? "Ready" : "");
    }

    /// <summary>
    /// 清空当前显示内容。
    /// </summary>
    public void Clear()
    {
        minion = null;

        SetText(nameText, "");
        SetText(attackText, "");
        SetText(healthText, "");
        SetText(canAttackText, "");
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
