using System;
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
 * Generic value interpolator for smoothly transitioning between values using linear or lerped modes.
 * Accepts custom interpolation functions for full type flexibility (float, Vector types, Quaternion, etc.).
 * Use the Update() method manually each frame, passing deltaTime to progress the interpolation.
 * Supports different speeds for increasing vs. decreasing values via optional negativeSpeed logic.
 *
 * ============= Usage =============
 * var interp = ValueInterpolator.Float(0, 10);
 * interp.targetValue = 20;
 * interp.Update(Time.deltaTime); // you must call this when recalculation is needed
 * Debug.Log(interp.currentValue);
 */

namespace PiDev.Utilities
{
    [Serializable]
    public class ValueInterpolator<T>
    {
        public enum Mode
        {
            Linear, Lerp
        }
        public Mode mode = Mode.Linear;
        public float speed = 100f;
        public float lerpf = 1f;
        public bool useNegativeSpeed;
        public float negativeSpeed;

        public T currentValue;
        public T targetValue;

        private readonly Func<T, T, float, T> moveTowardsFunc;
        private readonly Func<T, T, float, T> lerpFunc;
        private readonly Func<T, T, bool> comparisonFunc;

        public ValueInterpolator(
            T current,
            T target,
            Func<T, T, float, T> moveTowards,
            Func<T, T, float, T> lerp,
            Func<T, T, bool> comparison = null)
        {
            currentValue = current;
            targetValue = target;
            moveTowardsFunc = moveTowards;
            lerpFunc = lerp;
            comparisonFunc = comparison;
        }

        /// <summary>
        /// Manually update this interpolator each frame.
        /// </summary>
        /// <param name="dt">Delta time</param>
        public void Update(float dt)
        {
            float currentSpeed = useNegativeSpeed && ShouldUseNegativeSpeed() ? negativeSpeed : speed;

            if (mode == Mode.Linear)
            {
                currentValue = moveTowardsFunc(currentValue, targetValue, dt * currentSpeed);
            }
            else if (mode == Mode.Lerp)
            {
                T intermediateValue = moveTowardsFunc(currentValue, targetValue, dt * currentSpeed);
                currentValue = lerpFunc(currentValue, intermediateValue, lerpf);
            }
        }

        private bool ShouldUseNegativeSpeed()
        {
            if (comparisonFunc != null) return comparisonFunc(currentValue, targetValue);
            else return false;
        }
    }

    // Factory class for most common ValueInterpolators
    public static class ValueInterpolator
    {
        public static ValueInterpolator<float> Float(float current = 0, float target = 0)
            => new ValueInterpolator<float>(current, target,
                Mathf.MoveTowards, Mathf.Lerp, (a, b) => a > b);

        public static ValueInterpolator<Vector3> Vector3(Vector3 current = default, Vector3 target = default)
            => new ValueInterpolator<Vector3>(current, target,
                UnityEngine.Vector3.MoveTowards, UnityEngine.Vector3.Lerp, (a, b) => a.sqrMagnitude > b.sqrMagnitude);

        public static ValueInterpolator<Vector4> Vector4(Vector4 current = default, Vector4 target = default)
            => new ValueInterpolator<Vector4>(current, target,
                UnityEngine.Vector4.MoveTowards, UnityEngine.Vector4.Lerp, (a, b) => a.sqrMagnitude > b.sqrMagnitude);

        public static ValueInterpolator<Vector2> Vector2(Vector2 current = default, Vector2 target = default)
            => new ValueInterpolator<Vector2>(current, target,
                UnityEngine.Vector2.MoveTowards, UnityEngine.Vector2.Lerp, (a, b) => a.sqrMagnitude > b.sqrMagnitude);

        public static ValueInterpolator<Quaternion> Quaternion(Quaternion current = default, Quaternion target = default)
            => new ValueInterpolator<Quaternion>(current, target,
                UnityEngine.Quaternion.RotateTowards, UnityEngine.Quaternion.Slerp, (a, b) => false);
    }
}