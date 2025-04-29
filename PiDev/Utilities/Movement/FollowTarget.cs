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
 * Follows a target transform in applying a configurable offset.
 * Automatically moves using Rigidbody if available; destroys self if the target is missing (optional).
 * Useful for attaching floating UI, indicators, or effects to dynamic objects.
 *
 * ============= Usage =============
 * Assign a target in the inspector or via SetTarget().
 * Enable DestroyIfTargetMissing to auto-clean when the target disappears.
 */

namespace PiDev.Utilities
{

    [ExecuteInEditMode]
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 7.5f, 0f);
        public bool DestroyIfTargetMissing;

        Rigidbody rb;
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void SetTarget(Transform t, bool destroyIfMissing = false, Vector3? offset = null)
        {
            target = t;
            DestroyIfTargetMissing = destroyIfMissing;
            if (offset != null) this.offset = offset.Value;
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (target != null)
            {
                if (Application.isPlaying)
                {
                    if (rb) rb.MovePosition(target.position + offset);
                    else transform.position = target.position + offset;
                }
                else transform.position = target.position + offset;
            }
            else if (Application.isPlaying && DestroyIfTargetMissing) Destroy(gameObject);
#else
            if (target != null)
            {
                if (rb) rb.MovePosition(target.position + offset);
                else transform.position = target.position + offset;
            }
            else if(DestroyIfTargetMissing) Destroy(gameObject);
#endif
        }
    }
}