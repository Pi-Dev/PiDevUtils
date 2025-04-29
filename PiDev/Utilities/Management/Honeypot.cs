using System;
using System.Runtime.InteropServices;
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
 * Honeypot memory trap for detecting and reacting to tampering of sensitive data.
 * Stores values in multiple memory locations and checks for unauthorized changes.
 * Triggers an action if tampering is detected and occasionally randomizes real value position.
 *
 * ============= Usage =============
 * var honeypot = new Honeypot<int>(initialValue, () => Debug.Log("Tampering detected!"));
 * honeypot.SetValue(newValue);
 * var currentValue = honeypot.GetValue();
 * honeypot.CheckForTampering();
 * honeypot.Dispose();
 */

namespace PiDev.Utilities
{
    public class Honeypot<T> where T : struct
    {
        private T v1;
        private T v2;
        private T v3;
        private GCHandle gc1;
        private GCHandle gc2;
        private GCHandle gc3;
        private IntPtr valuePtr1;
        private IntPtr valuePtr2;
        private IntPtr valuePtr3;
        private int valId; // Randomly switches between value1, value2, honeypot
        private Action onTrigger;
        private readonly Action prevaction;

        public Honeypot(T initialValue, Action triggerAction)
        {
            v1 = initialValue;
            v2 = initialValue;
            v3 = GenerateValues();

            prevaction = triggerAction;
            onTrigger = triggerAction;

            gc1 = GCHandle.Alloc(v1, GCHandleType.Pinned);
            gc2 = GCHandle.Alloc(v2, GCHandleType.Pinned);
            gc3 = GCHandle.Alloc(v3, GCHandleType.Pinned);
            valuePtr1 = gc1.AddrOfPinnedObject();
            valuePtr2 = gc2.AddrOfPinnedObject();
            valuePtr3 = gc3.AddrOfPinnedObject();

            // Randomly assign which value is considered the real one
            valId = UnityEngine.Random.Range(0, 3);

            // Debug.Log($"[Honeypot] Initialized. Real value stored at index: {valId}");
        }

        private T GenerateValues()
        {
            if (typeof(T) == typeof(int)) return (T)(object)999999;
            if (typeof(T) == typeof(float)) return (T)(object)9999.99f;
            if (typeof(T) == typeof(string)) return (T)(object)"PlayerScore";
            return default;
        }

        public T GetValue()
        {
            if (UnityEngine.Random.Range(0, 20) < 2) // 20% chance per check
                CheckForTampering();
            switch (valId)
            {
                case 0: return v1;
                case 1: return v2;
                case 2: return v3;
                default: return v1;
            }
        }

        public void SetValue(T newValue)
        {
            switch (valId)
            {
                case 0: v1 = newValue; break;
                case 1: v2 = newValue; break;
                case 2: v3 = newValue; break;
            }
        }

        public void CheckForTampering()
        {
            bool tampered = false;

            switch (valId)
            {
                case 0: tampered = !v1.Equals(gc1.Target); break;
                case 1: tampered = !v2.Equals(gc2.Target); break;
                case 2: tampered = !v3.Equals(gc3.Target); break;
            }

            // Check all locations
            if (Marshal.ReadInt32(valuePtr1) != Convert.ToInt32(v1) ||
                Marshal.ReadInt32(valuePtr2) != Convert.ToInt32(v2) ||
                Marshal.ReadInt32(valuePtr3) != Convert.ToInt32(v3))
            {
                tampered = true;
            }

            // Check if the action was modified
            if (onTrigger != prevaction)
            {
                //Debug.LogError("[Honeypot] Bypass (action tampered)!");
                Application.Quit();
            }

            if (tampered)
            {
                //Debug.LogWarning("[Honeypot] Memory tampering detected!");
                onTrigger?.Invoke();
            }

            // Occasionally change which location is "real"
            if (UnityEngine.Random.Range(0, 10) < 2) // 20% chance per check
            {
                valId = UnityEngine.Random.Range(0, 3);
                //Debug.Log($"[Honeypot] Real value location changed to index: {valId}");
            }
        }

        public void Dispose()
        {
            if (gc1.IsAllocated) gc1.Free();
            if (gc2.IsAllocated) gc2.Free();
            if (gc3.IsAllocated) gc3.Free();
        }
    }
}