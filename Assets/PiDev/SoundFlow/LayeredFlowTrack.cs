using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * Licensed under MIT
 *
 * ============= Description =============
 * A track that mixes multiple audio stems (layers) together, with per-layer volume/pan control.
 * 
 * ============= Usage =============
 * Add to a GameObject and configure layers.
 * Call SetLayerWeight(name/ID, weight) or SetLayerWeight(name/ID, left, right) to control layer volume/pan.
 */

namespace PiDev.SoundFlow
{
    [Serializable]
    public class LayeredFlowTrack : FlowTrackBase
    {
        public float dspDelay = 0.5f;

        public enum MixingMode { ManualVolume, Weighted }
        public MixingMode mixingMode = MixingMode.ManualVolume;

        [Serializable]
        public class MixFade
        {
            public float inTime = 1f;
            public float outTime = 1f;
        }

        public MixFade mixFade = new();

        [Serializable]
        public class Stem
        {
            public string name;

            // Audio
            public AudioClip clip;
            public float volumeScale = 1f;

            // Mono weighting
            public float weight = 0f;

            // Stereo weighting (per-layer)
            public bool stereoMode = false;
            public float leftWeight = 0f;
            public float rightWeight = 0f;

            [NonSerialized] public AudioSource source;

            // Ramping state
            [NonSerialized] public float currentVolume = 0f;
            [NonSerialized] public float targetVolume = 0f;
            [NonSerialized] public float currentPan = 0f;   // -1..+1
            [NonSerialized] public float targetPan = 0f;
        }

        [FormerlySerializedAs("layers")]
        public List<Stem> stems = new List<Stem>();

        bool stopRequested = false;

        // Convenience lookups & setters

        public Stem GetByName(string name) => stems.FirstOrDefault(layer => layer.name == name);

        public void SetLayerWeight(string name, float weight)
        {
            var l = GetByName(name);
            if (l == null) return;
            l.stereoMode = false;
            l.weight = weight;
        }
        public void SetLayerWeight(int id, float weight)
        {
            if (id < 0 || id >= stems.Count) return;
            stems[id].stereoMode = false;
            stems[id].weight = weight;
        }

        public void SetLayerWeight(string name, float left, float right)
        {
            var l = GetByName(name);
            if (l == null) return;
            l.stereoMode = true;
            l.leftWeight = left;
            l.rightWeight = right;
        }
        public void SetLayerWeight(int id, float left, float right)
        {
            if (id < 0 || id >= stems.Count) return;
            var l = stems[id];
            l.stereoMode = true;
            l.leftWeight = left;
            l.rightWeight = right;
        }

        // Lifecycle

        // Called by the engine when starting/playing (create or update hint)
        public override void OnPlay(SoundFlowPlayer engine)
        {
            stopRequested = false;

            double dspTime = double.IsNaN(state.scheduledDsp)
                ? AudioSettings.dspTime + dspDelay
                : state.scheduledDsp;

            foreach (var layer in stems)
            {
                if (layer == null) continue;

                if (layer.clip != null)
                {
                    var src = layer.source;
                    if (src == null)
                    {
                        src = gameObject.AddComponent<AudioSource>();
                        src.playOnAwake = false;
                        src.spatialBlend = 0f; // 2D so panStereo works
                        src.loop = true;
                        layer.source = src;
                        TryApplyMixerGroup(src);
                    }

                    if (src.clip != layer.clip)
                    {
                        if (src.isPlaying) src.Stop();
                        src.clip = layer.clip;
                        src.timeSamples = 0;
                    }

                    // start quiet; mixer ramps
                    src.volume = 0f;
                    src.panStereo = layer.currentPan = 0f;
                    layer.currentVolume = 0f;
                    layer.targetVolume = 0f;
                    layer.targetPan = 0f;

                    if (!src.isPlaying) src.PlayScheduled(dspTime);
                }
                else
                {
                    if (layer.source != null && layer.source.isPlaying)
                        layer.source.Stop();

                    layer.currentVolume = 0f;
                    layer.targetVolume = 0f;
                    layer.currentPan = 0f;
                    layer.targetPan = 0f;
                }
            }
        }

        // Called when user/engine requests stop: ramp to 0 (no cleanup)
        public override void OnStop(SoundFlowPlayer engine)
        {
            stopRequested = true; // UpdateFlowTrack will ramp volumes down via mixFade.outTime
        }

        // Called when it is safe to unregister/dispose
        public override void OnCleanup(SoundFlowPlayer engine)
        {
            foreach (var layer in stems)
            {
                if (layer == null) continue;

                if (layer.source != null)
                {
                    if (layer.source.isPlaying) layer.source.Stop();
                    UnityEngine.Object.Destroy(layer.source);
                    layer.source = null;
                }

                layer.currentVolume = 0f;
                layer.targetVolume = 0f;
                layer.currentPan = 0f;
                layer.targetPan = 0f;
            }
        }

        // Engine calls this every frame to compute & apply volumes
        public override void UpdateFlowTrack(SoundFlowPlayer engine)
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) dt = 0.0001f;

            // Track envelope from engine; stopRequested forces zero target
            float trackScalar = stopRequested ? 0f : Mathf.Clamp01(state.currentVolume);

            // --- PAN TARGET ---
            // If stereoMode is on, pan from left/right; else center.
            foreach (var l in stems)
            {
                if (l == null) continue;
                l.targetPan = l.stereoMode ? ComputePan(l.leftWeight, l.rightWeight) : 0f;
            }

