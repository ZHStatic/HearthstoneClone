/// <summary>
/// GameManager 的战斗日志相关方法。
/// 拆到单独文件只是为了降低主流程文件长度，不改变 GameManager 的职责边界。
/// </summary>
public partial class GameManager
{
    /// <summary>
    /// 记录一条战斗日志。
    /// </summary>
    private BattleLogEntry RecordBattleLog(
        BattleLogEntryType entryType,
        Player sourcePlayer = null,
        Player targetPlayer = null,
        string sourceName = "",
        string targetName = "",
        int attemptedAmount = 0,
        int actualAmount = 0,
        string message = "",
        bool setAsLastAction = false)
    {
        if (BattleLogger == null) return null;

        BattleLogEntry entry = BattleLogger.Add(
            entryType,
            TurnNumber,
            GetPlayerLogName(sourcePlayer),
            GetPlayerLogName(targetPlayer),
            sourceName,
            targetName,
            attemptedAmount,
            actualAmount,
            message);

        if (setAsLastAction)
        {
            LastActionLogEntry = entry;
        }

        return entry;
    }

    /// <summary>
    /// 记录出牌日志。
    /// </summary>
    private void RecordCardPlayed(Card card, Player sourcePlayer)
    {
        if (card == null) return;

        string cardName = GetCardLogName(card);
        RecordBattleLog(
            BattleLogEntryType.CardPlayed,
            sourcePlayer: sourcePlayer,
            sourceName: cardName,
            message: $"{GetPlayerLogName(sourcePlayer)} 打出 {cardName}。");
    }

    /// <summary>
    /// 记录召唤日志。
    /// </summary>
    private void RecordMinionSummoned(Minion minion)
    {
        if (minion == null) return;

        string minionName = GetMinionLogName(minion);
        RecordBattleLog(
            BattleLogEntryType.MinionSummoned,
            sourcePlayer: minion.Owner,
            targetPlayer: minion.Owner,
            sourceName: minionName,
            targetName: minionName,
            message: $"{GetPlayerLogName(minion.Owner)} 召唤 {minionName}。");
    }

    /// <summary>
    /// 记录攻击行为日志。具体伤害由 DamageMinion / DamageHero 记录。
    /// </summary>
    private void RecordAttack(Minion attacker, string targetName)
    {
        if (attacker == null) return;

        string attackerName = GetMinionLogName(attacker);
        RecordBattleLog(
            BattleLogEntryType.Attack,
            sourcePlayer: attacker.Owner,
            sourceName: attackerName,
            targetName: targetName,
            message: $"{attackerName} 攻击 {targetName}。");
    }

    /// <summary>
    /// 记录随从死亡日志。
    /// </summary>
    private void RecordMinionDied(Minion minion)
    {
        if (minion == null) return;

        string minionName = GetMinionLogName(minion);
        RecordBattleLog(
            BattleLogEntryType.MinionDied,
            targetPlayer: minion.Owner,
            targetName: minionName,
            message: $"{minionName} 死亡。");
    }

    /// <summary>
    /// 对随从造成伤害，并记录尝试伤害、实际伤害和圣盾抵消。
    /// </summary>
    private BattleLogEntry DamageMinion(
        Minion target,
        int attemptedAmount,
        string sourceName,
        Player sourcePlayer,
        BattleLogEntryType entryType)
    {
        if (target == null) return null;
        if (attemptedAmount <= 0) return null;

        string targetName = GetMinionLogName(target);
        bool hadDivineShield = target.HasDivineShield;
        int actualAmount = target.TakeDamage(attemptedAmount);

        if (hadDivineShield && actualAmount == 0)
        {
            return RecordBattleLog(
                BattleLogEntryType.DivineShieldPrevented,
                sourcePlayer: sourcePlayer,
                targetPlayer: target.Owner,
                sourceName: sourceName,
                targetName: targetName,
                attemptedAmount: attemptedAmount,
                actualAmount: actualAmount,
                message: $"{targetName} 的圣盾抵消了 {sourceName} 的 {attemptedAmount} 点伤害。",
                setAsLastAction: true);
        }

        return RecordBattleLog(
            entryType,
            sourcePlayer: sourcePlayer,
            targetPlayer: target.Owner,
            sourceName: sourceName,
            targetName: targetName,
            attemptedAmount: attemptedAmount,
            actualAmount: actualAmount,
            message: $"{sourceName} 对 {targetName} 造成 {actualAmount} 点伤害。",
            setAsLastAction: true);
    }

    /// <summary>
    /// 对英雄造成伤害，并记录尝试伤害和实际伤害。
    /// </summary>
    private BattleLogEntry DamageHero(
        Hero targetHero,
        int attemptedAmount,
        string sourceName,
        Player sourcePlayer,
        BattleLogEntryType entryType)
    {
        if (targetHero == null) return null;
        if (attemptedAmount <= 0) return null;

        int actualAmount = targetHero.TakeDamage(attemptedAmount);
        string targetName = GetHeroLogName(targetHero);
        Player targetPlayer = GetHeroOwner(targetHero);

        return RecordBattleLog(
            entryType,
            sourcePlayer: sourcePlayer,
            targetPlayer: targetPlayer,
            sourceName: sourceName,
            targetName: targetName,
            attemptedAmount: attemptedAmount,
            actualAmount: actualAmount,
            message: $"{sourceName} 对 {targetName} 造成 {actualAmount} 点伤害。",
            setAsLastAction: true);
    }

    /// <summary>
    /// 获取日志中使用的玩家名称。
    /// </summary>
    private string GetPlayerLogName(Player player)
    {
        if (player == null) return "";
        if (player == Player) return "Player";
        if (player == Enemy) return "Enemy";
        if (player.Hero != null) return player.Hero.Name;

        return "未知玩家";
    }

    /// <summary>
    /// 获取日志中使用的卡牌名称。
    /// </summary>
    private string GetCardLogName(Card card)
    {
        if (card == null || card.CardData == null) return "未知卡牌";

        return card.CardData.CardName;
    }

    /// <summary>
    /// 获取日志中使用的随从名称。
    /// </summary>
    private string GetMinionLogName(Minion minion)
    {
        if (minion == null || minion.CardData == null) return "未知随从";

        return minion.CardData.CardName;
    }

    /// <summary>
    /// 获取日志中使用的英雄名称。
    /// </summary>
    private string GetHeroLogName(Hero hero)
    {
        if (hero == null) return "未知英雄";

        return hero.Name;
    }

    /// <summary>
    /// 根据英雄对象找到所属玩家。
    /// </summary>
    private Player GetHeroOwner(Hero hero)
    {
        if (hero == null) return null;
        if (Player != null && hero == Player.Hero) return Player;
        if (Enemy != null && hero == Enemy.Hero) return Enemy;

        return null;
    }
}
