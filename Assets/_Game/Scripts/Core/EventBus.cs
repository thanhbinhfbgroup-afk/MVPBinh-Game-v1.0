using System;
using System.Collections.Generic;

namespace BillGameCore.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Action<object>>> _subscribers = new();
        public static void Subscribe<T>(Action<T> handler) {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type)) _subscribers[type] = new List<Action<object>>();
            _subscribers[type].Add(obj => handler((T)obj));
        }
        public static void Publish<T>(T signal) {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type)) foreach (var handler in _subscribers[type]) handler(signal);
        }
    }
}