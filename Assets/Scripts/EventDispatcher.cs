using System;
using System.Collections.Generic;

// dispatches an object of type *T* to subscribers whenever invoked
public class EventDispatcher<T>
{
    List<Action<T>> subscribers;

    public EventDispatcher(out Action<T> invoke)
    {
        subscribers = new List<Action<T>>();

        invoke = (t) => Invoke(t);
    }

    // returns the unsubscribe action for *subscriber*
    public Action Subscribe(Action<T> subscriber)
    {
        int i = subscribers.FindIndex((x) => x == null);
        if (i == -1)                                                 // if there are no null subscribers
        {
            subscribers.Add(subscriber);
            return () => subscribers[subscribers.Count - 1] = null;
        }
        subscribers[i] = subscriber;
        return () => subscribers[i] = null;
    }

    void Invoke(T eventData)
    {
        for (int i = 0; i < subscribers.Count; i++)
            subscribers[i]?.Invoke(eventData);
    }
}
