using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Public Domain - 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 *
 * ============= Description =============
 * Simple MonoBehaviour for keeping strong references to Unity Objects to prevent them from being stripped or unloaded.
 * Useful for asset management, ensuring referenced objects are recognized as used during builds or runtime.
 *
 * ============= Usage =============
 * Attach ObjectReferences to a GameObject and assign assets to the 'references' array in the Inspector.
 */

namespace PiDev.Utilities
{
    public class ObjectReferences : MonoBehaviour
    {

        public Object[] references;
    }
}