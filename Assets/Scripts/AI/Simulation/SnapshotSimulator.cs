using System.Collections.Generic;

/// <summary>
/// AI 快照模拟器。
/// 它对快照执行一个 SnapshotAction，并返回新的快照，不修改真实 GameManager，也不修改传入的旧快照。
/// </summary>
public static class SnapshotSimulator
{
    /// <summary>
    /// 对局面快照执行一个动作，返回动作后的新快照。
    /// 第一版只模拟当前 AI 评估需要的血量、手牌、法力和场面变化。
    /// </summary>
    public static GameStateSnapshot Simulate(GameStateSnapshot state, SnapshotAction action)
    {
        if (state == null) return null;

        PlayerSnapshot player = CopyPlayer(state.Player);
        PlayerSnapshot enemy = CopyPlayer(state.Enemy);
        List<MinionSnapshot> playerMinions = CopyMinions(state.Board?.PlayerMinions);
        List<MinionSnapshot> enemyMinions = CopyMinions(state.Board?.EnemyMinions);
        int currentPlayerIndex = state.CurrentPlayerIndex;

        if (action != null)
        {
            switch (action.ActionType)
            {
                case GameActionType.PlayMinionCard:
                    SimulatePlayMinion(action, ref player, ref enemy, playerMinions, enemyMinions);
                    break;
                case GameActionType.PlaySpellOnMinion:
                    SimulateSpellOnMinion(action, ref player, ref enemy, playerMinions, enemyMinions);
                    break;
                case GameActionType.PlaySpellOnHero:
                    SimulateSpellOnHero(action, ref player, ref enemy);
                    break;
                case GameActionType.AttackMinion:
                    SimulateAttackMinion(action, ref player, ref enemy, playerMinions, enemyMinions);
                    break;
                case GameActionType.AttackHero:
                    SimulateAttackHero(action, ref player, ref enemy, playerMinions, enemyMinions);
                    break;
                case GameActionType.EndTurn:
                    SimulateEndTurn(ref currentPlayerIndex, ref player, ref enemy, playerMinions, enemyMinions);
                    break;
            }
        }

        BoardSnapshot board = new BoardSnapshot(playerMinions, enemyMinions);
        return new GameStateSnapshot(currentPlayerIndex, player, enemy, board);
    }

    private static void SimulatePlayMinion(
        SnapshotAction action,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy,
        List<MinionSnapshot> playerMinions,
        List<MinionSnapshot> enemyMinions)
    {
        PlayerSnapshot actor = GetPlayer(action.ActorIndex, player, enemy);
        List<MinionSnapshot> actorMinions = GetMinions(action.ActorIndex, playerMinions, enemyMinions);

        actor = SpendCard(actor, action.CardCost, action.HandCardIndex);

        // 当前模拟关键词、无目标战吼和一层亡语伤害，更复杂的死亡连锁后续再补。
        actorMinions.Add(new MinionSnapshot(
            action.CardAttack,
            action.CardHealth,
            action.CardHealth,
            action.CardHasCharge,
            action.CardHasTaunt,
            action.CardHasDivineShield,
            action.CardHasCharge,
            action.DeathrattleType,
            action.DeathrattleValue));

        SetPlayer(action.ActorIndex, actor, ref player, ref enemy);
        ResolveBattlecry(action, ref player, ref enemy);
    }

    private static void ResolveBattlecry(
        SnapshotAction action,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy)
    {
        if (action == null) return;

        switch (action.BattlecryType)
        {
            case BattlecryType.DealDamageToEnemyHero:
                DamageOpponentHeroForBattlecry(action, ref player, ref enemy);
                break;
            case BattlecryType.DrawCard:
                DrawCardsForBattlecryOwner(action, ref player, ref enemy);
                break;
        }
    }

    private static void DamageOpponentHeroForBattlecry(
        SnapshotAction action,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy)
    {
        PlayerSnapshot opponent = GetPlayer(GetOpponentIndex(action.ActorIndex), player, enemy);
        opponent = DamageHero(opponent, action.BattlecryValue);
        SetPlayer(GetOpponentIndex(action.ActorIndex), opponent, ref player, ref enemy);
    }

    private static void DrawCardsForBattlecryOwner(
        SnapshotAction action,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy)
    {
        PlayerSnapshot actor = GetPlayer(action.ActorIndex, player, enemy);
        actor = DrawCards(actor, action.BattlecryValue);
        SetPlayer(action.ActorIndex, actor, ref player, ref enemy);
    }

