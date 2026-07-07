using PiDev.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * Licensed under MIT
 *
 * ============= Description =============
 * Modern, explicit audio runtime. Global weight/duck/mute logic with per-track.
 * This is the main SoundFlow player component. It manages and updates all registered Sound Flow tracks.
 * Mixing is done based on track priority and weight, with support for muting and ducking.
 * 
 * ============= Usage =============
 * Add to a Manager GameObject or let the API create it automatically.
 *
 * Play(AudioClip clip, ...) - Creates and plays a new BasicFlowTrack with the specified clip and settings.
 * Play(FlowTrackBase track, ...) - Plays an existing track, registering it if not already registered.
 * Stop(FlowTrackBase track, ...) - Stops the specified track, with optional fade out and unload.
 * StopWithName(string name, ...) - Stops all tracks with the specified name.
 * FindWithName(string name) - Finds all tracks with the specified name.
 */

namespace PiDev.SoundFlow
{
    /// <summary>
    /// SoundFlow: modern, explicit audio runtime. Global weight/duck/mute logic with per-track UpdateFlowTrack control.
    /// This player can be used as a Singleton, but does not lazy-create itself. Place it in the scene.
    /// </summary>
    public class SoundFlowPlayer : Singleton<SoundFlowPlayer>
    {
        public AudioMixerGroup mixerGroup;

        [SerializeField] public List<FlowTrackBase> _tracks = new List<FlowTrackBase>(64);
        private readonly List<FlowTrackBase> _tmp = new List<FlowTrackBase>(64);

        void Register(FlowTrackBase track)
        {
            if (track == null) return;
            if (track.state.hasError) return; // ignore errored tracks on register
            if (_tracks.Contains(track)) return;
            track.player = this;
            track.state.isPlaying = false; //revert state / stop
            track.state.removeIfSilent = false;
            _tracks.Add(track);
        }

        public BasicFlowTrack Play(AudioClip clip, float weight, float volumeScale, string name = null, double? dspTime = null, float startOffset = 0f, bool loop = true, float pitch = 1f, AudioMixerGroup group = null, FlowTrackBase.Fading fading = null)
        {
            if (clip == null) return null;

            // Try to find an equivalent non-errored BasicFlowTrack
            for (int i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i] is BasicFlowTrack b && !b.state.hasError)
                {
                    if (b.clip == clip && b.trackName == name)
                    {
                        b.state.removeIfSilent = false;
                        b.state.isPlaying = true;
                        b.settings.weight = weight;
                        b.startOffset = startOffset;
                        b.loop = loop;
                        b.pitch = pitch;
                        b.state.scheduledDsp = dspTime ?? float.NaN;
                        //b.state.currentVolume = 0f; // will ramp in
                        b.settings.targetVolume = 0f;
                        b.volumeScale = volumeScale;
                        b.settings.weight = weight;
                        b.mixerGroup = group;
                        if (fading != null) b.fading = fading;
                        SafeInvoke(() => b.OnPlay(this), b);
                        return b;
                    }
                }
            }

