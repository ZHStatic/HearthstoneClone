/// <summary>
/// 游戏操作失败原因。
/// 用来让 Core 层明确说明一次操作为什么失败，避免 UI 只拿到 false 后自己猜。
/// </summary>
public enum GameActionFailureReason
{
    /// <summary>
    /// 没有失败原因。通常表示操作成功，或结果还没有设置失败原因。
    /// </summary>
    None,

    /// <summary>
    /// 游戏已经结束，不能继续执行会改变局面的操作。
    /// </summary>
    GameOver,

    /// <summary>
    /// 卡牌为空，或卡牌模板数据无效。
    /// </summary>
    InvalidCard,

    /// <summary>
    /// 卡牌类型不符合当前操作。
    /// 例如把法术牌当成随从牌打出。
    /// </summary>
    WrongCardType,

    /// <summary>
    /// 当前没有行动玩家。
    /// </summary>
    NoCurrentPlayer,

    /// <summary>
    /// 这张卡不在当前玩家手牌中。
    /// </summary>
    CardNotInHand,

    /// <summary>
    /// 当前玩家法力不足。
    /// </summary>
    NotEnoughMana,

    /// <summary>
    /// 战场对象不存在或尚未初始化。
    /// </summary>
    BoardUnavailable,

    /// <summary>
    /// 当前玩家的战场已满，不能召唤更多随从。
    /// </summary>
    BoardFull,

    /// <summary>
    /// 攻击者为空或不是有效随从。
    /// </summary>
    InvalidAttacker,

    /// <summary>
    /// 攻击者不是当前玩家控制的随从。
    /// </summary>
    NotCurrentPlayerMinion,

    /// <summary>
    /// 随从当前不能攻击。
    /// </summary>
    MinionCannotAttack,

    /// <summary>
    /// 随从已经死亡。
    /// </summary>
    MinionDead,

    /// <summary>
    /// 目标为空、死亡、不属于本局，或不能成为当前操作的目标。
    /// </summary>
    InvalidTarget,

    /// <summary>
    /// 目标阵营不合法，例如攻击或敌方法术选中了友方目标。
    /// </summary>
    TargetIsFriendly,

    /// <summary>
    /// 防守方有嘲讽随从，当前目标被嘲讽规则阻挡。
    /// </summary>
    TauntBlocksTarget,

    /// <summary>
    /// 兜底失败原因。只有暂时没有更精确枚举时才使用。
    /// </summary>
    Unknown
}