    private static void SimulateSpellOnMinion(
        SnapshotAction action,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy,
        List<MinionSnapshot> playerMinions,
        List<MinionSnapshot> enemyMinions)
    {
        List<MinionSnapshot> targetMinions = GetMinions(GetOpponentIndex(action.ActorIndex), playerMinions, enemyMinions);
        if (!IsValidIndex(targetMinions, action.TargetMinionIndex)) return;

        PlayerSnapshot actor = GetPlayer(action.ActorIndex, player, enemy);
        actor = SpendCard(actor, action.CardCost, action.HandCardIndex);
        SetPlayer(action.ActorIndex, actor, ref player, ref enemy);

        targetMinions[action.TargetMinionIndex] = DamageMinion(targetMinions[action.TargetMinionIndex], action.SpellDamage);
        ResolveDeathrattlesAndRemoveDeadMinions(
            targetMinions,
            GetOpponentIndex(action.ActorIndex),
            ref player,
            ref enemy);
    }

    private static void SimulateSpellOnHero(
        SnapshotAction action,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy)
    {
        PlayerSnapshot actor = GetPlayer(action.ActorIndex, player, enemy);
        PlayerSnapshot opponent = GetPlayer(GetOpponentIndex(action.ActorIndex), player, enemy);

        actor = SpendCard(actor, action.CardCost, action.HandCardIndex);
        opponent = DamageHero(opponent, action.SpellDamage);

        SetPlayer(action.ActorIndex, actor, ref player, ref enemy);
        SetPlayer(GetOpponentIndex(action.ActorIndex), opponent, ref player, ref enemy);
    }

    private static void SimulateEndTurn(
        ref int currentPlayerIndex,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy,
        List<MinionSnapshot> playerMinions,
        List<MinionSnapshot> enemyMinions)
    {
        currentPlayerIndex = GetOpponentIndex(currentPlayerIndex);

        PlayerSnapshot nextPlayer = GetPlayer(currentPlayerIndex, player, enemy);
        nextPlayer = StartTurnPlayer(nextPlayer);
        SetPlayer(currentPlayerIndex, nextPlayer, ref player, ref enemy);

        List<MinionSnapshot> nextPlayerMinions = GetMinions(currentPlayerIndex, playerMinions, enemyMinions);
        SetMinionsCanAttack(nextPlayerMinions, true);
    }

    private static PlayerSnapshot StartTurnPlayer(PlayerSnapshot player)
    {
        if (player == null) return new PlayerSnapshot(0, 0, 0, 0, 0, 0);

        int maxMana = player.MaxMana < Player.MaxManaCap
            ? player.MaxMana + 1
            : player.MaxMana;

        PlayerSnapshot refreshedPlayer = new PlayerSnapshot(
            player.HeroHealth,
            player.HeroMaxHealth,
            maxMana,
            maxMana,
            player.HandCount,
            player.DeckCount,
            player.HandCards);

        return DrawCardForTurnStart(refreshedPlayer);
    }

    private static PlayerSnapshot DrawCardForTurnStart(PlayerSnapshot player)
    {
        if (player == null) return new PlayerSnapshot(0, 0, 0, 0, 0, 0);
        if (player.DeckCount <= 0) return player;

        int handCount = player.HandCount;
        if (handCount < Player.MaxHandSize)
        {
            handCount++;
        }

        return new PlayerSnapshot(
            player.HeroHealth,
            player.HeroMaxHealth,
            player.CurrentMana,
            player.MaxMana,
            handCount,
            player.DeckCount - 1,
            player.HandCards);
    }

    private static PlayerSnapshot DrawCards(PlayerSnapshot player, int drawCount)
    {
        PlayerSnapshot result = player;

        for (int i = 0; i < drawCount; i++)
        {
            result = DrawOneCard(result);
        }

        return result;
    }

    private static PlayerSnapshot DrawOneCard(PlayerSnapshot player)
    {
        if (player == null) return new PlayerSnapshot(0, 0, 0, 0, 0, 0);
        if (player.DeckCount <= 0) return player;

        int handCount = player.HandCount;
        if (handCount < Player.MaxHandSize)
        {
            handCount++;
        }

        return new PlayerSnapshot(
            player.HeroHealth,
            player.HeroMaxHealth,
            player.CurrentMana,
            player.MaxMana,
            handCount,
            player.DeckCount - 1,
            player.HandCards);
    }

