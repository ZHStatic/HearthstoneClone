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
    [SerializeField] private int cost = 1;
    [SerializeField] private int attack = 1;
    [SerializeField] private int health = 1;
    [SerializeField] [TextArea] private string description = "";

    // 公开只读属性 — 外部能读，但不能改
    // 模板数据不应该在运行时被修改
    public string CardName => cardName;
    public int Cost => cost;
    public int Attack => attack;
    public int Health => health;
    public string Description => description;
}
