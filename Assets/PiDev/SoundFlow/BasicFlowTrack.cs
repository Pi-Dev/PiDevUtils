using UnityEngine;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * Licensed under MIT
 *
 * ============= Description =============
 * Basic SoundFlow track that plays a single AudioClip.
 * 
 * ============= Usage =============
 * Add to a GameObject with and configure the AudioClip to play.
 * This track type can also be created via SoundFlowPlayer's Play(clip, ...) method.
 */

namespace PiDev.SoundFlow
{
    public class BasicFlowTrack : FlowTrackBase
    {
        [Header("Basic Settings")]
        public AudioClip clip;
        public float startOffset = 0f;
        public bool loop = true;
        public float pitch = 1f;
        public float volumeScale = 1f;

        public AudioSource source;

        public override void OnPlay(SoundFlowPlayer engine)
        {
            if (source == null)
            {
                source = gameObject.GetComponent<AudioSource>();
                if (source == null) source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                TryApplyMixerGroup(source);

                source.clip = clip;
                source.loop = loop;
                source.pitch = pitch;
                source.volume = state.currentVolume;
            }
            if (!source.isPlaying)
            {
                if (!double.IsNaN(state.scheduledDsp))
                {
                    if (clip != null && startOffset > 0f)
                        source.time = Mathf.Clamp(startOffset, 0f, clip.length);
                    source.PlayScheduled(state.scheduledDsp);
                }
                else
                {
                    if (clip != null && startOffset > 0f)
                        source.time = Mathf.Clamp(startOffset, 0f, clip.length);
                    source.Play();
                }
            }
            state.activeFadeTime = fading.startTime;
        }

        public override void OnCleanup(SoundFlowPlayer engine)
        {
            if (source != null) source.Stop();
        }

        public override void UpdateFlowTrack(SoundFlowPlayer engine)
        {
            if (source == null) return;
            source.volume = Mathf.Clamp01(state.currentVolume * volumeScale);
        }

        public override string ToString()
        {
            string clipName = clip != null ? clip.name : "<no clip>";
            return $"Basic '{trackName}' [{clipName}]";
        }
    }
}
