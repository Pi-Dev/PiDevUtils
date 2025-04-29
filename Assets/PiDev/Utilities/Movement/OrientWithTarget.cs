using UnityEngine;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * 
 * The MIT License (MIT)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * ============= Description =============
 * Orients this GameObject to match a target transform’s rotation.
 * Automatically applies Rigidbody-based rotation if available.
 * Ideal for syncing orientation of child objects, indicators, or effects.
 *
 * ============= Usage =============
 * Assign a target transform to 'target' and optionally use Rigidbody for physical alignment.
 */

namespace PiDev.Utilities
{
    [ExecuteInEditMode]
    public class OrientWithTarget : MonoBehaviour
    {
        public Transform target;
        public bool DestroyIfTargetMissing;

        Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                if (rb) rb.MoveRotation(target.rotation);
                else transform.rotation = target.rotation;
            }
            else transform.rotation = target.rotation;
        }
    }
}