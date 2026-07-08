using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 套牌选择 UI 控制器。
/// 负责显示预制套牌选项，并把玩家选择交给 GameManager 开始新对局。
/// 它不创建牌库、不修改 Player 状态，也不直接处理对局规则。
/// 当前阶段采用“同一场景里切换 Panel”的做法，这是阶段性简化：
/// 先把主流程跑通，之后再考虑主菜单场景、结算场景或更完整的 UI 流程管理。
/// </summary>
public class DeckSelectionController : MonoBehaviour
{
    // 对局核心入口。
    // 套牌选择完成后，只调用 GameManager.StartNewGame(...)，真正的玩家、牌库、战场创建仍然由 Core 负责。
    [SerializeField] private GameManager gameManager;

    // 战斗 UI 入口。
    // 新对局创建完后，需要让战斗界面重新读取 GameManager 的当前状态并刷新显示。
    [SerializeField] private GameUIController gameUIController;

    // 玩家可以选择的预制套牌。
    // 在 Unity Inspector 中把多个 DeckData.asset 拖进来；数组下标会和 deckOptionViews 对应。
    [SerializeField] private DeckData[] availableDecks;

    // AI 使用的默认套牌。
    // 当前阶段先让 AI 固定使用一套牌，方便验证“玩家选不同套牌后能进入战斗”这条主流程。
    [SerializeField] private DeckData defaultEnemyDeck;

    // 每个 DeckOptionView 对应屏幕上的一个套牌选项。
    // 一个选项内部自己绑定 Button、NameText、DescriptionText 和 CountText。
    [SerializeField] private DeckOptionView[] deckOptionViews;

    // 套牌选择区域。
    // 玩家还没开始对局，或从后续主菜单/结算界面返回时显示它。
    [SerializeField] private GameObject selectionPanel;

    // 战斗区域。
    // 点击“开始”后显示它；具体战斗内容仍由 GameUIController 刷新。
    [SerializeField] private GameObject battlePanel;

    // 胜负结算区域。
    // 对局结束后显示它；按钮可以绑定“再来一局”和“回到套牌选择”。
    [SerializeField] private GameObject settlementPanel;

    // 结算结果文本。
    // 只显示胜利、失败或平局，不承载普通战斗反馈。
    [SerializeField] private Text settlementResultText;

    // 结算按钮。
    // 如果这里绑定了 Button 字段，代码会自动注册点击事件；不要再在 Inspector OnClick 里重复绑定同一个方法。
    [SerializeField] private Button restartBattleButton;
    [SerializeField] private Button backToDeckSelectionButton;

    // 当前选中的玩家套牌下标。
    // -1 表示还没有选中任何有效套牌。
    private int selectedDeckIndex = -1;

    // 当前是否已经进入结算界面。
    // 防止 GameUIController 每次刷新 GameOverText 时重复切换 Panel。
    private bool isShowingSettlement;

