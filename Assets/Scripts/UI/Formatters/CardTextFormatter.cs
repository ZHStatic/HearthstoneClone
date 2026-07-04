using System.Collections.Generic;

/// <summary>
/// UI 层卡牌效果文本格式化工具。
/// 只负责把卡牌模板上的类型、法术、战吼和亡语配置转换成显示文本。
/// </summary>
public static class CardTextFormatter
{
    /// <summary>
    /// 把卡牌类型转换成玩家能看懂的显示文本。
    /// </summary>
    public static string GetCardTypeText(CardData cardData)
    {
        if (cardData == null) return "";

        return cardData.CardType switch
        {
            CardType.Minion => "随从",
            CardType.Spell => "法术",
            _ => "",
        };
    }

    /// <summary>
    /// 生成法术效果显示文本。
    /// 当前只支持单目标伤害法术，所以先显示伤害值。
    /// </summary>
    public static string GetSpellEffectText(CardData cardData)
    {
        if (cardData == null) return "";
        if (cardData.CardType != CardType.Spell) return "";
        if (cardData.SpellDamage <= 0) return "";

        return $"造成 {cardData.SpellDamage} 点伤害";
    }

    /// <summary>
    /// 生成卡牌规则效果文本。
    /// 手牌上的 EffectText 统一显示关键词、法术效果、战吼和亡语。
    /// </summary>
    public static string GetCardEffectText(CardData cardData)
    {
        if (cardData == null) return "";

        List<string> lines = new List<string>();

        AddLine(lines, KeywordTextFormatter.BuildKeywordsText(cardData.Keywords));
        AddLine(lines, GetSpellEffectText(cardData));
        AddLine(lines, GetBattlecryText(cardData));
        AddLine(lines, GetDeathrattleText(cardData));

        return string.Join("\n", lines);
    }

    /// <summary>
    /// 生成战吼显示文本。
    /// 战吼是出牌时触发的一次性效果，主要显示在手牌描述区。
    /// </summary>
    public static string GetBattlecryText(CardData cardData)
    {
        if (cardData == null) return "";
        if (!cardData.HasBattlecry) return "";

        return cardData.BattlecryType switch
        {
            BattlecryType.DealDamageToEnemyHero => $"战吼：对敌方英雄造成 {cardData.BattlecryValue} 点伤害",
            BattlecryType.DrawCard => $"战吼：抽 {cardData.BattlecryValue} 张牌",
            _ => "",
        };
    }

    /// <summary>
    /// 生成手牌描述区使用的亡语完整文本。
    /// </summary>
    public static string GetDeathrattleText(CardData cardData)
    {
        if (cardData == null) return "";
        if (!cardData.HasDeathrattle) return "";

        return cardData.DeathrattleType switch
        {
            DeathrattleType.DealDamageToEnemyHero => $"亡语：对敌方英雄造成 {cardData.DeathrattleValue} 点伤害",
            _ => "",
        };
    }

    /// <summary>
    /// 生成场上随从使用的亡语短文本。
    /// </summary>
    public static string GetDeathrattleSummaryText(CardData cardData)
    {
        if (cardData == null) return "";
        if (!cardData.HasDeathrattle) return "";

        return cardData.DeathrattleType switch
        {
            DeathrattleType.DealDamageToEnemyHero => $"亡语:{cardData.DeathrattleValue}",
            _ => "",
        };
    }

    private static void AddLine(List<string> lines, string line)
    {
        if (lines == null) return;
        if (string.IsNullOrWhiteSpace(line)) return;

        lines.Add(line);
    }
}
