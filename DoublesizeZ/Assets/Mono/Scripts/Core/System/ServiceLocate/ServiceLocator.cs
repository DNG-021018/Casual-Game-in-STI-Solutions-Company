using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoublesideZ
{
    [DefaultExecutionOrder(-1000)]
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _instance;
        private readonly Dictionary<Type, object> _services = new();

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new("ServiceLocator [Global]");
                    _instance = go.AddComponent<ServiceLocator>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #region Register Services

        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);

            if (Instance._services.ContainsKey(type))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered! Overwriting...");
#endif
            }

            Instance._services[type] = service;
#if UNITY_EDITOR
            Debug.Log($"[ServiceLocator] Registered: {type.Name}");
#endif
        }

        #endregion

        #region Get Services

        public static T Get<T>() where T : class
        {
            Type type = typeof(T);

            if (Instance._services.TryGetValue(type, out object service))
            {
                return service as T;
            }

#if UNITY_EDITOR
            Debug.LogError($"[ServiceLocator] Service {type.Name} not found! Did you forget to register it?");
#endif
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);

            if (Instance._services.TryGetValue(type, out object foundService))
            {
                service = foundService as T;
                return true;
            }

            service = null;
            return false;
        }

        public static bool Has<T>() where T : class
        {
            return Instance._services.ContainsKey(typeof(T));
        }

        #endregion

        #region Unregister Services

        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);

            if (Instance._services.Remove(type))
            {
#if UNITY_EDITOR
                Debug.Log($"[ServiceLocator] Unregistered: {type.Name}");
#endif
            }
        }
        #endregion
    }
}