    private static void SimulateAttackMinion(
        SnapshotAction action,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy,
        List<MinionSnapshot> playerMinions,
        List<MinionSnapshot> enemyMinions)
    {
        List<MinionSnapshot> actorMinions = GetMinions(action.ActorIndex, playerMinions, enemyMinions);
        List<MinionSnapshot> opponentMinions = GetMinions(GetOpponentIndex(action.ActorIndex), playerMinions, enemyMinions);

        if (!IsValidIndex(actorMinions, action.AttackerIndex)) return;
        if (!IsValidIndex(opponentMinions, action.TargetMinionIndex)) return;

        MinionSnapshot attacker = actorMinions[action.AttackerIndex];
        MinionSnapshot target = opponentMinions[action.TargetMinionIndex];

        MinionSnapshot damagedAttacker = DamageMinion(attacker, target.Attack);
        damagedAttacker = SetCanAttack(damagedAttacker, false);
        MinionSnapshot damagedTarget = DamageMinion(target, attacker.Attack);

        actorMinions[action.AttackerIndex] = damagedAttacker;
        opponentMinions[action.TargetMinionIndex] = damagedTarget;

        ResolveDeathrattlesAndRemoveDeadMinions(
            actorMinions,
            action.ActorIndex,
            ref player,
            ref enemy);
        ResolveDeathrattlesAndRemoveDeadMinions(
            opponentMinions,
            GetOpponentIndex(action.ActorIndex),
            ref player,
            ref enemy);
    }

    private static void SimulateAttackHero(
        SnapshotAction action,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy,
        List<MinionSnapshot> playerMinions,
        List<MinionSnapshot> enemyMinions)
    {
        List<MinionSnapshot> actorMinions = GetMinions(action.ActorIndex, playerMinions, enemyMinions);
        if (!IsValidIndex(actorMinions, action.AttackerIndex)) return;

        MinionSnapshot attacker = actorMinions[action.AttackerIndex];
        PlayerSnapshot opponent = GetPlayer(GetOpponentIndex(action.ActorIndex), player, enemy);

        opponent = DamageHero(opponent, attacker.Attack);
        actorMinions[action.AttackerIndex] = SetCanAttack(attacker, false);

        SetPlayer(GetOpponentIndex(action.ActorIndex), opponent, ref player, ref enemy);
    }

    private static PlayerSnapshot SpendCard(PlayerSnapshot player, int cost, int handCardIndex)
    {
        if (player == null) return new PlayerSnapshot(0, 0, 0, 0, 0, 0);

        IReadOnlyList<CardSnapshot> remainingHandCards = RemoveKnownHandCard(player, handCardIndex);
        return new PlayerSnapshot(
            player.HeroHealth,
            player.HeroMaxHealth,
            player.CurrentMana - cost,
            player.MaxMana,
            player.HandCount - 1,
            player.DeckCount,
            remainingHandCards);
    }

    private static IReadOnlyList<CardSnapshot> RemoveKnownHandCard(PlayerSnapshot player, int handCardIndex)
    {
        if (player == null) return null;
        if (player.HandCards == null) return null;

        if (handCardIndex < 0 || handCardIndex >= player.HandCards.Count)
        {
            // 没有具体手牌索引时，不能可靠判断剩余手牌是哪几张。
            // 只保留 HandCount，丢弃具体列表，避免后续搜索误用已经打出的牌。
            return null;
        }

        List<CardSnapshot> remainingCards = new List<CardSnapshot>();
        for (int i = 0; i < player.HandCards.Count; i++)
        {
            if (i == handCardIndex) continue;
            remainingCards.Add(player.HandCards[i]);
        }

        return remainingCards;
    }

    private static PlayerSnapshot DamageHero(PlayerSnapshot player, int damage)
    {
        if (player == null) return new PlayerSnapshot(0, 0, 0, 0, 0, 0);

        return new PlayerSnapshot(
            player.HeroHealth - damage,
            player.HeroMaxHealth,
            player.CurrentMana,
            player.MaxMana,
            player.HandCount,
            player.DeckCount,
            player.HandCards);
    }

    private static MinionSnapshot DamageMinion(MinionSnapshot minion, int damage)
    {
        if (minion == null) return null;
        if (damage <= 0) return minion;

        if (minion.HasDivineShield)
        {
            return new MinionSnapshot(
                minion.Attack,
                minion.CurrentHealth,
                minion.MaxHealth,
                minion.CanAttack,
                minion.HasTaunt,
                false,
                minion.HasCharge,
                minion.DeathrattleType,
                minion.DeathrattleValue);
        }

        return new MinionSnapshot(
            minion.Attack,
            minion.CurrentHealth - damage,
            minion.MaxHealth,
            minion.CanAttack,
            minion.HasTaunt,
            minion.HasDivineShield,
            minion.HasCharge,
            minion.DeathrattleType,
            minion.DeathrattleValue);
    }

