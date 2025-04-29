using UnityEngine;
using UnityEngine.EventSystems;

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
 * Touch-based joystick for mobile UI, using drag input to update a direction vector.
 * Supports dynamic repositioning on touch and configurable clamp radius for knob movement.
 * Ideal for character control or camera panning on mobile platforms.
 *
 * ============= Usage =============
 * Attach to a Canvas UI element and assign references to a knob and background.
 * Use GetDirection() to retrieve the current joystick input as a Vector2.
 */

namespace PiDev.Utilities
{
    public class MobileUIJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public RectTransform mobileControlMove;  // Reference to MobileControlMove widget
        public RectTransform knob;               // Reference to the Knob child
        public float maxDistance = 100f;         // Maximum distance knob can be moved

        private Vector2 inputVector;             // Stores input direction vector
        private Vector2 startPosition;           // Start position of MobileControlMove
        private bool isDragging;                 // Check if dragging is in progress

        private void Start()
        {
            // Store the initial position of the MobileControlMove widget
            startPosition = mobileControlMove.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Move the MobileControlMove widget to the touch position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)mobileControlMove.parent,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint
            );

            mobileControlMove.anchoredPosition = localPoint;
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Move the knob to follow the finger position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mobileControlMove,
                eventData.position,
                eventData.pressEventCamera,
                out var position
            );

            // Clamp the knob's position to the maxDistance
            inputVector = Vector2.ClampMagnitude(position, maxDistance);
            knob.anchoredPosition = inputVector;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Reset knob and MobileControlMove position when releasing
            knob.anchoredPosition = Vector2.zero;
            mobileControlMove.anchoredPosition = startPosition;
            inputVector = Vector2.zero;
            isDragging = false;
        }

        public Vector2 GetDirection()
        {
            // Return normalized direction if magnitude > 1, otherwise return actual input vector
            return inputVector.magnitude > 1 ? inputVector.normalized : inputVector;
        }

        public bool IsDragging() => isDragging;

        //private void Update()
        //{
        //    if (isDragging)
        //    {
        //        // Continuously update the knob position based on touch movement
        //        // This may be empty if all the movement logic is handled in OnDrag
        //    }
        //}
    }
}