using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Infrastructure.Phisics
{
    public class IndexedStorage<T>
    {
        private readonly T[] _storage;
        private readonly Queue<int> _freeSlots;
        private int _count;

        public int Capacity => _storage.Length;
        public int Count => _count;

        public IndexedStorage(int capacity)
        {
            _storage = new T[capacity];
            _freeSlots = new Queue<int>(capacity);

            // Инициализируем очередь свободных слотов
            for (int i = 0; i < capacity; i++)
            {
                _freeSlots.Enqueue(i);
            }
        }

        /// <summary>
        /// Добавляет элемент в первый доступный слот и возвращает его индекс
        /// </summary>
        public int Add(T item)
        {
            if (_freeSlots.Count == 0)
                throw new InvalidOperationException("Storage is full");

            int index = _freeSlots.Dequeue();
            _storage[index] = item;
            _count++;
            return index;
        }

        /// <summary>
        /// Удаляет элемент по индексу и возвращает слот в пул свободных
        /// </summary>
        public void Remove(int index)
        {
            if (index < 0 || index >= _storage.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            _storage[index] = default;
            _freeSlots.Enqueue(index);
            _count--;
        }

        /// <summary>
        /// Получает элемент по индексу
        /// </summary>
        public T Get(int index)
        {
            if (index < 0 || index >= _storage.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _storage[index];
        }

        /// <summary>
        /// Устанавливает значение по индексу
        /// </summary>
        public void Set(int index, T value)
        {
            if (index < 0 || index >= _storage.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            _storage[index] = value;
        }

        public void Clear()
        {
            Array.Clear(_storage, 0, _storage.Length);
            _freeSlots.Clear();
            _count = 0;

            for (int i = 0; i < _storage.Length; i++)
            {
                _freeSlots.Enqueue(i);
            }
        }
    }
}
