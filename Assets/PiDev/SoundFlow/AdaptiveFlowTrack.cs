using System;
using System.Collections.Generic;
using UnityEngine;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * Licensed under MIT
 *
 * ============= Description =============
 * A track that plays multiple audio stems and adjusts their volumes based on an intensity parameter.
 * It allows for dynamic mixing of different audio layers to create a more immersive sound experience.
 * An intensity value controls which stems are audible and at what volume.
 * The track can also control its own weight, priority, and target volume based on intensity curves.
 * 
 * ============= Usage =============
 * Add to a GameObject and configure the audio stems and intensity curves.
 * Call SetIntensity(float) to adjust the mix dynamically.
 */

namespace PiDev.SoundFlow
{
    [Serializable]
    public class AdaptiveFlowTrack : FlowTrackBase
    {
        [Serializable]
        public class Stem
        {
            public AudioClip clip;
            public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 0, 1, 1);
            [NonSerialized] public AudioSource source;
            [NonSerialized] public float currentVolume = 0f;
            [NonSerialized] public float targetVolume = 0f;
        }

        [Serializable]
        public class MixFade
        {
            public float inTime = 0.25f;
            public float outTime = 0.35f;
        }

        public List<Stem> stems = new List<Stem>();
        public float intensity = 0f;
        public bool setOwnTargetVolume = false;
        public bool setOwnWeight = false;
        public bool setOwnPriority = false;
        public AnimationCurve targetVolumeCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public AnimationCurve weightCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public AnimationCurve priorityCurve = AnimationCurve.Linear(0, 0, 1, 0);
        public MixFade mixFade = new();

        bool stopRequested = false;

        public override void OnPlay(SoundFlowPlayer engine)
        {
            stopRequested = false;
            if (setOwnTargetVolume)
                settings.manualVolumeControl = true;

            double dspTime = double.IsNaN(state.scheduledDsp) ? AudioSettings.dspTime + 0.5f : state.scheduledDsp;

            // Reset per-stem state and prepare sources
            var keep = new HashSet<AudioSource>();
            foreach (var stem in stems)
            {
                if (stem == null) continue;

                stem.currentVolume = 0f;
                stem.targetVolume = 0f;

                if (stem.clip == null)
                {
                    if (stem.source != null) Destroy(stem.source);
                    stem.source = null;
                    continue;
                }

                if (stem.clip.loadState == AudioDataLoadState.Unloaded)
                    stem.clip.LoadAudioData();
                if (stem.clip.loadState != AudioDataLoadState.Loaded) continue;

                var src = stem.source;
                if (src == null)
                {
                    src = gameObject.AddComponent<AudioSource>();
                    src.playOnAwake = false;
                    src.spatialBlend = 0f;
                    src.loop = true;
                    stem.source = src;
                    TryApplyMixerGroup(src);
                }

                if (src.clip != stem.clip)
                {
                    if (src.isPlaying) src.Stop();
                    src.clip = stem.clip;
                    src.timeSamples = 0;
                    src.volume = 0f;
                }

                if (!src.isPlaying) src.PlayScheduled(dspTime);
                keep.Add(src);
            }

            var all = GetComponents<AudioSource>();
            foreach (var a in all)
            {
                if (a == null || keep.Contains(a)) continue;
                if (a.isPlaying) a.Stop();
                Destroy(a);
            }
        }

        public override void OnStop(SoundFlowPlayer engine)
        {
            stopRequested = true;
        }

        public override void UpdateFlowTrack(SoundFlowPlayer engine)
        {
            if (!stopRequested)
            {
                if (setOwnWeight && weightCurve != null)
                    settings.weight = weightCurve.Evaluate(intensity);
                if (setOwnTargetVolume && targetVolumeCurve != null)
                    settings.targetVolume = targetVolumeCurve.Evaluate(intensity);
                if (setOwnPriority && priorityCurve != null)
                    settings.priority = Mathf.RoundToInt(priorityCurve.Evaluate(intensity));
            }
            else
            {
                if (setOwnWeight) settings.weight = 0f;
                if (setOwnPriority) settings.priority = int.MinValue;
            }

            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) dt = 0.0001f;

            foreach (var stem in stems)
            {
                if (stem == null || stem.source == null || stem.clip == null) continue;

                float intensityFactor = stem.intensityCurve != null ? stem.intensityCurve.Evaluate(intensity) : 1f;
                stem.targetVolume = Mathf.Clamp01(intensityFactor) * Mathf.Clamp01(state.currentVolume);

                float fadeTime = stem.targetVolume > stem.currentVolume
                    ? (mixFade.inTime > 0f ? mixFade.inTime : 0.0001f)
                    : (mixFade.outTime > 0f ? mixFade.outTime : 0.0001f);
                float step = dt / Mathf.Max(0.0001f, fadeTime);

                if (stem.currentVolume < stem.targetVolume)
                {
                    stem.currentVolume += step;
                    if (stem.currentVolume > stem.targetVolume)
                        stem.currentVolume = stem.targetVolume;
                }
                else if (stem.currentVolume > stem.targetVolume)
                {
                    stem.currentVolume -= step;
                    if (stem.currentVolume < stem.targetVolume)
                        stem.currentVolume = stem.targetVolume;
                }

                stem.source.volume = Mathf.Clamp01(stem.currentVolume * state.currentVolume);
            }
        }

        public override void OnCleanup(SoundFlowPlayer engine)
        {
            foreach (var s in stems) s.source = null;
            foreach (var s in GetComponents<AudioSource>()) Destroy(s);
        }

        public override string ToString()
        {
            string clipName = "<no clips>";

            if (stems != null && stems.Count > 0)
            {
                float maxVol = float.MinValue;
                AudioClip loudestClip = null;

                foreach (var stem in stems)
                {
                    if (stem?.clip == null) continue;
                    if (stem.currentVolume > maxVol)
                    {
                        maxVol = stem.currentVolume;
                        loudestClip = stem.clip;
                    }
                }
                if (loudestClip != null)
                    clipName = loudestClip.name;
            }
            return $"Adaptive '{trackName}' I={intensity} [{clipName}]";
        }

    }
}
