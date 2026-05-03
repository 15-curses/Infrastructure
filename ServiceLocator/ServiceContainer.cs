using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Infrastructure.ServiceLocator
{
    public static class ServiceContainer
    {
        #region DictionaryS
        private static Dictionary<Type, Func<object>> _transients = new();
        private static Dictionary<Type, object> _singletons = new();
        private static Dictionary<Type, object> _currentScope = new();

        private static Dictionary<Type, Func<object>> _scopedFactories = new();
        #endregion

        #region Registers
        public static void RegisterTransient<T>(Func<T> factory) where T : class
        {
            _transients[typeof(T)] = () => factory();
        }

        public static void RegisterSingleton<T>(T instance) where T : class
        {
            _singletons[typeof(T)] = instance;
        }
        public static void RegisterSingleton<T>() where T : class, new()
        {
            var instance = new T();
            _singletons[typeof(T)] = instance;
        }
        public static T RegisterMonoSingleton<T>(T instance = null) where T : MonoBehaviour
        {
            if (instance == null)
                instance = UnityEngine.Object.FindFirstObjectByType<T>();

            if (instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name);
                instance = go.AddComponent<T>();
            }

            UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);

            RegisterSingleton(instance);

            return instance;
        }

        public static void RegisterScoped<T>(Func<T> factory) where T : class
        {
            _scopedFactories[typeof(T)] = () => factory();
        }
        #endregion

        #region Get
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);

            #region Singleton
            if (_singletons.TryGetValue(type, out var singleton))
                return (T)singleton;
            #endregion

            #region Scoped
            if (_currentScope.TryGetValue(type, out var scoped))
                return (T)scoped;

            if (_scopedFactories.TryGetValue(type, out var scopedFactory))
            {
                var instance = scopedFactory();
                _currentScope[type] = instance;
                return (T)instance;
            }
            #endregion

            #region Transient
            if (_transients.TryGetValue(type, out var transientFactory))
            {
                return (T)transientFactory();
            }
            #endregion

            throw new Exception($"Сервис {type} не зарегистрирован"); // ошибку каст
        }
        #endregion

        #region Clear
        public static void ClearScope()
        {
            _currentScope.Clear();
        }
        public static void ClearAll()
        {
            _singletons.Clear();
            _transients.Clear();
            _currentScope.Clear();
        }
        #endregion

        #region Graph
        public static string GetDependencyGraph()
        {
            var sb = new StringBuilder();

            var allKeys = _singletons.Keys
                .Union(_transients.Keys)
                .Union(_currentScope?.Keys ?? Enumerable.Empty<Type>());

            foreach (var type in allKeys)
            {
                sb.AppendLine($"├── {type.Name} (Singleton)");
                var deps = GetDependencies(type);
                foreach (var dep in deps)
                    sb.AppendLine($"│   └── depends on: {dep.Name}");
            }
            return sb.ToString();
        }
        private static List<Type> GetDependencies(Type type)
        {
            var constructor = type.GetConstructors().FirstOrDefault();
            if (constructor == null) return new List<Type>();

            return constructor.GetParameters().Select(p => p.ParameterType).ToList();
        }
        #endregion
    }
}
