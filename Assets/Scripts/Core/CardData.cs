using UnityEngine;

/// <summary>
/// 卡牌模板数据 — ScriptableObject
/// 每张卡牌在硬盘上是一个 .asset 文件，存储它的"出厂参数"
/// 运行时 Card 类会引用这里的模板来创建实例
/// </summary>
[CreateAssetMenu(fileName = "NewCard", menuName = "HearthstoneClone/Card Data")]
public class CardData : ScriptableObject
{
    [SerializeField] private string cardName = "未命名卡牌";
    [SerializeField] private CardType cardType = CardType.Minion;
    [SerializeField] private int cost = 1;
    [SerializeField] private int attack = 1;
    [SerializeField] private int health = 1;
    [SerializeField] private int spellDamage = 0;
    [SerializeField] private SpellTargetType spellTargetType = SpellTargetType.None;
    [SerializeField] [TextArea] private string description = "";

    // 公开只读属性 — 外部能读，但不能改
    // 模板数据不应该在运行时被修改
    public string CardName => cardName;
    public CardType CardType => cardType;
    public int Cost => cost;
    public int Attack => attack;
    public int Health => health;
    public int SpellDamage => spellDamage;
    public SpellTargetType SpellTargetType => spellTargetType;
    public string Description => description;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            cardName = "未命名卡牌";
        }

        cost = Mathf.Max(0, cost);
        attack = Mathf.Max(0, attack);
        spellDamage = Mathf.Max(0, spellDamage);

        health = cardType == CardType.Minion
            ? Mathf.Max(1, health)
            : Mathf.Max(0, health);
    }
}
