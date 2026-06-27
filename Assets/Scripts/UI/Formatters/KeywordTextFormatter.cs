using System.Collections.Generic;

/// <summary>
/// UI 层关键词文本格式化工具。
/// 只负责把关键词枚举转换成玩家能看懂的显示文本。
/// </summary>
public static class KeywordTextFormatter
{
    /// <summary>
    /// 把关键词枚举转成中文显示文本。
    /// </summary>
    public static string GetKeywordText(KeywordType keyword)
    {
        return keyword switch
        {
            KeywordType.Charge => "冲锋",
            KeywordType.Taunt => "嘲讽",
            KeywordType.DivineShield => "圣盾",
            _ => "",
        };
    }

    /// <summary>
    /// 把关键词列表转成空格分隔的文本。
    /// </summary>
    public static string BuildKeywordsText(IEnumerable<KeywordType> keywords)
    {
        if (keywords == null) return "";

        List<string> keywordTexts = new List<string>();
        foreach (KeywordType keyword in keywords)
        {
            string keywordText = GetKeywordText(keyword);
            if (string.IsNullOrWhiteSpace(keywordText)) continue;

            keywordTexts.Add(keywordText);
        }

        return string.Join(" ", keywordTexts);
    }
}
