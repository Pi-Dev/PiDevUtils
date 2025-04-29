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
namespace PiDev.Utilities
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isShuttingDown = false;

        public static T instance
        {
            get
            {
                if (_isShuttingDown)
                {
                    Debug.LogWarning($"[Singleton] instance of {typeof(T)} is already destroyed. Returning null.");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject($"{typeof(T)} (Singleton)");
                        _instance = singletonObject.AddComponent<T>();
                        // DontDestroyOnLoad(singletonObject);
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
                // DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject); // Destroy duplicate instances
            }
        }

        private void OnApplicationQuit()
        {
            _isShuttingDown = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _isShuttingDown = true;
            }
        }
    }
}