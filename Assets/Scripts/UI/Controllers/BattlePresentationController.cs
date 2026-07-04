using System.Collections;
using UnityEngine;

/// <summary>
/// 战斗表现控制器。
/// 只根据 Core 已经记录好的 BattleLogEntry 播放基础音效和 UI 脉冲反馈，不参与规则结算。
/// 当前是阶段 4.5 的第一版表现入口：先验证“规则先结算，表现后播放”的边界。
/// 后续如果要做出牌飞行动画、攻击轨迹、受击闪烁，可以继续从这里扩展表现队列。
/// </summary>
public class BattlePresentationController : MonoBehaviour
{
    // 音效配置区。
    // 这些 AudioClip 都在 Unity Editor 里绑定；代码只决定“哪类日志播放哪个音效”。
    [Header("Audio")]
    // 用来播放一次性音效的 AudioSource。
    // 如果 Inspector 没有手动绑定，Awake 会尝试从当前物体上获取。
    [SerializeField] private AudioSource audioSource;
    // 出牌或召唤随从时播放的音效。
    [SerializeField] private AudioClip cardPlayedClip;
    // 记录攻击行为时播放的音效。
    [SerializeField] private AudioClip attackClip;
    // 法术、英雄技能、战吼、亡语或普通伤害结算时播放的音效。
    [SerializeField] private AudioClip damageClip;
    // 随从死亡时播放的音效。
    [SerializeField] private AudioClip deathClip;
    // 圣盾抵消伤害时播放的音效。
    [SerializeField] private AudioClip divineShieldClip;
    // 回合开始或结束时播放的音效。
    [SerializeField] private AudioClip turnStartedClip;
    // 游戏结束时播放的音效。
    [SerializeField] private AudioClip gameEndedClip;

    // 缩放反馈配置区。
    // 第一版只做一个通用 UI 脉冲，不直接移动卡牌或随从。
    [Header("Pulse")]
    // 默认播放缩放反馈的 UI 目标。
    // 可以先绑定 FeedbackText 或反馈区域，让玩家看到“刚刚发生了结算”。
    [SerializeField] private Transform defaultPulseTarget;
    // 缩放到原始大小的多少倍。1.08 表示放大到 108%。
    [SerializeField] private float smallPulseScale = 1.08f;
    // 放大或缩回各自花费的时间，单位是秒。
    [SerializeField] private float shortDuration = 0.12f;

    // 当前正在播放的缩放协程。
    // 用它可以在新反馈到来时停止旧动画，避免多个缩放动画同时改同一个 Transform。
    private Coroutine pulseCoroutine;
    // 当前正在被缩放的 UI 目标。
    // 记录它是为了中断动画时能把缩放还原。
    private Transform pulsingTarget;
    // 当前 UI 目标开始缩放前的原始大小。
    // 动画结束或被打断时会恢复到这个值。
    private Vector3 pulsingOriginalScale;

    /// <summary>
    /// Unity 生命周期方法：对象创建时尝试自动获取 AudioSource。
    /// 这样场景里只要把 AudioSource 挂在同一个物体上，就不一定要手动拖字段。
    /// </summary>
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 根据一条战斗日志播放基础表现。
    /// 日志来自 Core 结算结果；这里不判断伤害、死亡或胜负规则。
    /// 例如 Core 记录 DivineShieldPrevented，这里就只负责播放圣盾音效和通用脉冲。
    /// </summary>
    public void PlayLogFeedback(BattleLogEntry entry)
    {
        if (entry == null) return;

        PlayAudio(GetClipForEntry(entry));
        PlayButtonPulse(defaultPulseTarget);
    }

    /// <summary>
    /// 对一个 UI 目标做轻微缩放反馈。
    /// 没有目标时直接跳过，方便第一版只绑定音效。
    /// 如果上一次脉冲还没结束，会先停掉旧协程并恢复旧目标缩放，再播放新的脉冲。
    /// </summary>
    public void PlayButtonPulse(Transform target)
    {
        if (target == null) return;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            ResetPulsingTargetScale();
        }

        pulseCoroutine = StartCoroutine(PlayPulseRoutine(target));
    }

    /// <summary>
    /// 安全播放一次性音效。
    /// 没有绑定 AudioSource 或 AudioClip 时不报错，方便逐步在 Editor 里补资源。
    /// PlayOneShot 不会替换 AudioSource 当前正在播放的 clip，适合短促反馈音。
    /// </summary>
    private void PlayAudio(AudioClip clip)
    {
        if (audioSource == null) return;
        if (clip == null) return;

        audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 把战斗日志类型映射成对应音效。
    /// 这里不改变日志，也不执行任何游戏规则，只做表现层选择。
    /// 当前多个伤害来源共用 damageClip，这是第一版阶段性简化。
    /// </summary>
    private AudioClip GetClipForEntry(BattleLogEntry entry)
    {
        if (entry == null) return null;

        switch (entry.EntryType)
        {
            case BattleLogEntryType.TurnStarted:
            case BattleLogEntryType.TurnEnded:
                return turnStartedClip;
            case BattleLogEntryType.CardPlayed:
            case BattleLogEntryType.MinionSummoned:
                return cardPlayedClip;
            case BattleLogEntryType.Attack:
                return attackClip;
            case BattleLogEntryType.Spell:
            case BattleLogEntryType.HeroSkill:
            case BattleLogEntryType.Battlecry:
            case BattleLogEntryType.Deathrattle:
            case BattleLogEntryType.Damage:
                return damageClip;
            case BattleLogEntryType.DivineShieldPrevented:
                return divineShieldClip;
            case BattleLogEntryType.MinionDied:
                return deathClip;
            case BattleLogEntryType.GameEnded:
                return gameEndedClip;
            default:
                return null;
        }
    }

    /// <summary>
    /// 播放一次缩放脉冲：先从原始大小放大，再缩回原始大小。
    /// 协程每帧执行一小步，所以它不会卡住主线程，也不会阻止 Core 继续运行。
    /// </summary>
    private IEnumerator PlayPulseRoutine(Transform target)
    {
        // 先记录本次动画目标和它的初始缩放，方便结束或中断时还原。
        pulsingTarget = target;
        pulsingOriginalScale = target.localScale;

        // 防止 Inspector 中把时间填成 0 或负数，也防止缩放比例小于 1 导致反向缩小。
        float duration = Mathf.Max(0.01f, shortDuration);
        Vector3 targetScale = pulsingOriginalScale * Mathf.Max(1f, smallPulseScale);

        // 第一段：从原始大小逐步放大到目标大小。
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.Lerp(pulsingOriginalScale, targetScale, t);
            yield return null;
        }

        // 第二段：从放大后的大小逐步缩回原始大小。
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.Lerp(targetScale, pulsingOriginalScale, t);
            yield return null;
        }

        // 动画正常结束后也统一走还原方法，避免浮点插值留下很小的缩放误差。
        ResetPulsingTargetScale();
        pulseCoroutine = null;
    }

    /// <summary>
    /// 把当前正在缩放的目标恢复到动画开始前的大小，并清空运行时记录。
    /// 这个方法会在动画正常结束时调用，也会在动画被新反馈打断时调用。
    /// </summary>
    private void ResetPulsingTargetScale()
    {
        if (pulsingTarget != null)
        {
            pulsingTarget.localScale = pulsingOriginalScale;
        }

        pulsingTarget = null;
        pulsingOriginalScale = Vector3.one;
    }
}
