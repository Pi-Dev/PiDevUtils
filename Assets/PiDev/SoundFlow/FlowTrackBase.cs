using System;
using UnityEngine;
using UnityEngine.Audio;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * Licensed under MIT
 *
 * ============= Description =============
 * The base class for all SoundFlow tracks.
 * It provides common settings, state management, and lifecycle methods.
 * 
 * ============= Usage =============
 * All SoundFlow track types share this base class.
 * 
 * Settings:
 *  - mute: Mutes the track.
 *  
 *  - priority: The track's priority for playback.
 *    Tracks are mixed based on priority (higher first).
 *    All tracks that share same highest priority are mixed together and all others are silent.
 *    
 *  - weight: The track's weight for mixing in its priority group.
 *  
 *  - manualVolumeControl: If true, the track's volume is controlled manually via targetVolume.
 *  
 *  - exclusiveMode: If true, SoundFlowPlayer will not control this track's volume and state.
 *    By using this mode, your scripts are fully responsible for track's state and lifecycle.
 *    
 * State:
 *  - currentVolume: The current volume of the track (0.0 to 1.0).
 *  - activeFadeTime: The time for the current fade operation.
 *  - removeIfSilent: If true, the track will be removed when its volume reaches 0.
 *  - hasError: If true, the track has encountered an error and will be trated as if in exclusive mode.
 *  - isPlaying: If true, the track is currently playing.
 *  - scheduledDsp: The DSP time when the track is scheduled to start.
 *  - rampTarget: The target volume for ramping when not in manual volume control mode.
 *  
 * Fading: Settings for fade in/out times.
 * 
 * To derive a custom track type, inherit from this class and implement:
 * UpdateFlowTrack(SoundFlowPlayer engine) - called every frame to update the track's state.
 * OnPlay(SoundFlowPlayer engine) - called when the track starts playing.
 * OnStop(SoundFlowPlayer engine) - called when the track stops playing.
 * OnCleanup(SoundFlowPlayer engine) - called when the track is removed from the player.
 * OnErrored(Exception e) - called when the track encounters an error.
 */

namespace PiDev.SoundFlow
{
    public abstract class FlowTrackBase : MonoBehaviour
    {
        public SoundFlowPlayer player;
        public string trackName;
        public AudioMixerGroup mixerGroup;

        [Serializable]
        public class Settings
        {
            public bool mute = false;
            public int priority = 0;
            public float weight = 1f;
            public bool manualVolumeControl = false;
            public float targetVolume = 1;
            public bool exclusiveMode = false;
        }

        [Serializable]
        public class Fading
        {
            public float startTime = 1f;
            public float stopTime = 1f;
            public float pauseTime = 0.5f;
            public float resumeTime = 0.5f;
            public void Set(float start = float.NaN, float stop = float.NaN, float pause = float.NaN, float resume = float.NaN)
            {
                if (!float.IsNaN(start)) startTime = start;
                if (!float.IsNaN(stop)) stopTime = stop;
                if (!float.IsNaN(pause)) pauseTime = pause;
                if (!float.IsNaN(resume)) resumeTime = resume;
            }
        }

        [Serializable]
        public class State
        {
            public float currentVolume = 0;
            public float activeFadeTime = 1;
            public bool removeIfSilent = false;
            public bool hasError = false;
            public bool isPlaying = false;
            public double scheduledDsp = float.NaN;
            public float rampTarget = 0;
        }

        public Fading fading = new();
        public State state = new();
        public Settings settings = new();

        public bool playOnStart = true;

        protected virtual void Start()
        {
            if (playOnStart && player != null) player.Play(this, settings.weight);
        }

        // Resolve: track > player > null
        protected AudioMixerGroup ResolveMixerGroup()
        {
            if (mixerGroup != null) return mixerGroup;
            if (player != null) return player.mixerGroup;
            return null;
        }
        protected void TryApplyMixerGroup(AudioSource src)
        {
            var grp = ResolveMixerGroup();
            if (grp != null && src != null) src.outputAudioMixerGroup = grp;
        }

        public virtual void OnPlay(SoundFlowPlayer engine) { }
        public virtual void OnStop(SoundFlowPlayer engine) { }
        public virtual void OnCleanup(SoundFlowPlayer engine) { }
        public virtual void OnErrored(Exception e) { }

        public abstract void UpdateFlowTrack(SoundFlowPlayer engine);
    }
}
