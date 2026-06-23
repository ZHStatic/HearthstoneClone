using UnityEngine;

/// <summary>
/// 卡牌运行时实例 — 存于手牌或牌库中
/// 引用一张 CardData 模板，作为"这张卡原本是谁"的凭证
/// </summary>
public class Card
{
    // 指向模板数据（只读，运行时不会换模板）
    public CardData CardData { get; private set; }

    // 当前法力消耗（可能被效果临时修改，初始 = 模板消耗）
    public int CurrentCost { get; set; }

    /// <summary>
    /// 构造函数 — 从模板创建一张卡牌实例
    /// </summary>
    /// <param name="data">ScriptableObject 模板</param>
    public Card(CardData data)
    {
        CardData = data;
        CurrentCost = data.Cost;
    }

    /// <summary>
    /// 把当前消耗重置为模板消耗（回合开始时调用）
    /// </summary>
    public void ResetCost()
    {
        CurrentCost = CardData.Cost;
    }

    /// <summary>
    /// 方便调试：打印卡牌名称
    /// </summary>
    public override string ToString()
    {
        return $"{CardData.CardName} ({CurrentCost}费)";
    }
}
