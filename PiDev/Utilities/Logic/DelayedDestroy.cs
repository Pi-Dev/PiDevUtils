using UnityEngine;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * A simple Unity MonoBehaviour that automatically destroys its GameObject after a delay.
 * Useful for temporary effects like explosions, particles, or UI elements.
 *
 * ============= Usage =============
 * Attach to a GameObject and set the delay in the inspector or via script.
 */
namespace PiDev.Utilities
{
    public class DelayedDestroy : MonoBehaviour
    {
        public float delay = 1f;
        void Start()
        {
            Destroy(gameObject, delay);
        }
    }
}