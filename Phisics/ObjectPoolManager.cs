using Assets.Infrastructure.ServiceLocator;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Infrastructure.Phisics
{
    public class ObjectPoolManager
    {
        private readonly Dictionary<int, GameObject> _gameObjectRegistry = new();
        private readonly Dictionary<int, PhysicsBody> _physicsBodyRegistry = new();
        private readonly Dictionary<int, int> _unificationMainAdditionalBuffers = new();
        private readonly Stack<int> _availableMainSlots;
        private readonly Stack<int> _availableAdditionalSlots;
        private readonly int _capacity;

        public ObjectPoolManager(int capacity)
        {
            _capacity = capacity;
            _availableMainSlots = new Stack<int>(capacity);
            _availableAdditionalSlots = new Stack<int>(capacity);

            for (int i = capacity - 1; i >= 0; i--)
            {
                _availableMainSlots.Push(i);
                _availableAdditionalSlots.Push(i);
            }
        }

        public bool TryGetMainSlot(out int index) => _availableMainSlots.TryPop(out index);

        public bool TryGetAdditionalSlot(out int index) => _availableAdditionalSlots.TryPop(out index);

        public void ReturnMainSlot(int index)
        {
            _availableMainSlots.Push(index);
            _gameObjectRegistry.Remove(index);
            _physicsBodyRegistry.Remove(index);
        }

        public void ReturnAdditionalSlot(int index) => _availableAdditionalSlots.Push(index);

        public void RegisterGameObject(int index, GameObject gameObject) => _gameObjectRegistry[index] = gameObject;

        public void RegisterPhysicsBody(int index, PhysicsBody body) => _physicsBodyRegistry[index] = body;

        public void RegisterUnificationOfBuffers(int mainBufferIndex, int additionalBufferIndex) => 
            _unificationMainAdditionalBuffers[mainBufferIndex] = additionalBufferIndex;

        public bool TryGetGameObject(int index, out GameObject gameObject) => _gameObjectRegistry.TryGetValue(index, out gameObject);

        public bool TryGetPhysicsBody(int index, out PhysicsBody body) => _physicsBodyRegistry.TryGetValue(index, out body);

        public Dictionary<int, GameObject> GetRegistry() => _gameObjectRegistry;
    }
}
