using System.Collections.Generic;

/// <summary>
/// AI 局面评估函数。
/// 它只读取当前局面并返回分数，不生成动作、不执行动作、不修改游戏状态。
/// </summary>
public class Evaluator
{
    // 英雄血量差的权重。数值越高，AI 越重视保命和压低敌方英雄血量。
    private const int HeroHealthWeight = 2;

    // 手牌数量差的权重。数值越高，AI 越重视资源数量。
    private const int HandCardWeight = 4;

    // 随从攻击力的权重。数值越高，AI 越重视场面进攻能力。
    private const int MinionAttackWeight = 5;

    // 随从当前血量的权重。数值越高，AI 越重视场面存活能力。
    private const int MinionHealthWeight = 3;

    // 嘲讽的额外分。嘲讽能保护英雄和其他随从，所以给少量加分。
    private const int TauntBonus = 3;

    // 圣盾的额外分。圣盾通常能抵消一次伤害，所以价值高于普通关键词。
    private const int DivineShieldBonus = 5;

    // 冲锋的额外分。当前只表示这个随从更灵活，不直接预测后续攻击收益。
    private const int ChargeBonus = 2;

    /// <summary>
    /// 从 player 视角评估当前局面。
    /// 正分表示 player 更有优势，负分表示 opponent 更有优势。
    /// </summary>
    public int Evaluate(Player player, Player opponent, Board board)
    {
        return EvaluateDetailed(player, opponent, board).TotalScore;
    }

    /// <summary>
    /// 从指定玩家索引视角评估一份局面快照。
    /// 正分表示该玩家更有优势，负分表示对手更有优势。
    /// </summary>
    public int Evaluate(GameStateSnapshot state, int playerIndex)
    {
        return EvaluateDetailed(state, playerIndex).TotalScore;
    }

    /// <summary>
    /// 从 player 视角评估当前局面，并返回评分明细。
    /// 正分表示 player 更有优势，负分表示 opponent 更有优势。
    /// </summary>
    public EvaluationResult EvaluateDetailed(Player player, Player opponent, Board board)
    {
        if (player == null) return new EvaluationResult(0, 0, 0);

        int heroHealthScore = EvaluateHeroHealth(player, opponent);
        int handScore = EvaluateHandCount(player, opponent);
        int boardScore = EvaluateBoard(player, opponent, board);

        return new EvaluationResult(heroHealthScore, handScore, boardScore);
    }

    /// <summary>
    /// 从指定玩家索引视角评估一份局面快照，并返回评分明细。
    /// 这里使用和真实局面评分相同的权重，方便对比真实评分和快照评分是否一致。
    /// </summary>
    public EvaluationResult EvaluateDetailed(GameStateSnapshot state, int playerIndex)
    {
        if (state == null) return new EvaluationResult(0, 0, 0);

        int normalizedPlayerIndex = NormalizePlayerIndex(playerIndex);
        PlayerSnapshot player = GetPlayerSnapshot(state, normalizedPlayerIndex);
        PlayerSnapshot opponent = GetOpponentSnapshot(state, normalizedPlayerIndex);

        int heroHealthScore = EvaluateHeroHealth(player, opponent);
        int handScore = EvaluateHandCount(player, opponent);
        int boardScore = EvaluateBoard(state, normalizedPlayerIndex);

        return new EvaluationResult(heroHealthScore, handScore, boardScore);
    }

    /// <summary>
    /// 评估英雄血量差。
    /// 血量越高越安全；敌方血量越低，进攻压力越大。
    /// </summary>
    private int EvaluateHeroHealth(Player player, Player opponent)
    {
        int playerHealth = GetHeroHealth(player);
        int opponentHealth = GetHeroHealth(opponent);

        return (playerHealth - opponentHealth) * HeroHealthWeight;
    }

    /// <summary>
    /// 评估快照中的英雄血量差。
    /// </summary>
    private int EvaluateHeroHealth(PlayerSnapshot player, PlayerSnapshot opponent)
    {
        int playerHealth = player != null ? player.HeroHealth : 0;
        int opponentHealth = opponent != null ? opponent.HeroHealth : 0;

        return (playerHealth - opponentHealth) * HeroHealthWeight;
    }

    /// <summary>
    /// 评估手牌数量差。
    /// 当前阶段先只看数量，不区分手牌质量。
    /// </summary>
    private int EvaluateHandCount(Player player, Player opponent)
    {
        int playerHandCount = player != null && player.Hand != null ? player.Hand.Count : 0;
        int opponentHandCount = opponent != null && opponent.Hand != null ? opponent.Hand.Count : 0;

        return (playerHandCount - opponentHandCount) * HandCardWeight;
    }

