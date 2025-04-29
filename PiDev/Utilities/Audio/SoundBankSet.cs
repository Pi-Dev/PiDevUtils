using System;
using UnityEngine;
using UnityEngine.Audio;

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
 * Configurable sound bank utility for playing random spatial and 2D audio clips 
 * with randomized pitch and playback logic.
 * Supports single-play and looping sounds with spatial blend, falloff settings, 
 * and optional clip shuffling.
 * Ideal for sound effects, ambient audio systems, or dynamic audio behavior in Unity.
 *
 * ============= Usage =============
 * 
 * [SerializeField] SoundBankSet soundBank;
 * soundBank.Play(position);
 * soundBank.Play2D();
 * soundBank.PlayLooping(position);
 * soundBank.PlayLooping(followTransform);
 */

namespace PiDev.Utilities
{

    [Serializable]
    public class SoundBankSet
    {
        public AudioClip[] sounds;
        public AudioMixerGroup mixerGroup;
        [Range(0, 1)] public float volume = 1;
        [Tooltip("0 = 2D, 1 = 3D")]
        [Range(0, 1)] public float spatialBlend = 1;
        public float mainRadius = 20;
        public float falloffRadius = 40;
        public float pitchRandomize = 0.3f;
        public bool shuffle = false;
        public bool ensureDifferent = false;

        int cnt;
        int last;

        public AudioClip Get(int next = -1, int index = -1)
        {
            if (index != -1) return sounds[index % sounds.Length];
            if (sounds.Length == 0) return null;
            if (sounds.Length == 1) return sounds[0];
            if (next == -1) cnt++; else cnt = next;
            if (!shuffle) return sounds[cnt % sounds.Length];
            last = cnt;
            if (ensureDifferent)
            {
                int n;
                do n = UnityEngine.Random.Range(0, sounds.Length); while (n == last);
                last = n;
                return sounds[n];
            }
            return sounds[UnityEngine.Random.Range(0, sounds.Length)];
        }

        public AudioSource Play(Vector3 position, float volumeScale = 1, float overrideSpatial = float.NaN, float delay = 0)
        {
            return PlaySFXAtPoint(Get(), volume * volumeScale, position, null, float.IsNaN(overrideSpatial) ? spatialBlend : overrideSpatial, mainRadius, falloffRadius, pitchRandomize, delay);
        }

        public AudioSource PlayLooping(Vector3 position, float volumeScale = 1)
        {
            return LoopSFXAtPoint(Get(), volume * volumeScale, position, null, spatialBlend, mainRadius, falloffRadius, pitchRandomize);
        }

        public AudioSource PlayLooping(Transform followTarget, float volumeScale = 1)
        {
            var sfx = LoopSFXAtPoint(Get(), volume * volumeScale, followTarget.position, null, spatialBlend, mainRadius, falloffRadius, pitchRandomize);
            var ft = sfx.gameObject.AddComponent<FollowTarget>();
            ft.target = followTarget;
            ft.offset = Vector3.zero;
            return sfx;
        }

        public AudioSource Play2D(float volumeScale = 1, bool usePitchRandimization = false, float delay = 0, int index = -1)
        {
            return PlaySFXAtPoint(Get(-1, index), volume * volumeScale, Vector3.zero, null, 0, 10000, 10000, usePitchRandimization ? pitchRandomize : 0, delay);
        }

        public static AudioSource PlaySFXAtPoint(AudioClip clip, float volume, Vector3 position, AudioMixerGroup group, float spatial = 1, float mainRadius = 5, float falloffRadius = 10, float pitchRandomize = 0, float delay = 0)
        {
            if (!Application.isPlaying) return null;
            if (clip == null) return null;
            var go = new GameObject("SFX: " + clip.name);
            go.hideFlags = HideFlags.DontSave;
            go.transform.position = position;
            var s = go.AddComponent<AudioSource>();
            s.clip = clip;
            s.volume = volume;
            s.outputAudioMixerGroup = group;
            s.spatialBlend = spatial;
            s.rolloffMode = AudioRolloffMode.Linear;
            s.minDistance = mainRadius;
            s.maxDistance = falloffRadius;
            s.pitch = 1 + UnityEngine.Random.Range(-pitchRandomize, +pitchRandomize);
            s.PlayDelayed(delay);
            UnityEngine.Object.Destroy(go, delay + clip.length + 0.2f);
            return s;
        }

        public static AudioSource LoopSFXAtPoint(AudioClip clip, float volume, Vector3 position, AudioMixerGroup group = null, float spatial = 1, float mainRadius = 5, float falloffRadius = 10, float pitchRandomize = 0)
        {
            if (clip == null) return null;
            var go = new GameObject("SFX-LOOP: " + clip.name);
            go.hideFlags = HideFlags.DontSave;
            go.transform.position = position;
            var s = go.AddComponent<AudioSource>();
            s.clip = clip;
            s.loop = true;
            s.volume = volume;
            s.outputAudioMixerGroup = group;
            s.spatialBlend = spatial;
            s.rolloffMode = AudioRolloffMode.Linear;
            s.minDistance = mainRadius;
            s.maxDistance = falloffRadius;
            s.pitch = 1 + UnityEngine.Random.Range(-pitchRandomize, +pitchRandomize);
            s.Play();
            return s;
        }
    }
}