using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对局管理器。
/// 负责一局游戏的初始化、回合切换、出牌、攻击、死亡清理和胜负判断。
/// 之后 UI 可以调用它的方法，但它本身不直接依赖 UI。
/// </summary>
public class GameManager : MonoBehaviour
{
    // 在 Inspector 里配置的双方初始牌库模板。
    // 每个元素都是一个 CardData asset，运行时会被 Player 转成 Card 实例。
    [SerializeField] private List<CardData> playerDeckData = new List<CardData>();
    [SerializeField] private List<CardData> enemyDeckData = new List<CardData>();

    // 开局每名玩家抽几张牌。先用 3，之后如果要还原炉石规则可以再扩展。
    [SerializeField] private int startingHandCount = 3;

    // 运行时对象：进入 Play 模式后由 StartNewGame 创建。
    public Player Player { get; private set; }
    public Player Enemy { get; private set; }
    public Board Board { get; private set; }

    // 当前正在行动的玩家，以及本局游戏的结果状态。
    public Player CurrentPlayer { get; private set; }
    public Player Winner { get; private set; }
    public int TurnNumber { get; private set; }
    public bool IsGameOver { get; private set; }

    // Unity 生命周期方法：Awake 会早于其他脚本的 Start 执行。
    // 这样 UI 脚本在 Start 里刷新时，Player / Enemy / Board 已经创建好了。
    private void Awake()
    {
        StartNewGame();
    }

    /// <summary>
    /// 开始一局新游戏：创建玩家、战场、抽起手牌，然后进入玩家的第一个回合。
    /// </summary>
    public void StartNewGame()
    {
        Player = new Player(playerDeckData, "Player");
        Enemy = new Player(enemyDeckData, "Enemy");
        Board = new Board(Player, Enemy);

        CurrentPlayer = Player;
        Winner = null;
        TurnNumber = 0;
        IsGameOver = false;

        DrawStartingHands(Player);
        DrawStartingHands(Enemy);
        StartTurn(CurrentPlayer);
    }

    /// <summary>
    /// 给指定玩家抽起手牌。
    /// </summary>
    public void DrawStartingHands(Player targetPlayer)
    {
        if (targetPlayer == null) return;

        int drawCount = startingHandCount > 0 ? startingHandCount : 0;
        for (int i = 0; i < drawCount; i++)
        {
            targetPlayer.DrawCard();
        }
    }

    /// <summary>
    /// 开始指定玩家的回合。
    /// Player.StartTurn 会处理法力水晶、重置手牌费用、抽一张牌。
    /// GameManager 额外负责让该玩家场上的随从恢复攻击权限。
    /// </summary>
    public void StartTurn(Player targetPlayer)
    {
        if (targetPlayer == null) return;
        if (IsGameOver) return;

        CurrentPlayer = targetPlayer;
        TurnNumber++;

        targetPlayer.StartTurn();

        IReadOnlyList<Minion> minions = Board.GetMinions(targetPlayer);
        if (minions == null) return;

        foreach (Minion minion in minions)
        {
            minion.SetCanAttack(true);
        }
    }

    /// <summary>
    /// 结束当前玩家的回合，并切换到对手回合。
    /// </summary>
    public void EndTurn()
    {
        if (IsGameOver) return;

        Player nextPlayer = GetOpponent(CurrentPlayer);
        StartTurn(nextPlayer);
    }

    /// <summary>
    /// 根据一名玩家，返回他的对手。
    /// 如果传入的不是本局里的 Player 或 Enemy，则返回 null。
    /// </summary>
    public Player GetOpponent(Player targetPlayer)
    {
        if (targetPlayer == Player) return Enemy;
        if (targetPlayer == Enemy) return Player;

        return null;
    }

    /// <summary>
    /// 尝试把当前玩家手牌中的一张随从牌召唤到战场。
    /// 目前阶段所有 CardData 都先当作随从牌处理，法术和武器之后再扩展。
    /// </summary>
    public bool TryPlayMinionCard(Card card)
    {
        if (card == null) return false;
        if (IsGameOver) return false;
        if (CurrentPlayer == null) return false;
        if (!Board.CanSummon(CurrentPlayer)) return false;

        bool played = CurrentPlayer.PlayCard(card);
        if (!played) return false;

        Minion minion = new Minion(card.CardData, CurrentPlayer);
        return Board.SummonMinion(minion);
    }

    /// <summary>
    /// 尝试让一个随从攻击另一个随从。
    /// 双方会互相造成等于自身攻击力的伤害，然后清理死亡随从。
    /// </summary>
    public bool TryAttackMinion(Minion attacker, Minion target)
    {
        if (!CanAttack(attacker)) return false;
        if (target == null) return false;
        if (target.Owner == attacker.Owner) return false;

        attacker.TakeDamage(target.Attack);
        target.TakeDamage(attacker.Attack);
        attacker.SetCanAttack(false);

        CleanupDeadMinions();
        CheckGameOver();
        return true;
    }

    /// <summary>
    /// 尝试让一个随从攻击对方英雄。
    /// 不能攻击自己的英雄，也不能攻击不属于本局对手的英雄。
    /// </summary>
    public bool TryAttackHero(Minion attacker, Hero targetHero)
    {
        if (!CanAttack(attacker)) return false;
        if (targetHero == null) return false;

        Player opponent = GetOpponent(attacker.Owner);
        if (opponent == null) return false;
        if (targetHero != opponent.Hero) return false;

        targetHero.TakeDamage(attacker.Attack);
        attacker.SetCanAttack(false);

        CheckGameOver();
        return true;
    }

    /// <summary>
    /// 清理双方战场上生命值小于等于 0 的随从。
    /// </summary>
    public void CleanupDeadMinions()
    {
        RemoveDeadMinions(Player);
        RemoveDeadMinions(Enemy);
    }

    /// <summary>
    /// 检查双方英雄是否死亡。
    /// 如果双方同时死亡，Winner 保持 null，表示平局。
    /// </summary>
    public void CheckGameOver()
    {
        if (IsGameOver) return;

        bool playerDead = Player != null && Player.Hero.IsDead;
        bool enemyDead = Enemy != null && Enemy.Hero.IsDead;

        if (!playerDead && !enemyDead) return;

        IsGameOver = true;

        if (playerDead && enemyDead)
        {
            Winner = null;
        }
        else
        {
            Winner = playerDead ? Enemy : Player;
        }
    }

    /// <summary>
    /// 统一检查随从是否满足攻击条件。
    /// </summary>
    private bool CanAttack(Minion attacker)
    {
        if (attacker == null) return false;
        if (IsGameOver) return false;
        if (attacker.Owner != CurrentPlayer) return false;
        if (!attacker.CanAttack) return false;
        if (attacker.IsDead) return false;

        return true;
    }

    /// <summary>
    /// 从指定玩家的战场列表中倒序移除死亡随从。
    /// 倒序遍历可以避免删除元素时影响还没检查的索引。
    /// </summary>
    private void RemoveDeadMinions(Player owner)
    {
        if (owner == null) return;

        IReadOnlyList<Minion> minions = Board.GetMinions(owner);
        if (minions == null) return;

        for (int i = minions.Count - 1; i >= 0; i--)
        {
            Minion minion = minions[i];
            if (minion.IsDead)
            {
                Board.RemoveMinion(minion);
            }
        }
    }
}
