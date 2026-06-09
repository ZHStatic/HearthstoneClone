using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows one minion on the board.
/// Attack interaction will be added later.
/// </summary>
public class MinionView : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text attackText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text canAttackText;

    private Minion minion;

    public void SetMinion(Minion minion)
    {
        this.minion = minion;
        Refresh();
    }

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

    public void Clear()
    {
        minion = null;

        SetText(nameText, "");
        SetText(attackText, "");
        SetText(healthText, "");
        SetText(canAttackText, "");
    }

    private void SetText(Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }
}
