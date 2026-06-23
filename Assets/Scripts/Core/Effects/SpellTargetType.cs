/// <summary>
/// 法术目标类型。
/// 阶段 2.1 先用于描述单目标法术可以选择哪些对象。
/// </summary>
public enum SpellTargetType
{
    None,
    AnyCharacter,
    EnemyCharacter,
    FriendlyCharacter,
    Minion,
    EnemyMinion,
    FriendlyMinion
}
