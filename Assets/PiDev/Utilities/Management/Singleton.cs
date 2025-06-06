using UnityEngine;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Yet another VERY SIMPLE and stupid MonoBehaviour singleton implementation with 
 * automatic instance creation and shutdown safety.
 * Ensures a single instance of T exists at runtime, and destroys duplicates if multiple are found.
 * Optional: Uncomment DontDestroyOnLoad if persistence across scenes is required.
 *
 * ============= Usage =============
 * public class MyManager : Singleton<MyManager> { }
 * Access the singleton with MyManager.instance.
 */
using UnityEngine;

namespace PiDev.Utilities
{
    public enum InstanceBehavior
    {
        KeepExistingDestroyNew,
        ReplaceExistingDestroyOld,
        AllowDuplicatesOverrideOldInstance,
        AllowDuplicatesDontOverrideOldInstance
    }

    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isShuttingDown;

        public static T instance
        {
            get
            {
                if (_isShuttingDown)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} is already destroyed.");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null && StaticAutoCreateIfMissing)
                    {
                        GameObject obj = new GameObject(GetDefaultName());
                        _instance = obj.AddComponent<T>();
                    }

                    if (_instance != null && StaticDontDestroyOnLoad)
                    {
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                if (UseDontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);

                OnInitialize();
            }
            else if (_instance != this)
            {
                switch (InstanceManagementMode)
                {
                    case InstanceBehavior.KeepExistingDestroyNew:
                        Destroy(gameObject);
                        break;

                    case InstanceBehavior.ReplaceExistingDestroyOld:
                        Destroy(_instance.gameObject);
                        _instance = this as T;
                        if (UseDontDestroyOnLoad)
                            DontDestroyOnLoad(gameObject);
                        OnInitialize();
                        break;

                    case InstanceBehavior.AllowDuplicatesOverrideOldInstance:
                        _instance = this as T;
                        Debug.LogWarning($"[Singleton] Replaced instance of {typeof(T)} with a duplicate.");
                        break;

                    case InstanceBehavior.AllowDuplicatesDontOverrideOldInstance:
                        Debug.LogWarning($"[Singleton] Duplicate of {typeof(T)} allowed, keeping original instance.");
                        break;
                }
            }
        }

        protected virtual void OnApplicationQuit() => _isShuttingDown = true;

        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _isShuttingDown = true;
        }

        protected virtual void OnInitialize() { }

        protected virtual bool UseDontDestroyOnLoad => Settings.Load().useDontDestroyOnLoad;
        protected virtual bool AutoCreateIfMissing => Settings.Load().autoCreateIfMissing;
        protected virtual InstanceBehavior InstanceManagementMode => Settings.Load().instanceManagementMode;

        // === Static access helpers ===
        private static bool StaticDontDestroyOnLoad
        {
            get
            {
                if (_instance is Singleton<T> inst)
                    return inst.UseDontDestroyOnLoad;
                return true;
            }
        }

        private static bool StaticAutoCreateIfMissing
        {
            get
            {
                if (_instance is Singleton<T> inst)
                    return inst.AutoCreateIfMissing;
                return true;
            }
        }

        private static string GetDefaultName() => $"{typeof(T).Name} (Singleton)";
    }
}
