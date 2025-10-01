using System;
using System.Collections.Generic;
using UnityEngine;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * Licensed under MIT
 *
 * ============= Description =============
 * A track that plays a sequence of audio clips one after another.
 * 
 * ============= Usage =============
 * Add to a GameObject with and configure the clips to play in sequence.
 * This track type is still incomplete, it's intended to provide the Spelunky 2 style of audio sequencing.
 */

namespace PiDev.SoundFlow
{
    [Serializable]
    public class SequenceFlowTrack : FlowTrackBase
    {
        [Serializable]
        public class SequenceClip
        {
            public AudioClip clip;
            public float delayAfter = 0f; // Delay after this clip before the next one
        }

        [Header("Sequence Settings")]
        public List<SequenceClip> clips = new List<SequenceClip>();
        public bool loopSequence = false;
        private int _currentIndex = 0;
        private double _nextStartDsp = 0.0;
        private AudioSource src;

        public override void OnPlay(SoundFlowPlayer engine)
        {
            if (src == null)
            {
                src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false; src.spatialBlend = 0f; src.loop = false;
                TryApplyMixerGroup(src);
            }

            _currentIndex = 0;
            _nextStartDsp = double.IsNaN(state.scheduledDsp) ? AudioSettings.dspTime : state.scheduledDsp;
            ScheduleNextClip();
        }

        public override void OnStop(SoundFlowPlayer engine)
        {
            if (src != null) src.Stop();
        }

        public override void UpdateFlowTrack(SoundFlowPlayer engine)
        {
            if (src == null) return;
            src.volume = Mathf.Clamp01(state.currentVolume);

            if (AudioSettings.dspTime >= _nextStartDsp - 0.05) // Slight tolerance before scheduling next
                ScheduleNextClip();
        }

        private void ScheduleNextClip()
        {
            if (clips.Count == 0) return;
            var seqClip = clips[_currentIndex];
            if (seqClip.clip == null) return;

            src.clip = seqClip.clip;
            src.PlayScheduled(_nextStartDsp);
            _nextStartDsp += seqClip.clip.length + seqClip.delayAfter;

            _currentIndex++;
            if (_currentIndex >= clips.Count)
            {
                if (loopSequence) _currentIndex = 0;
                else _currentIndex = clips.Count - 1; // Stay on last
            }
        }

        public override string ToString()
        {
            string clipName = (clips != null && clips.Count > 0 && clips[_currentIndex].clip != null) ? clips[_currentIndex].clip.name : "<no clips>";
            return $"SequenceFlowTrack [{clipName}]";
        }
    }
}
