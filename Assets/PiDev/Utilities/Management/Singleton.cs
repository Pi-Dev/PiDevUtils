using System.Runtime.CompilerServices;
using UnityEngine;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * A collection of singletons that provide different behaviors for different use cases.
 *
 * ============= Usage =============
 * public class GameManager : GlobalSingleton<GameManager> { }
 * public class AudioManager : Singleton<AudioManager> { }
 * public class SceneLogic : SceneSingleton<SceneLogic> { }
 * Access the singleton with MyManager.instance.
 */

namespace PiDev.Utilities
{
    public abstract class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;
        protected static bool _isShuttingDown;

        public static T Instance => _instance;
        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                OnInitialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnInitialize() { }

        protected virtual void OnApplicationQuit() => _isShuttingDown = true;

        protected virtual void OnDestroy()
        {
            // if (_instance == this)
            //     _isShuttingDown = true;
        }

        protected static string GetDefaultName()
        {
            if (typeof(GlobalSingleton<T>).IsAssignableFrom(typeof(T)))
                return $"{typeof(T).Name} (GlobalSingleton)";
            if (typeof(LazySingleton<T>).IsAssignableFrom(typeof(T)))
                return $"{typeof(T).Name} (LazySingleton)";
            return $"{typeof(T).Name} (Singleton)";
        }
    }

    /// <summary>
    /// Must exist in the scene manually. Will not be created if missing.
    /// </summary>
    public abstract class SceneSingleton<T> : SingletonBase<T> where T : MonoBehaviour
    {
        public new static T Instance => _isShuttingDown ? null : _instance;
    }

    /// <summary>
    /// Created on first access if not present. Destroyed on scene unload.
    /// </summary>
    public abstract class LazySingleton<T> : SingletonBase<T> where T : MonoBehaviour
    {
        public new static T Instance
        {
            get
            {
                if (_isShuttingDown)
                {
                    Debug.LogWarning($"[LazySingleton] instance of {typeof(T)} is already destroyed.");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject(GetDefaultName());
                        _instance = obj.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// Created on first access if not present. Persists across scene loads.
    /// </summary>
    public abstract class GlobalSingleton<T> : LazySingleton<T> where T : MonoBehaviour
    {
        protected override void OnInitialize()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
