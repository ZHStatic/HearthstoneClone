using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor-only helper for applying repeatable AI test decks to the scene GameManager.
/// This keeps test deck setup inside Unity serialization instead of hand-editing scene YAML.
/// </summary>
public static class AITestDeckEditorTool
{
    private const string CardFolderPath = "Assets/ScriptableObjects/Cards";
    private const string ScriptableObjectFolderPath = "Assets/ScriptableObjects";
    private const string DeckFolderPath = "Assets/ScriptableObjects/Decks";

    private static readonly DeckEntry[] ComprehensiveDeckEntries =
    {
        new DeckEntry("训练新兵", 3),
        new DeckEntry("疾风斥候", 2),
        new DeckEntry("火花", 3),
        new DeckEntry("河湾猎手", 2),
        new DeckEntry("圣盾卫士", 2),
        new DeckEntry("亡语炸弹人", 2),
        new DeckEntry("书卷侍从", 2),
        new DeckEntry("火焰学徒", 2),
        new DeckEntry("城墙守卫", 2),
        new DeckEntry("战场斗士", 1),
        new DeckEntry("岩石巨人", 1),
    };

    private static readonly string[] FixedEnemyDeckCardNames =
    {
        "训练新兵",
        "火花",
        "亡语炸弹人",
        "圣盾卫士",
        "疾风斥候",
        "河湾猎手",
        "书卷侍从",
        "城墙守卫",
        "火焰学徒",
        "战场斗士",
        "岩石巨人",
        "训练新兵",
        "火花",
        "亡语炸弹人",
        "圣盾卫士",
        "疾风斥候",
        "河湾猎手",
        "书卷侍从",
        "城墙守卫",
        "火焰学徒",
        "训练新兵",
        "火花",
    };

    [MenuItem("HearthstoneClone/AI Test Deck/Apply Comprehensive Deck To Both Players")]
    private static void ApplyComprehensiveDeckToBothPlayers()
    {
        if (!TryBuildDeck(ComprehensiveDeckEntries, out List<CardData> deck))
        {
            return;
        }

        DeckData deckData = CreateOrUpdateDeckAsset(
            "ai_test_comprehensive",
            "AI 测试综合套牌",
            "覆盖低费随从、法术、嘲讽、圣盾、战吼和亡语的综合测试套牌。",
            deck);
        if (deckData == null) return;

        ApplyDecks(deckData, deckData, "Comprehensive Deck To Both Players");
    }

    [MenuItem("HearthstoneClone/AI Test Deck/Apply Fixed Observation Deck")]
    private static void ApplyFixedObservationDeck()
    {
        if (!TryBuildDeck(ComprehensiveDeckEntries, out List<CardData> playerDeck))
        {
            return;
        }

        if (!TryBuildDeck(FixedEnemyDeckCardNames, out List<CardData> enemyDeck))
        {
            return;
        }

        DeckData playerDeckData = CreateOrUpdateDeckAsset(
            "ai_test_observation_player",
            "AI 观察玩家套牌",
            "用于 AI 观察局的玩家侧综合套牌。",
            playerDeck);
        DeckData enemyDeckData = CreateOrUpdateDeckAsset(
            "ai_test_observation_enemy",
            "AI 观察敌方套牌",
            "按固定顺序排列，便于关闭洗牌后观察 AI 起手和行动选择。",
            enemyDeck);
        if (playerDeckData == null || enemyDeckData == null) return;

        ApplyDecks(playerDeckData, enemyDeckData, "Fixed Observation Deck");
    }

    private static void ApplyDecks(DeckData playerDeck, DeckData enemyDeck, string presetName)
    {
        GameManager gameManager = FindGameManagerInScene();
        if (gameManager == null)
        {
            Debug.LogError("AI Test Deck: 当前打开的场景里没有找到 GameManager。");
            return;
        }

        SerializedObject serializedGameManager = new SerializedObject(gameManager);
        bool wrotePlayerDeck = WriteObjectProperty(serializedGameManager, "defaultPlayerDeck", playerDeck);
        bool wroteEnemyDeck = WriteObjectProperty(serializedGameManager, "defaultEnemyDeck", enemyDeck);
        if (!wrotePlayerDeck || !wroteEnemyDeck)
        {
            return;
        }

        SetDebugOptions(serializedGameManager);
        serializedGameManager.ApplyModifiedProperties();

        EditorUtility.SetDirty(gameManager);
        EditorSceneManager.MarkSceneDirty(gameManager.gameObject.scene);

        LogAppliedDeck(presetName, playerDeck, enemyDeck);
    }

    private static GameManager FindGameManagerInScene()
    {
        return Object.FindObjectOfType<GameManager>();
    }

    private static bool TryBuildDeck(DeckEntry[] entries, out List<CardData> deck)
    {
        deck = new List<CardData>();

        foreach (DeckEntry entry in entries)
        {
            CardData cardData = LoadCard(entry.CardName);
            if (cardData == null)
            {
                deck.Clear();
                return false;
            }

            for (int i = 0; i < entry.Count; i++)
            {
                deck.Add(cardData);
            }
        }

        return true;
    }

    private static bool TryBuildDeck(string[] cardNames, out List<CardData> deck)
    {
        deck = new List<CardData>();

        foreach (string cardName in cardNames)
        {
            CardData cardData = LoadCard(cardName);
            if (cardData == null)
            {
                deck.Clear();
                return false;
            }

            deck.Add(cardData);
        }

        return true;
    }

