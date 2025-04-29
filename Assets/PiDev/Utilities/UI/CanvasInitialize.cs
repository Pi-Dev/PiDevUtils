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
 * Automatically configures a Canvas component at runtime to use a specified camera and plane distance.
 * Optionally hides the Canvas on non-touchscreen devices for optimized UI handling.
 * Runs early in the execution order to ensure UI is properly initialized before other scripts run.
 *
 * ============= Usage =============
 * Attach to a Canvas GameObject and assign the camera.
 * Optionally enable 'HideOnNonTouchDevices' for mobile-specific UIs.
 */

namespace PiDev.Utilities.UI
{
    [DefaultExecutionOrder(-10)]
    public class CanvasInitialize : MonoBehaviour
    {
        public bool HideOnNonTouchDevices;
        public float planeDistance = 0.2f;
        public Camera targetCamera;
        private void Start()
        {
            var c = GetComponent<Canvas>();
            if (targetCamera != null)
            {
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.planeDistance = planeDistance;
                c.worldCamera = targetCamera;
            }
            if (HideOnNonTouchDevices)
            {
                if (!PortingUtils.IsDeviceWithTouchscreen())
                    gameObject.SetActive(false);
            }

        }
    }
}