    /// <summary>
    /// Unity 生命周期方法：脚本启用后自动执行一次。
    /// 这里负责补齐引用、配置套牌选项，并默认选中第一套有效套牌。
    /// </summary>
    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameUIController == null)
        {
            gameUIController = FindObjectOfType<GameUIController>();
        }

        SetupDeckOptionViews();
        SelectFirstAvailableDeck();
        RegisterSettlementButtons();
        ShowDeckSelection();
    }

    private void OnDestroy()
    {
        UnregisterSettlementButtons();
    }

    /// <summary>
    /// 选择指定下标的套牌。
    /// 可由套牌按钮 OnClick 调用，也可由代码在初始化时调用。
    /// 这里只改变 UI 选择状态，不开始游戏、不创建牌库。
    /// </summary>
    public void SelectDeck(int index)
    {
        if (!IsValidDeckIndex(index))
        {
            selectedDeckIndex = -1;
            RefreshDeckOptions();
            return;
        }

        selectedDeckIndex = index;
        RefreshDeckOptions();
    }

    /// <summary>
    /// 用当前选中的玩家套牌和默认 AI 套牌开始一局新对局。
    /// 这是“开始游戏”按钮应该绑定的入口。
    /// 如果没有单独配置 AI 套牌，当前阶段会临时让 AI 使用玩家选中的同一套牌，避免流程被空引用卡住。
    /// </summary>
    public void StartBattleWithSelectedDeck()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("DeckSelectionController: 没有绑定 GameManager，无法开始对局。");
            return;
        }

        DeckData playerDeck = GetSelectedDeck();
        if (playerDeck == null)
        {
            Debug.LogWarning("DeckSelectionController: 没有选中有效套牌，无法开始对局。");
            ShowDeckSelection();
            return;
        }

        DeckData enemyDeck = defaultEnemyDeck != null ? defaultEnemyDeck : playerDeck;
        gameManager.StartNewGame(playerDeck, enemyDeck);

        if (gameUIController != null)
        {
            gameUIController.RefreshAll();
        }

        ShowBattle();
    }

    /// <summary>
    /// 显示套牌选择界面。
    /// 后续主菜单或结算界面可以复用这个入口返回选牌。
    /// 它只切换 UI Panel，不会清理当前对局；真正的重新开局发生在 StartBattleWithSelectedDeck()。
    /// </summary>
    public void ShowDeckSelection()
    {
        isShowingSettlement = false;
        RefreshDeckOptions();
        SetPanelActive(selectionPanel, true);
        SetPanelActive(battlePanel, false);
        SetPanelActive(settlementPanel, false);
    }

    /// <summary>
    /// 显示战斗界面。
    /// 它只切换 Panel，不创建对局；创建对局仍然由 StartBattleWithSelectedDeck() 或 RestartBattleWithSelectedDeck() 完成。
    /// </summary>
    public void ShowBattle()
    {
        isShowingSettlement = false;
        SetPanelActive(selectionPanel, false);
        SetPanelActive(battlePanel, true);
        SetPanelActive(settlementPanel, false);
    }

    /// <summary>
    /// 显示胜负结算界面。
    /// GameUIController 检测到 GameManager.IsGameOver 后可以调用这个入口。
    /// </summary>
    public void ShowSettlement()
    {
        if (isShowingSettlement) return;

        isShowingSettlement = true;
        RefreshSettlementResultText();
        SetPanelActive(selectionPanel, false);
        SetPanelActive(battlePanel, false);
        SetPanelActive(settlementPanel, true);
    }

    /// <summary>
    /// 使用当前选中的玩家套牌重新开始一局。
    /// 这是“再来一局”按钮应该绑定的入口。
    /// </summary>
    public void RestartBattleWithSelectedDeck()
    {
        StartBattleWithSelectedDeck();
    }

    /// <summary>
    /// 回到套牌选择界面。
    /// 这里只切换 UI，不主动清理当前对局；下一次点击开始时会创建新对局。
    /// </summary>
    public void ReturnToDeckSelection()
    {
        ShowDeckSelection();
    }

    /// <summary>
    /// 根据当前配置刷新所有套牌选项的文案和按钮状态。
    /// 这个方法容忍数组长度不一致：某个 Text 或 Button 没绑时只跳过，不让 UI 因为空引用报错。
    /// </summary>
    public void RefreshDeckOptions()
    {
        int optionCount = GetDeckOptionCount();
        for (int i = 0; i < optionCount; i++)
        {
            DeckOptionView optionView = GetDeckOptionViewAt(i);
            if (optionView != null)
            {
                optionView.Refresh(i == selectedDeckIndex);
            }
        }
    }

    private void SetupDeckOptionViews()
    {
        int optionCount = GetDeckOptionCount();
        for (int i = 0; i < optionCount; i++)
        {
            DeckOptionView optionView = GetDeckOptionViewAt(i);
            if (optionView != null)
            {
                optionView.SetOption(i, GetDeckAt(i), SelectDeck);
            }
        }
    }

    private void SelectFirstAvailableDeck()
    {
        if (IsValidDeckIndex(selectedDeckIndex))
        {
            RefreshDeckOptions();
            return;
        }

        int optionCount = GetDeckOptionCount();
        for (int i = 0; i < optionCount; i++)
        {
            // 找到第一套真实配置的 DeckData 作为默认选择，减少玩家进入界面后的额外操作。
            if (GetDeckAt(i) != null)
            {
                SelectDeck(i);
                return;
            }
        }

        selectedDeckIndex = -1;
        RefreshDeckOptions();
    }

    private void RegisterSettlementButtons()
    {
        if (restartBattleButton != null)
        {
            restartBattleButton.onClick.AddListener(RestartBattleWithSelectedDeck);
        }

        if (backToDeckSelectionButton != null)
        {
            backToDeckSelectionButton.onClick.AddListener(ReturnToDeckSelection);
        }
    }

    private void UnregisterSettlementButtons()
    {
        if (restartBattleButton != null)
        {
            restartBattleButton.onClick.RemoveListener(RestartBattleWithSelectedDeck);
        }

        if (backToDeckSelectionButton != null)
        {
            backToDeckSelectionButton.onClick.RemoveListener(ReturnToDeckSelection);
        }
    }

    private void RefreshSettlementResultText()
    {
        if (settlementResultText == null) return;

        if (gameManager == null || !gameManager.IsGameOver)
        {
            settlementResultText.text = "";
            return;
        }

        if (gameManager.Winner == null)
        {
            settlementResultText.text = "平局";
            return;
        }

        settlementResultText.text = gameManager.Winner == gameManager.Player
            ? "胜利"
            : "失败";
    }

    private DeckData GetSelectedDeck()
    {
        return IsValidDeckIndex(selectedDeckIndex)
            ? availableDecks[selectedDeckIndex]
            : null;
    }

    private bool IsValidDeckIndex(int index)
    {
        if (availableDecks == null) return false;
        if (index < 0 || index >= availableDecks.Length) return false;

        return availableDecks[index] != null;
    }

    private DeckData GetDeckAt(int index)
    {
        if (availableDecks == null) return null;
        if (index < 0 || index >= availableDecks.Length) return null;

        return availableDecks[index];
    }

    private int GetDeckOptionCount()
    {
        // 这里取 DeckData 和 DeckOptionView 中较长的长度。
        // 这样 UI 选项比 DeckData 多时，也能显示“未配置套牌”，方便在 Inspector 中发现漏绑。
        int count = GetArrayLength(availableDecks);
        count = Mathf.Max(count, GetArrayLength(deckOptionViews));

        return count;
    }

    private int GetArrayLength<T>(T[] array)
    {
        return array != null ? array.Length : 0;
    }

    private DeckOptionView GetDeckOptionViewAt(int index)
    {
        if (deckOptionViews == null) return null;
        if (index < 0 || index >= deckOptionViews.Length) return null;

        return deckOptionViews[index];
    }

    private void SetPanelActive(GameObject panel, bool isActive)
    {
        // Panel 是可选绑定。
        // 如果后续改成多场景流程，这两个字段可以不绑，脚本仍然能负责套牌选择和开局。
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }
}