            // Create GameObject + component
            var go = new GameObject($"[Basic '{name}' {clip.name}]");
            go.transform.SetParent(transform, false);
            var track = go.AddComponent<BasicFlowTrack>();
            track.trackName = name;
            track.clip = clip;
            track.startOffset = startOffset;
            track.loop = loop;
            track.pitch = pitch;
            track.state.scheduledDsp = dspTime ?? float.NaN;
            track.state.currentVolume = 0f; // will ramp in
            track.state.removeIfSilent = false;
            track.settings.targetVolume = 0f;
            track.volumeScale = volumeScale;
            track.settings.weight = weight;
            track.mixerGroup = group;
            if (fading != null) track.fading = fading;
            Register(track);
            track.state.isPlaying = true;
            SafeInvoke(() => track.OnPlay(this), track);
            return track;
        }

        public FlowTrackBase Play(FlowTrackBase track, float weight, double? dspTime = null)
        {
            if (track == null) return null;
            if (track.state.hasError) return null; // ignore errored references
            if (!_tracks.Contains(track)) Register(track);
            track.state.scheduledDsp = dspTime ?? float.NaN;
            track.state.isPlaying = true;
            track.state.removeIfSilent = false;
            track.settings.weight = weight;
            SafeInvoke(() => track.OnPlay(this), track);
            return track;
        }

        public FlowTrackBase[] FindWithName(string name)
        {
            _tmp.Clear();
            for (int i = 0; i < _tracks.Count; i++)
            {
                var t = _tracks[i];
                if (t != null && t.trackName == name)
                    _tmp.Add(t);
            }
            return _tmp.ToArray();
        }

        public void StopWithName(string name, float fadeTime = 0f, bool unload = true)
        {
            foreach (var track in _tracks)
            {
                if (track == null || track.trackName != name) continue;
                SafeInvoke(() => track.OnStop(this), track);
                StopFadeToZero(track, fadeTime, unload);
            }
        }

        public void StopAll(float fadeTime = 0f, bool unload = true)
        {
            foreach (var track in _tracks)
            {
                if (track == null) continue;
                SafeInvoke(() => track.OnStop(this), track);
                StopFadeToZero(track, fadeTime <= 0 ? track.fading.stopTime : fadeTime, unload: unload);
            }
        }

        public void Stop(FlowTrackBase track, float fadeTime = float.NaN, bool unload = true)
        {
            if (track == null) return;
            if (!_tracks.Contains(track)) return;

            SafeInvoke(() => track.OnStop(this), track);

            float finalFade = float.IsNaN(fadeTime) ? track.fading.stopTime : fadeTime;
            if (finalFade <= 0f) finalFade = track.fading.startTime > 0 ? track.fading.startTime : 0.0001f;
            StopFadeToZero(track, finalFade, unload);
        }

        public void StopFadeToZero(FlowTrackBase track, float fadeTime, bool unload)
        {
            if (track == null) return;
            track.settings.exclusiveMode = false;
            track.settings.manualVolumeControl = false;
            track.state.removeIfSilent = unload;
            track.state.scheduledDsp = double.PositiveInfinity;
            track.state.activeFadeTime = float.IsNaN(fadeTime) ? track.fading.stopTime : fadeTime;
            track.settings.targetVolume = 0f;
            track.settings.weight = 0f;
        }

        void Update()
        {
            double now = AudioSettings.dspTime;
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) dt = 0.0001f;

            // 1) Start due schedules
            foreach (var t in _tracks)
            {
                if (t == null || t.state.hasError) continue;
                if (t.state.isPlaying) continue;

                if (double.IsNaN(t.state.scheduledDsp) || t.state.scheduledDsp <= now)
                {
                    t.state.activeFadeTime = t.fading.startTime;
                    SafeInvoke(() => t.OnPlay(this), t);
                }
            }

            // Build candidate set (not Exclusive, not Errored, playing)
            _tmp.Clear();
            foreach (var t in _tracks)
            {
                if (t == null || t.state.hasError) continue;
                if (t.settings.exclusiveMode) continue;
                if (!t.state.isPlaying) continue;
                _tmp.Add(t);
            }

            // 2) Priority scan (ignore muted: muted tracks don't exist for priority)
            int maxPriority = int.MinValue;
            bool hasUnmuted = false;
            foreach (var t in _tmp)
            {
                if (t.settings.mute) continue;
                hasUnmuted = true;
                if (t.settings.priority > maxPriority) maxPriority = t.settings.priority;
            }
            if (!hasUnmuted) maxPriority = int.MinValue; // all muted -> no audible group

            // 3) Weight sum for the audible group only: unmuted + top-priority
            float audibleWeightSum = 0f;
            foreach (var t in _tmp)
            {
                if (t.settings.mute) continue;
                if (t.settings.priority != maxPriority) continue;
                audibleWeightSum += Mathf.Max(0f, t.settings.weight);
            }

            // 4) Target assignment and ducking
            foreach (var t in _tmp)
            {
                float localTarget = t.settings.targetVolume;

                // Mute always fades toward 0
                if (t.settings.mute)
                {
                    t.state.activeFadeTime = t.fading.pauseTime;
                    t.state.rampTarget = 0f;
                    continue;
                }

                // Lower than top priority? duck to 0
                if (hasUnmuted && t.settings.priority < maxPriority)
                {
                    t.state.activeFadeTime = t.fading.pauseTime;
                    t.state.rampTarget = 0f;
                    continue;
                }

                // Top-priority & unmuted: distribute by audible group weight
                if (!t.settings.manualVolumeControl)
                {
                    if (audibleWeightSum > 0f)
                        localTarget = Mathf.Max(0f, t.settings.weight) / audibleWeightSum;
                    else
                        localTarget = 0f; // no weights -> silence (explicit)
                }
                //t.state.activeFadeTime = t.fading.resumeTime; // 
                t.state.rampTarget = localTarget;
            }

            // 5) Ramp currentVolume toward calculated target
            foreach (var t in _tracks)
            {
                if (t == null || t.state.hasError) continue;
                if (!t.state.isPlaying) continue;

                float fadeTime = (t.state.activeFadeTime > 0f) ? t.state.activeFadeTime : (t.fading.startTime > 0 ? t.fading.startTime : 0.0001f);
                float step = dt / Mathf.Max(0.0001f, fadeTime);
                t.state.currentVolume = MoveToward(t.state.currentVolume, t.state.rampTarget, step);
            }

            // 6) Track update
            foreach (var t in _tracks)
            {
                if (t == null || t.state.hasError) continue;
                if (!t.state.isPlaying) continue;
                if (t.settings.exclusiveMode) continue;
                SafeInvoke(() => t.UpdateFlowTrack(this), t);
            }

            // 7) Cleanup silent (respect ManualVolume rule)
            for (int i = _tracks.Count - 1; i >= 0; i--)
            {
                var t = _tracks[i];
                if (t == null) { _tracks.RemoveAt(i); continue; }
                if (t.settings.manualVolumeControl) continue;

                if (t.state.removeIfSilent && t.state.isPlaying && Mathf.Approximately(t.state.currentVolume, 0f))
                {
                    // If you've refactored to call OnStop on user Stop(), do NOT call it here.
                    t.state.isPlaying = false;
                    _tracks.RemoveAt(i);
                    SafeInvoke(() => t.OnCleanup(this), t);
                    if (t.transform != null && t.transform.IsChildOf(transform))
                        Destroy(t.gameObject);
                }
            }
        }

        static float MoveToward(float current, float target, float maxDelta)
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

        static void SafeInvoke(Action call, FlowTrackBase track)
        {
            try { call?.Invoke(); }
            catch (Exception e)
            {
                if (track != null)
                {
                    track.state.hasError = true;
                    try { track.OnErrored(e); } catch { Debug.LogException(e); }
                }
#if UNITY_EDITOR
                Debug.LogException(e);
#endif
            }
        }
    }

}
