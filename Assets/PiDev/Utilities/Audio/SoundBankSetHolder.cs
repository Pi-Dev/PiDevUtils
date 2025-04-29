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
 * Component wrapper for SoundBankSet that plays sounds automatically or on demand.
 * Supports optional low-pass filtering and radius visualization for editor debugging.
 * Can delay playback on start and control sound lifecycle via Play/Stop.
 *
 * ============= Usage =============
 * Attach to a GameObject, assign a SoundBankSet, and call Play() or enable PlayOnStart.
 * Use 'useLowPassFilter' to add AudioLowPassFilter with custom frequency.
 */

namespace PiDev.Utilities
{
    public class SoundBankSetHolder : MonoBehaviour
    {
        public SoundBankSet sounds;
        public bool PlayOnStart = false;
        public float PlayOnStartDelay = 0;
        AudioSource playing;
        public bool useLowPassFilter;
        public float lpFilterFrequency = 5000;

        IEnumerator Start()
        {
            if (PlayOnStart)
            {
                yield return new WaitForSeconds(PlayOnStartDelay);
                Play();
            }
        }

        public void Stop()
        {
            if (playing) playing.Stop();
        }

        public void Play(float volumeMultiplier = 1)
        {
            playing = sounds.Play(transform.position, volumeMultiplier);
            if (useLowPassFilter)
            {
                var lpf = playing.gameObject.AddComponent<AudioLowPassFilter>();
                lpf.cutoffFrequency = lpFilterFrequency;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.7f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, sounds.mainRadius);
            Gizmos.color = new Color(1f, 0.7f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, sounds.falloffRadius);
        }
    }
}