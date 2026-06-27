using System;
using System.Collections.Generic;

/// <summary>
/// 游戏事件总线。
/// 负责保存监听方法，并在事件发布时通知它们。
/// </summary>
public class GameEventBus
{
    private readonly Dictionary<GameEventType, List<Action<GameEvent>>> listeners =
        new Dictionary<GameEventType, List<Action<GameEvent>>>();

    /// <summary>
    /// 订阅一种事件。
    /// </summary>
    public void Subscribe(GameEventType eventType, Action<GameEvent> listener)
    {
        if (listener == null) return;

        if (!listeners.ContainsKey(eventType))
        {
            listeners[eventType] = new List<Action<GameEvent>>();
        }

        if (listeners[eventType].Contains(listener)) return;

        listeners[eventType].Add(listener);
    }

    /// <summary>
    /// 取消订阅一种事件。
    /// </summary>
    public void Unsubscribe(GameEventType eventType, Action<GameEvent> listener)
    {
        if (listener == null) return;
        if (!listeners.ContainsKey(eventType)) return;

        listeners[eventType].Remove(listener);

        if (listeners[eventType].Count == 0)
        {
            listeners.Remove(eventType);
        }
    }

    /// <summary>
    /// 发布事件，并通知所有订阅该事件类型的方法。
    /// </summary>
    public void Publish(GameEvent gameEvent)
    {
        if (gameEvent == null) return;
        if (!listeners.ContainsKey(gameEvent.Type)) return;

        List<Action<GameEvent>> eventListenersSnapshot = new List<Action<GameEvent>>(listeners[gameEvent.Type]);
        for (int i = 0; i < eventListenersSnapshot.Count; i++)
        {
            eventListenersSnapshot[i]?.Invoke(gameEvent);
        }
    }
}
