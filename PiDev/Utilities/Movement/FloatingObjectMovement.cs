using PiDev;
using System.Collections;
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
 * Adds smooth, random floating motion to an object with optional target-following.
 * Generates a new random offset at intervals and interpolates towards it using SmoothDamp.
 * Supports distance-based radius scaling and optional movement multipliers per axis.
 *
 * ============= Usage =============
 * Attach to a GameObject and configure radius, interval, and damping.
 * Optionally assign a FollowTarget to float relative to another transform.
 */

namespace PiDev.Utilities
{
    public class FloatingObjectMovement : MonoBehaviour
    {

        public float radius = 1f;
        public float damping = 0.5f;
        public float interval = 1f;
        public Vector3 multiplier = Vector3.one;
        public Vector3 objectPosition;
        public bool useDistanceBasedRadius;

        [Header("Optional, if set will follow it")]
        public Transform FollowTarget;
        public Vector3 FollowTargetOffset;
        public void SetFollowTarget(Transform target)
        {
            FollowTarget = target;
        }

        [Header("Debug")]
        public Vector3 target;

        Coroutine cr;
        private void OnEnable()
        {
            objectPosition = transform.position;
            cr = StartCoroutine(NewTarget());
        }

        private IEnumerator NewTarget()
        {
            while (true)
            {
                float r;
                if (useDistanceBasedRadius)
                {
                    var dist = Vector3.Distance(transform.position, target);
                    r = Mathf.Max(radius, dist);
                }
                else r = radius;

                if (FollowTarget != null) objectPosition = FollowTarget.position + FollowTargetOffset;

                target = (UnityEngine.Random.insideUnitSphere * r).MultiplyMembers(multiplier) + objectPosition;
                yield return new WaitForSeconds(interval);
            }

        }

        private void OnDisable()
        {
            StopCoroutine(cr);
        }

        // Use this for initialization
        void Start()
        {

        }

        Vector3 velocity;
        void LateUpdate()
        {
            if (!FollowTarget.gameObject.activeInHierarchy) gameObject.SetActive(false);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, damping, Mathf.Infinity, Time.deltaTime);
        }
    }
}