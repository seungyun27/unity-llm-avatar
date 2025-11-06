using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityLLMAvatar.Events
{
    public struct Unit
    {
        public static Unit Default => default;
    }

    public interface IGameEventListener<in T>
    {
        void OnEvent(T data);
    }

    public class GameEventListener<T> : MonoBehaviour, IGameEventListener<T>
    {
        [SerializeField]
        private GameEvent<T> _gameEvent;

        [SerializeField]
        private UnityEvent<T> _response;

        private void OnEnable() => _gameEvent.Register(this);
        private void OnDisable() => _gameEvent.Deregister(this);

        public void OnEvent(T data) => _response.Invoke(data);
    }

    public class GameEventListener : GameEventListener<Unit> { }

    public class GameEvent<T> : ScriptableObject
    {
        private readonly List<IGameEventListener<T>> _listeners = new();

        public void Raise(T data)
        {
            for (var i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].OnEvent(data);
            }
        }

        public void Register(IGameEventListener<T> listener) => _listeners.Add(listener);
        public void Deregister(IGameEventListener<T> listener) => _listeners.Remove(listener);
    }

    [CreateAssetMenu(menuName = "GameEvent")]
    public class GameEvent : GameEvent<Unit>
    {
        public void Raise() => Raise(Unit.Default);
    }
}