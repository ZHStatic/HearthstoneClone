using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 套牌模板数据。
/// 一套 DeckData 对应一套预制套牌，运行时会把里面的 CardData 列表交给 Player 创建牌库。
/// </summary>
[CreateAssetMenu(fileName = "NewDeck", menuName = "HearthstoneClone/Deck Data")]
public class DeckData : ScriptableObject
{
    [SerializeField] private string deckKey = "new_deck";
    [SerializeField] private string deckName = "未命名套牌";
    [SerializeField] [TextArea] private string description = "";
    [SerializeField] private List<CardData> cards = new List<CardData>();

    /// <summary>
    /// 套牌稳定标识。
    /// 当前阶段用于选择和记录预制套牌，后续可以扩展成真正的分享码系统。
    /// </summary>
    public string DeckKey => deckKey;

    /// <summary>
    /// 玩家在套牌选择界面看到的名称。
    /// </summary>
    public string DeckName => deckName;

    /// <summary>
    /// 套牌说明，例如“低费随从压制”或“嘲讽防守反击”。
    /// </summary>
    public string Description => description;

    /// <summary>
    /// 原始牌表。允许重复 CardData，表示这套牌里有多张同名卡。
    /// </summary>
    public IReadOnlyList<CardData> Cards => cards;

    /// <summary>
    /// 有效卡牌数量。空槽位不计入数量，避免 Inspector 临时空项影响 UI 显示。
    /// </summary>
    public int CardCount => CountValidCards();

    /// <summary>
    /// 创建一份运行时可用的牌表副本。
    /// 返回新 List，避免外部直接修改 DeckData 里的模板配置。
    /// </summary>
    public List<CardData> CreateCardList()
    {
        List<CardData> result = new List<CardData>();
        if (cards == null) return result;

        foreach (CardData card in cards)
        {
            if (card == null) continue;

            result.Add(card);
        }

        return result;
    }

    private void OnValidate()
    {
        deckKey = NormalizeDeckKey(deckKey);

        if (string.IsNullOrWhiteSpace(deckName))
        {
            deckName = "未命名套牌";
        }

        if (cards == null)
        {
            cards = new List<CardData>();
        }
    }

    private int CountValidCards()
    {
        if (cards == null) return 0;

        int count = 0;
        foreach (CardData card in cards)
        {
            if (card != null)
            {
                count++;
            }
        }

        return count;
    }

    private string NormalizeDeckKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "new_deck";
        }

        return value.Trim().Replace(" ", "_");
    }
}
