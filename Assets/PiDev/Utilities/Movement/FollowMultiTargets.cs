using System.Collections.Generic;
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
 * Follows the average position and orientation of multiple target transforms.
 * Supports optional axis snapping to align the resulting rotation with a reference transform.
 * Useful for group tracking, midpoints, or collective indicators.
 *
 * ============= Usage =============
 * Add transforms to 'targets' and assign a reference to 'objectRoot' for axis snapping.
 * Adjust axisSnapStrength to control how strongly to align with the reference up direction.
 */

namespace PiDev.Utilities
{
    public class FollowMultiTargets : MonoBehaviour
    {
        public List<Transform> targets;
        public Vector3 SnapToAxis;
        public Transform objectRoot;
        public float axisSnapStrength; // 0 = follow the average orientation of targets, 1 = snapped to reference axis

        void Update()
        {
            if (targets == null || targets.Count == 0)
                return;

            // Calculate the average position of all targets
            Vector3 averagePosition = Vector3.zero;
            foreach (Transform target in targets)
            {
                if (target != null)
                {
                    averagePosition += target.position;
                }
            }
            averagePosition /= targets.Count;

            // Set the position of this object to follow the average position
            transform.position = averagePosition;

            // Calculate the average rotation of all targets
            Quaternion averageRotation = Quaternion.identity;
            int validTargetCount = 0;

            foreach (Transform target in targets)
            {
                if (target != null)
                {
                    averageRotation = Quaternion.Slerp(averageRotation, target.rotation, 1.0f / ++validTargetCount);
                }
            }

            // Snap the rotation to the specified axis
            if (axisSnapStrength > 0)
            {
                // Snap to the nearest reference axis while preserving rotation
                Vector3 currentUp = averageRotation * Vector3.up; // Local "up" direction
                SnapToAxis = objectRoot.transform.up;
                Vector3 snappedUp = Vector3.Lerp(currentUp, SnapToAxis, axisSnapStrength).normalized;

                // Reconstruct the rotation while snapping the "up" vector
                averageRotation = Quaternion.LookRotation(averageRotation * Vector3.forward, snappedUp);
            }

            // Apply the calculated rotation to this object
            transform.rotation = averageRotation;
        }
    }
}