            // --- PER-CHANNEL SUMS FOR WEIGHTED MIXING ---
            float leftSum = 0f, rightSum = 0f;
            if (mixingMode == MixingMode.Weighted)
            {
                foreach (var l in stems)
                {
                    if (l == null || l.source == null || l.clip == null) continue;
                    ComputeChannelComponents(l, out float cL, out float cR);
                    leftSum += cL;
                    rightSum += cR;
                }
            }

            foreach (var l in stems)
            {
                if (l == null || l.source == null || l.clip == null) continue;

                // --- TARGET VOLUME (pan-aware) ---
                switch (mixingMode)
                {
                    case MixingMode.ManualVolume:
                        {
                            // Manual: no normalization; magnitude per layer
                            // Stereo -> magnitude = L+R ; Mono -> magnitude = weight
                            float magnitude;
                            if (l.stereoMode)
                                magnitude = Mathf.Max(0f, l.leftWeight) + Mathf.Max(0f, l.rightWeight);
                            else
                                magnitude = Mathf.Max(0f, l.weight);

                            l.targetVolume = l.volumeScale * trackScalar * magnitude;
                            break;
                        }

                    case MixingMode.Weighted:
                        {
                            // Weighted: normalize per channel, then derive a layer's share from its left/right shares.
                            ComputeChannelComponents(l, out float cL, out float cR);
                            float shareL = (leftSum > 0f) ? cL / leftSum : 0f;
                            float shareR = (rightSum > 0f) ? cR / rightSum : 0f;
                            // Use the dominant share so a fully left (or right) layer can reach full volume.
                            float normalizedShare = Mathf.Max(shareL, shareR);
                            l.targetVolume = l.volumeScale * trackScalar * normalizedShare;
                            break;
                        }
                }

                // --- RAMP VOLUME & PAN ---
                float fadeTime = (l.targetVolume > l.currentVolume)
                    ? (mixFade.inTime > 0f ? mixFade.inTime : 0.0001f)
                    : (mixFade.outTime > 0f ? mixFade.outTime : 0.0001f);
                float step = dt / Mathf.Max(0.0001f, fadeTime);

                l.currentVolume = MoveToward(l.currentVolume, l.targetVolume, step);
                l.currentPan = MoveToward(l.currentPan, l.targetPan, step);

                // Apply
                l.source.volume = Mathf.Clamp01(l.currentVolume);
                l.source.panStereo = Mathf.Clamp(l.currentPan, -1f, 1f);
            }
        }

        // Helpers

        // Returns each layer's contribution to Left/Right channels (non-negative).
        // Stereo mode uses (leftWeight, rightWeight).
        // Mono mode splits by pan (-1..+1): leftRatio=(1-p)/2, rightRatio=(1+p)/2.
        private static void ComputeChannelComponents(Stem l, out float cL, out float cR)
        {
            if (l.stereoMode)
            {
                cL = Mathf.Max(0f, l.leftWeight);
                cR = Mathf.Max(0f, l.rightWeight);
            }
            else
            {
                float p = Mathf.Clamp(l.targetPan, -1f, 1f);
                float leftRatio = (1f - p) * 0.5f;   // p=-1 -> 1, p=0 -> 0.5, p=+1 -> 0
                float rightRatio = 1f - leftRatio;   // p=-1 -> 0, p=0 -> 0.5, p=+1 -> 1
                float w = Mathf.Max(0f, l.weight);
                cL = w * leftRatio;
                cR = w * rightRatio;
            }
        }

        public static float MixMagnitude(Stem l)
        {
            if (l.stereoMode) return Mathf.Max(0f, l.leftWeight) + Mathf.Max(0f, l.rightWeight);
            return Mathf.Max(0f, l.weight);
        }

        public static float MoveToward(float current, float target, float maxDelta)
        {
            if (current < target)
            {
                current += maxDelta;
                return (current > target) ? target : current;
            }
            else if (current > target)
            {
                current -= maxDelta;
                return (current < target) ? target : current;
            }
            return target;
        }

        public static float ComputePan(float left, float right)
        {
            float L = Mathf.Max(0f, left);
            float R = Mathf.Max(0f, right);
            float sum = L + R;
            if (sum <= 0f) return 0f;
            // map balance to [-1..+1]: more right => positive, more left => negative
            float pan = (R - L) / sum;
            return Mathf.Clamp(pan, -1f, 1f);
        }

        // Debug
        public bool debugExtra = false;

        public override string ToString()
        {
            string clipName = "<no clips>";
            float maxVol = float.MinValue;

            foreach (var layer in stems)
            {
                if (layer?.clip == null) continue;
                if (layer.currentVolume > maxVol)
                {
                    maxVol = layer.currentVolume;
                    clipName = layer.clip.name;
                }
            }

            string result = $"Layered '{trackName}' [{mixingMode}] {clipName}";

            if (debugExtra && stems != null && stems.Count > 0)
            {
                foreach (var layer in stems)
                {
                    if (layer == null) continue;
                    string cName = layer.clip != null ? layer.clip.name : "<no clip>";
                    string mode = layer.stereoMode ?
                        $"Stereo L:{layer.leftWeight:0.00} R:{layer.rightWeight:0.00} Pan:{layer.currentPan:0.00}" :
                        $"Mono W:{layer.weight:0.00}";
                    result += $"\n  => '{layer.name}' {cName} Vol:{layer.currentVolume:0.00}/{layer.targetVolume:0.00} {mode}";
                }
            }

            return result;
        }

    }
}