    /// <summary>
    /// 评估快照中的手牌数量差。
    /// </summary>
    private int EvaluateHandCount(PlayerSnapshot player, PlayerSnapshot opponent)
    {
        int playerHandCount = player != null ? player.HandCount : 0;
        int opponentHandCount = opponent != null ? opponent.HandCount : 0;

        return (playerHandCount - opponentHandCount) * HandCardWeight;
    }

    /// <summary>
    /// 评估双方战场随从价值差。
    /// 当前只计算已经在场上的存活随从，不预测亡语、战吼或下回合抽牌。
    /// </summary>
    private int EvaluateBoard(Player player, Player opponent, Board board)
    {
        if (board == null) return 0;

        int playerBoardScore = EvaluatePlayerBoard(player, board);
        int opponentBoardScore = EvaluatePlayerBoard(opponent, board);

        return playerBoardScore - opponentBoardScore;
    }

    /// <summary>
    /// 评估快照中的双方场面随从价值差。
    /// </summary>
    private int EvaluateBoard(GameStateSnapshot state, int playerIndex)
    {
        if (state == null || state.Board == null) return 0;

        int normalizedPlayerIndex = NormalizePlayerIndex(playerIndex);
        int opponentIndex = normalizedPlayerIndex == GameStateSnapshot.EnemyIndex
            ? GameStateSnapshot.PlayerIndex
            : GameStateSnapshot.EnemyIndex;

        int playerBoardScore = EvaluatePlayerBoard(state.Board.GetMinions(normalizedPlayerIndex));
        int opponentBoardScore = EvaluatePlayerBoard(state.Board.GetMinions(opponentIndex));

        return playerBoardScore - opponentBoardScore;
    }

    /// <summary>
    /// 评估一名玩家当前场上的所有随从。
    /// </summary>
    private int EvaluatePlayerBoard(Player player, Board board)
    {
        if (player == null || board == null) return 0;

        int score = 0;
        var minions = board.GetMinions(player);
        if (minions == null) return score;

        for (int i = 0; i < minions.Count; i++)
        {
            score += EvaluateMinion(minions[i]);
        }

        return score;
    }

    /// <summary>
    /// 评估快照中的一组随从。
    /// </summary>
    private int EvaluatePlayerBoard(IReadOnlyList<MinionSnapshot> minions)
    {
        int score = 0;
        if (minions == null) return score;

        for (int i = 0; i < minions.Count; i++)
        {
            score += EvaluateMinion(minions[i]);
        }

        return score;
    }

    /// <summary>
    /// 评估单个随从的当前价值。
    /// 攻击和血量是基础价值，关键词提供少量额外价值。
    /// </summary>
    private int EvaluateMinion(Minion minion)
    {
        if (minion == null || minion.IsDead) return 0;

        int score = 0;
        score += minion.Attack * MinionAttackWeight;
        score += minion.CurrentHealth * MinionHealthWeight;

        if (minion.HasKeyword(KeywordType.Taunt))
        {
            score += TauntBonus;
        }

        if (minion.HasKeyword(KeywordType.DivineShield))
        {
            score += DivineShieldBonus;
        }

        if (minion.HasKeyword(KeywordType.Charge))
        {
            score += ChargeBonus;
        }

        return score;
    }

    /// <summary>
    /// 评估快照中的单个随从价值。
    /// </summary>
    private int EvaluateMinion(MinionSnapshot minion)
    {
        if (minion == null || minion.IsDead) return 0;

        int score = 0;
        score += minion.Attack * MinionAttackWeight;
        score += minion.CurrentHealth * MinionHealthWeight;

        if (minion.HasTaunt)
        {
            score += TauntBonus;
        }

        if (minion.HasDivineShield)
        {
            score += DivineShieldBonus;
        }

        if (minion.HasCharge)
        {
            score += ChargeBonus;
        }

        return score;
    }

    private int GetHeroHealth(Player player)
    {
        if (player == null || player.Hero == null) return 0;

        return player.Hero.CurrentHealth > 0 ? player.Hero.CurrentHealth : 0;
    }

    private PlayerSnapshot GetPlayerSnapshot(GameStateSnapshot state, int playerIndex)
    {
        if (state == null) return null;

        return NormalizePlayerIndex(playerIndex) == GameStateSnapshot.EnemyIndex
            ? state.Enemy
            : state.Player;
    }

    private PlayerSnapshot GetOpponentSnapshot(GameStateSnapshot state, int playerIndex)
    {
        if (state == null) return null;

        return NormalizePlayerIndex(playerIndex) == GameStateSnapshot.EnemyIndex
            ? state.Player
            : state.Enemy;
    }

    private int NormalizePlayerIndex(int playerIndex)
    {
        return playerIndex == GameStateSnapshot.EnemyIndex
            ? GameStateSnapshot.EnemyIndex
            : GameStateSnapshot.PlayerIndex;
    }
}
