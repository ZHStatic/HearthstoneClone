using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    [SerializeField] private BattlecryType battlecryType = BattlecryType.None;
    [SerializeField] [FormerlySerializedAs("battlecryDamage")] private int battlecryValue = 0;
    [SerializeField] private DeathrattleType deathrattleType = DeathrattleType.None;
    [SerializeField] private int deathrattleValue = 0;

    // 这张卡牌模板拥有的关键词。
    // 用 List 是为了之后支持一张牌同时拥有多个关键词，例如冲锋 + 嘲讽。
    [SerializeField] private List<KeywordType> keywords = new List<KeywordType>();
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
    public BattlecryType BattlecryType => battlecryType;
    public int BattlecryValue => battlecryValue;
    public bool HasBattlecry => battlecryType != BattlecryType.None;
    public DeathrattleType DeathrattleType => deathrattleType;
    public int DeathrattleValue => deathrattleValue;
    public bool HasDeathrattle => deathrattleType != DeathrattleType.None;

    // 外部只能读取关键词列表，不能直接替换整个列表。
    public IReadOnlyList<KeywordType> Keywords => keywords;
    public string Description => description;

    /// <summary>
    /// 查询这张卡牌模板是否拥有指定关键词。
    /// </summary>
    public bool HasKeyword(KeywordType keyword)
    {
        if (keyword == KeywordType.None) return false;
        if (keywords == null) return false;

        return keywords.Contains(keyword);
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(cardName))
        {
            cardName = "未命名卡牌";
        }

        cost = Mathf.Max(0, cost);
        attack = Mathf.Max(0, attack);
        spellDamage = Mathf.Max(0, spellDamage);
        battlecryValue = Mathf.Max(0, battlecryValue);
        deathrattleValue = Mathf.Max(0, deathrattleValue);

        health = cardType == CardType.Minion
            ? Mathf.Max(1, health)
            : Mathf.Max(0, health);

        CleanKeywords();
    }

    /// <summary>
    /// 清理 Inspector 中的关键词配置，避免重复保存有效关键词。
    /// None 需要保留为编辑期占位，否则 Unity Inspector 点 + 后会立刻被清掉。
    /// </summary>
    private void CleanKeywords()
    {
        if (keywords == null)
        {
            keywords = new List<KeywordType>();
            return;
        }

        HashSet<KeywordType> seenKeywords = new HashSet<KeywordType>();
        List<KeywordType> cleanedKeywords = new List<KeywordType>();

        // HashSet 用来记录已经出现过的关键词，List 用来保留最终的 Inspector 显示顺序。
        foreach (KeywordType keyword in keywords)
        {
            if (keyword == KeywordType.None)
            {
                cleanedKeywords.Add(keyword);
                continue;
            }

            bool isNewKeyword = seenKeywords.Add(keyword);
            if (!isNewKeyword) continue;

            cleanedKeywords.Add(keyword);
        }

        keywords = cleanedKeywords;
    }
}