    private static MinionSnapshot SetCanAttack(MinionSnapshot minion, bool canAttack)
    {
        if (minion == null) return null;

        return new MinionSnapshot(
            minion.Attack,
            minion.CurrentHealth,
            minion.MaxHealth,
            canAttack,
            minion.HasTaunt,
            minion.HasDivineShield,
            minion.HasCharge,
            minion.DeathrattleType,
            minion.DeathrattleValue);
    }

    private static void SetMinionsCanAttack(List<MinionSnapshot> minions, bool canAttack)
    {
        if (minions == null) return;

        for (int i = 0; i < minions.Count; i++)
        {
            minions[i] = SetCanAttack(minions[i], canAttack);
        }
    }

    private static PlayerSnapshot CopyPlayer(PlayerSnapshot player)
    {
        if (player == null) return new PlayerSnapshot(0, 0, 0, 0, 0, 0);

        return new PlayerSnapshot(
            player.HeroHealth,
            player.HeroMaxHealth,
            player.CurrentMana,
            player.MaxMana,
            player.HandCount,
            player.DeckCount,
            player.HandCards);
    }

    private static List<MinionSnapshot> CopyMinions(IReadOnlyList<MinionSnapshot> minions)
    {
        List<MinionSnapshot> copy = new List<MinionSnapshot>();
        if (minions == null) return copy;

        for (int i = 0; i < minions.Count; i++)
        {
            MinionSnapshot minion = minions[i];
            if (minion == null) continue;

            copy.Add(new MinionSnapshot(
                minion.Attack,
                minion.CurrentHealth,
                minion.MaxHealth,
                minion.CanAttack,
                minion.HasTaunt,
                minion.HasDivineShield,
                minion.HasCharge,
                minion.DeathrattleType,
                minion.DeathrattleValue));
        }

        return copy;
    }

    private static PlayerSnapshot GetPlayer(int playerIndex, PlayerSnapshot player, PlayerSnapshot enemy)
    {
        return NormalizePlayerIndex(playerIndex) == GameStateSnapshot.EnemyIndex ? enemy : player;
    }

    private static void SetPlayer(int playerIndex, PlayerSnapshot value, ref PlayerSnapshot player, ref PlayerSnapshot enemy)
    {
        if (NormalizePlayerIndex(playerIndex) == GameStateSnapshot.EnemyIndex)
        {
            enemy = value;
            return;
        }

        player = value;
    }

    private static List<MinionSnapshot> GetMinions(
        int playerIndex,
        List<MinionSnapshot> playerMinions,
        List<MinionSnapshot> enemyMinions)
    {
        return NormalizePlayerIndex(playerIndex) == GameStateSnapshot.EnemyIndex
            ? enemyMinions
            : playerMinions;
    }

    private static void ResolveDeathrattlesAndRemoveDeadMinions(
        List<MinionSnapshot> minions,
        int ownerIndex,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy)
    {
        if (minions == null) return;

        for (int i = minions.Count - 1; i >= 0; i--)
        {
            if (minions[i] == null || minions[i].IsDead)
            {
                ResolveDeathrattle(minions[i], ownerIndex, ref player, ref enemy);
                minions.RemoveAt(i);
            }
        }
    }

    private static void ResolveDeathrattle(
        MinionSnapshot minion,
        int ownerIndex,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy)
    {
        if (minion == null) return;

        switch (minion.DeathrattleType)
        {
            case DeathrattleType.DealDamageToEnemyHero:
                DamageOpponentHeroForDeathrattle(minion, ownerIndex, ref player, ref enemy);
                break;
        }
    }

    private static void DamageOpponentHeroForDeathrattle(
        MinionSnapshot minion,
        int ownerIndex,
        ref PlayerSnapshot player,
        ref PlayerSnapshot enemy)
    {
        PlayerSnapshot opponent = GetPlayer(GetOpponentIndex(ownerIndex), player, enemy);
        opponent = DamageHero(opponent, minion.DeathrattleValue);
        SetPlayer(GetOpponentIndex(ownerIndex), opponent, ref player, ref enemy);
    }

    private static bool IsValidIndex(IReadOnlyList<MinionSnapshot> minions, int index)
    {
        return minions != null && index >= 0 && index < minions.Count;
    }

    private static int GetOpponentIndex(int playerIndex)
    {
        return NormalizePlayerIndex(playerIndex) == GameStateSnapshot.EnemyIndex
            ? GameStateSnapshot.PlayerIndex
            : GameStateSnapshot.EnemyIndex;
    }

    private static int NormalizePlayerIndex(int playerIndex)
    {
        return playerIndex == GameStateSnapshot.EnemyIndex
            ? GameStateSnapshot.EnemyIndex
            : GameStateSnapshot.PlayerIndex;
    }
}
