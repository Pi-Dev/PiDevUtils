using UnityEngine;
using UnityEngine.Events;

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
 * A reusable countdown timer for Unity that triggers events on each second and upon completion.
 * Ideal for use in gameplay events, cooldowns, or timed UI interactions.
 * Timer must be manually updated each frame via the Update() method.
 *
 * ============= Usage =============
 * timer.onSecondPassed.AddListener(sec => Debug.Log($"Seconds left: {sec}"));
 * timer.onTimedOut.AddListener(() => Debug.Log("Timer finished"));
 * timer.Start(seconds);
 * you also must call timer.Update() in your Update() method.
 */

namespace PiDev.Utilities
{
    /// <summary>
    /// A simple countdown timer that can be used to track time intervals.
    /// </summary>
    [System.Serializable]

    public class CountdownTimer
    {
        public UnityEvent<int> onSecondPassed; // Event fired each second, passing remaining seconds
        public UnityEvent onTimedOut;          // Event fired when the countdown finishes

        private double startTime;
        private int initialTimeInSeconds;
        private int lastSecondReported;
        private bool isRunning;

        public CountdownTimer()
        {
            onSecondPassed = new UnityEvent<int>();
            onTimedOut = new UnityEvent();
            isRunning = false;
        }

        /// <summary>
        /// Starts the countdown with a specified time.
        /// </summary>
        /// <param name="timeInSeconds">The countdown time in seconds.</param>
        public void Start(int timeInSeconds)
        {
            initialTimeInSeconds = timeInSeconds;
            startTime = Time.timeAsDouble;
            lastSecondReported = timeInSeconds;
            isRunning = true;
        }

        /// <summary>
        /// Stops the countdown.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
        }

        /// <summary>
        /// Updates the countdown timer. This method should be called once per frame.
        /// </summary>
        public void Update()
        {
            if (!isRunning)
                return;

            // Calculate the elapsed time in seconds
            int elapsedSeconds = (int)(Time.timeAsDouble - startTime);
            int remainingTime = initialTimeInSeconds - elapsedSeconds;

            // Trigger the onSecondPassed event when a new second has passed
            if (remainingTime != lastSecondReported && remainingTime > 0)
            {
                lastSecondReported = remainingTime;
                onSecondPassed?.Invoke(remainingTime);
            }

            // Check if the timer has finished
            if (remainingTime <= 0 && isRunning)
            {
                isRunning = false;
                onTimedOut?.Invoke();
            }
        }

        /// <summary>
        /// Returns the elapsed time since the countdown started.
        /// </summary>
        public int GetElapsedTime()
        {
            return isRunning ? (int)(Time.timeAsDouble - startTime) : initialTimeInSeconds;
        }

        /// <summary>
        /// Returns the remaining time until the countdown finishes.
        /// </summary>
        public int GetRemainingTime()
        {
            return Mathf.Max(initialTimeInSeconds - GetElapsedTime(), 0);
        }


        /// <summary>
        /// Checks if the timer is currently running.
        /// </summary>
        public bool IsRunning => isRunning;
    }
}