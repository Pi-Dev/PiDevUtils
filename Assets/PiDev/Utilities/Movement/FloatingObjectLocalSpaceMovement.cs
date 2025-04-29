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
 * Adds oscillating movement and rotation in local space for floating effects.
 * Can optionally sync with FollowTarget and OrientWithTarget components for dynamic references.
 * Resets transform on disable to ensure consistent behavior when re-enabled.
 *
 * ============= Usage =============
 * Attach to a GameObject with FollowTarget or OrientWithTarget if needed.
 * Configure movement, frequency, and rotation axis in the inspector.
 */

namespace PiDev.Utilities
{
    [DefaultExecutionOrder(1)] // After FollowTarget at 0
    public class FloatingObjectLocalSpaceMovement : MonoBehaviour
    {
        // Start is called before the first frame update

        [Header("Position")]
        public Vector3 Movement = Vector3.up;
        public float MovementFrequency = 5;

        [Header("Rotation")]
        public Vector3 rotationAxis;
        public float maxAngle;
        public float RotationFrequency = 5;

        Vector3 initialPos;
        Quaternion initialRot;

        FollowTarget ft;
        OrientWithTarget ort;

        void OnEnable()
        {
            ort = GetComponent<OrientWithTarget>();
            ft = GetComponent<FollowTarget>();
            initialPos = transform.localPosition;
            initialRot = transform.localRotation;
        }

        void OnDisable()
        {
            transform.localPosition = initialPos;
            transform.localRotation = initialRot;
        }

        private void Update()
        {
            if (ft) initialPos = ft.target.position + ft.offset;
            if (ort) initialRot = ort.target.rotation;
            transform.localPosition = initialPos + Movement * Mathf.Sin(Time.unscaledTime * MovementFrequency);
            transform.localRotation = Quaternion.AngleAxis(Mathf.Cos(Time.unscaledTime * RotationFrequency) * maxAngle, rotationAxis) * initialRot;
        }
    }
}