    private static CardData LoadCard(string cardName)
    {
        string assetPath = $"{CardFolderPath}/{cardName}.asset";
        CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);
        if (cardData == null)
        {
            Debug.LogError($"AI Test Deck: 找不到卡牌资源 {assetPath}。请确认 CardData asset 名称没有改动。");
        }

        return cardData;
    }

    private static DeckData CreateOrUpdateDeckAsset(string deckKey, string deckName, string description, List<CardData> cards)
    {
        EnsureDeckFolder();

        string assetPath = $"{DeckFolderPath}/{deckKey}.asset";
        DeckData deckData = AssetDatabase.LoadAssetAtPath<DeckData>(assetPath);
        if (deckData == null)
        {
            deckData = ScriptableObject.CreateInstance<DeckData>();
            AssetDatabase.CreateAsset(deckData, assetPath);
        }

        SerializedObject serializedDeck = new SerializedObject(deckData);
        bool wroteKey = WriteStringProperty(serializedDeck, "deckKey", deckKey);
        bool wroteName = WriteStringProperty(serializedDeck, "deckName", deckName);
        bool wroteDescription = WriteStringProperty(serializedDeck, "description", description);
        bool wroteCards = WriteCardListProperty(serializedDeck, "cards", cards);
        if (!wroteKey || !wroteName || !wroteDescription || !wroteCards)
        {
            Debug.LogError($"AI Test Deck: 写入套牌资源失败 {assetPath}。");
            return null;
        }

        serializedDeck.ApplyModifiedProperties();
        EditorUtility.SetDirty(deckData);
        AssetDatabase.SaveAssets();
        return deckData;
    }

    private static void EnsureDeckFolder()
    {
        if (!AssetDatabase.IsValidFolder(ScriptableObjectFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }

        if (!AssetDatabase.IsValidFolder(DeckFolderPath))
        {
            AssetDatabase.CreateFolder(ScriptableObjectFolderPath, "Decks");
        }
    }

    private static bool WriteStringProperty(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || property.propertyType != SerializedPropertyType.String)
        {
            Debug.LogError($"AI Test Deck: 找不到 string 字段 {propertyName}。");
            return false;
        }

        property.stringValue = value ?? "";
        return true;
    }

    private static bool WriteCardListProperty(SerializedObject serializedObject, string propertyName, List<CardData> deck)
    {
        SerializedProperty deckProperty = serializedObject.FindProperty(propertyName);
        if (deckProperty == null || !deckProperty.isArray)
        {
            Debug.LogError($"AI Test Deck: 找不到套牌字段 {propertyName}。");
            return false;
        }

        deckProperty.arraySize = deck.Count;
        for (int i = 0; i < deck.Count; i++)
        {
            deckProperty.GetArrayElementAtIndex(i).objectReferenceValue = deck[i];
        }

        return true;
    }

    private static bool WriteObjectProperty(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
        {
            Debug.LogError($"AI Test Deck: 找不到对象引用字段 {propertyName}。");
            return false;
        }

        property.objectReferenceValue = value;
        return true;
    }

    private static void SetDebugOptions(SerializedObject serializedObject)
    {
        SetBoolProperty(serializedObject, "disableDeckShuffleForDebug", true);
        SetBoolProperty(serializedObject, "logAIHandOnTurnStart", true);
        SetBoolProperty(serializedObject, "logSnapshotEvaluationOnTurnStart", true);
        SetBoolProperty(serializedObject, "logSnapshotSimulationOnTurnStart", true);
        SetBoolProperty(serializedObject, "logLegalActionsOnTurnStart", false);
        SetBoolProperty(serializedObject, "enableEnemyAI", true);
    }

    private static void SetBoolProperty(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || property.propertyType != SerializedPropertyType.Boolean)
        {
            Debug.LogWarning($"AI Test Deck: 找不到 bool 字段 {propertyName}，已跳过。");
            return;
        }

        property.boolValue = value;
    }

    private static string BuildAppliedDeckLog(string presetName, DeckData playerDeck, DeckData enemyDeck)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"AI Test Deck: 已应用 {presetName}。");
        builder.AppendLine($"Player Deck: {BuildDeckLabel(playerDeck)}");
        builder.AppendLine($"Enemy Deck: {BuildDeckLabel(enemyDeck)}");
        builder.Append("调试开关：关闭洗牌、打印 AI 手牌、打印快照评分、打印快照模拟。确认 Inspector 后请手动保存场景。");
        return builder.ToString();
    }

    private static void LogAppliedDeck(string presetName, DeckData playerDeck, DeckData enemyDeck)
    {
        Debug.Log(BuildAppliedDeckLog(presetName, playerDeck, enemyDeck));
    }

    private static string BuildDeckLabel(DeckData deckData)
    {
        if (deckData == null)
        {
            return "未配置";
        }

        return $"{deckData.DeckName} ({deckData.DeckKey}) - {deckData.CardCount} 张 - {BuildDeckSummary(deckData.Cards)}";
    }

    private static string BuildDeckSummary(IReadOnlyList<CardData> deck)
    {
        if (deck == null) return "空";

        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (CardData cardData in deck)
        {
            string cardName = cardData != null ? cardData.CardName : "未知卡牌";
            if (!counts.ContainsKey(cardName))
            {
                counts[cardName] = 0;
            }

            counts[cardName]++;
        }

        List<string> parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in counts)
        {
            parts.Add($"{pair.Key}x{pair.Value}");
        }

        return string.Join(" | ", parts);
    }

    private struct DeckEntry
    {
        public string CardName { get; private set; }
        public int Count { get; private set; }

        public DeckEntry(string cardName, int count)
        {
            CardName = cardName;
            Count = count;
        }
    }
}
