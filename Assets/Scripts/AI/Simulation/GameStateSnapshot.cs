/// <summary>
/// AI 模拟用的完整局面快照。
/// 它从真实对局复制必要数据，后续模拟动作时不会修改 GameManager 里的真实状态。
/// </summary>
public class GameStateSnapshot
{
    public const int PlayerIndex = 0;
    public const int EnemyIndex = 1;

    /// <summary>
    /// 当前行动方索引。
    /// 0 表示 Player，1 表示 Enemy。
    /// </summary>
    public int CurrentPlayerIndex { get; private set; }

    /// <summary>
    /// 玩家侧状态快照。
    /// </summary>
    public PlayerSnapshot Player { get; private set; }

    /// <summary>
    /// 敌人侧状态快照。
    /// </summary>
    public PlayerSnapshot Enemy { get; private set; }

    /// <summary>
    /// 双方战场快照。
    /// </summary>
    public BoardSnapshot Board { get; private set; }

    public GameStateSnapshot(int currentPlayerIndex, PlayerSnapshot player, PlayerSnapshot enemy, BoardSnapshot board)
    {
        CurrentPlayerIndex = currentPlayerIndex == EnemyIndex ? EnemyIndex : PlayerIndex;
        Player = player;
        Enemy = enemy;
        Board = board;
    }

    /// <summary>
    /// 从真实 GameManager 复制一份 AI 模拟用快照。
    /// 当前只复制 AI 评估和第一版动作模拟需要的轻量状态。
    /// </summary>
    public static GameStateSnapshot FromGameManager(GameManager gameManager)
    {
        if (gameManager == null)
        {
            return null;
        }

        int currentPlayerIndex = gameManager.CurrentPlayer == gameManager.Enemy
            ? EnemyIndex
            : PlayerIndex;

        PlayerSnapshot playerSnapshot = PlayerSnapshot.FromPlayer(gameManager.Player);
        PlayerSnapshot enemySnapshot = PlayerSnapshot.FromPlayer(gameManager.Enemy);
        BoardSnapshot boardSnapshot = BoardSnapshot.FromBoard(gameManager.Board, gameManager.Player, gameManager.Enemy);

        return new GameStateSnapshot(currentPlayerIndex, playerSnapshot, enemySnapshot, boardSnapshot);
    }

    /// <summary>
    /// 获取当前行动方的玩家快照。
    /// </summary>
    public PlayerSnapshot GetCurrentPlayer()
    {
        return CurrentPlayerIndex == EnemyIndex ? Enemy : Player;
    }

    /// <summary>
    /// 获取当前行动方的对手快照。
    /// </summary>
    public PlayerSnapshot GetOpponentPlayer()
    {
        return CurrentPlayerIndex == EnemyIndex ? Player : Enemy;
    }
}
