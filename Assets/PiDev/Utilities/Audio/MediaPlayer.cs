using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.XR;

namespace PiDev.Utilities
{
    public class MediaPlayer : GlobalSingleton<MediaPlayer>
    {
        [Serializable]
        public class MusicTrack
        {
            public AudioSource source;
            public AudioSource mixableSource;
            public string name;     // categorization
            public float currentVolume;
            public float targetVolume; // if weight is 0, set this to control interolation
            public float fadeTime;  // the time needed to fade to target


            // if >0 used by Update to add to 1, if not 0 must be 
            // manually controlled by code, by setting targetVolume accordingly!
            public float weight;
            public float volume;
            public int priority = 0;
            public float priorityMultiplier = 1;

            // if >0 delay will be eaten on Update, targetVolume will not update if delay>0
            public float delay;
            public bool removeIfSilent = false;
            public void Stop(float time = float.NaN, bool removeWhenDone = true)
            {
                if (!float.IsNaN(time)) fadeTime = time;
                weight = -1;
                targetVolume = 0;
                removeIfSilent = removeWhenDone;
                if(removeWhenDone) priority = 0;
            }
            public override string ToString()
            {
                string name = source == null ? "null" : source.clip ? source.clip.name : "null";
                string clipname = string.IsNullOrEmpty(this.name) ? "-" : this.name;
                return $"{name} | {this.name,7} | " +
                    $"vol={volume.ToString("N2"),4}, " +
                    $"{currentVolume.ToString("N2"),4}->{targetVolume.ToString("N2"),4} | " +
                    $"weight={weight.ToString("N2"),5} | " +
                    $"fade={fadeTime.ToString("N2"),5} | " +
                    $"delay={delay.ToString("N2"),5} | " +
                    $"{(removeIfSilent ? "RS" : "--")}";
            }
        }

        public AudioMixerGroup mixerGroupMusic;
        public AudioMixerGroup mixerGroupSFX;
        public AudioMixerGroup mixerGroupAmbient; // will be used but mapped to music

        public float foregroundVolume = 1;

        public List<MusicTrack> tracks = new List<MusicTrack>();

        private void Update()
        {
            // remove expired music tracks
            var newTracks = new List<MusicTrack>(tracks.Count);
            foreach (var t in tracks)
            {
                if (t.removeIfSilent && t.currentVolume <= 0.0001) Destroy(t.source);
                else newTracks.Add(t);
            }
            tracks = newTracks;

            // 1. set targetVolume based on weight ratios if weight is >0
            float sum = 0;
            var weightedTracks = tracks.Where(t => t.weight > 0).ToList();
            foreach (var mt in weightedTracks) sum += mt.weight;
            float ratio = 1 / sum;
            if (sum > 0)
                foreach (var mt in weightedTracks)
                    mt.targetVolume = mt.weight * ratio;

            // 1.5, cannot trust ChatGPT on this
            int maxPriority = tracks.Any() ? tracks.Max(t => t.priority) : 0;
            bool hasModal = tracks.Any(t => t.priority == maxPriority && maxPriority > 0);

            foreach (var t in tracks)
            {
                if (hasModal)
                {
                    float targetPriorityMultiplier = (t.priority == maxPriority) ? 1f : 0f;
                    t.priorityMultiplier = Mathf.MoveTowards(t.priorityMultiplier, targetPriorityMultiplier, 1 / t.fadeTime * Time.unscaledDeltaTime);
                }
                else
                {
                    t.priorityMultiplier = Mathf.MoveTowards(t.priorityMultiplier, 1f, 1 / t.fadeTime * Time.unscaledDeltaTime);
                }
            }


            // 2. interpolate all tracks towards designated goal based on their fadeTime and targetVolume
            foreach (var t in tracks)
            {
                if (t.delay > 0)
                {
                    t.delay = Mathf.MoveTowards(t.delay, 0, Time.unscaledDeltaTime);
                }
                else
                {
                    t.currentVolume = Mathf.MoveTowards(t.currentVolume, t.targetVolume, 1 / t.fadeTime * Time.unscaledDeltaTime);
                    t.source.volume = t.currentVolume * t.volume * t.priorityMultiplier;
                    if (t.mixableSource) t.mixableSource.volume = t.currentVolume * t.volume * foregroundVolume * t.priorityMultiplier;
                }
            }
        }

        /// <summary>
        /// Fades out all tracks. fadeTime is optional. removeTracks destroys the audioSources
        /// </summary>
        public static void StopAll(float fadeTime = float.NaN, bool removeTracks = true)
        {
            foreach (var t in Instance.tracks)
            {
                if (!float.IsNaN(fadeTime)) t.fadeTime = fadeTime;
                t.weight = -1;
                t.targetVolume = 0;
                if (removeTracks) t.removeIfSilent = true;
            }
        }

        /// <summary>
        /// Fades out all tracks with given name.
        /// fadeTime is optional. removeTracks destroys the audioSources
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fadeTime"></param>
        public static void StopWithName(string name, float fadeTime = float.NaN, bool removeTracks = true)
        {
            foreach (var t in Instance.tracks)
                if (t.name == name)
                {
                    if (!float.IsNaN(fadeTime)) t.fadeTime = fadeTime;
                    t.weight = -1;
                    t.targetVolume = 0;
                    if (removeTracks) t.removeIfSilent = true;
                }
        }

        /// <summary>
        /// Fades out all tracks with given name.
        /// fadeTime is optional. removeTracks destroys the audioSources
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fadeTime"></param>
        public static void Stop(MusicTrack t, float fadeTime = float.NaN, bool removeTracks = true)
        {

            if (!float.IsNaN(fadeTime)) t.fadeTime = fadeTime;
            t.weight = -1;
            t.targetVolume = 0;
            if (removeTracks) t.removeIfSilent = true;
        }

        /// <summary>
        /// Fades out the track that plays specified clip, if any.
        /// fadeTime is optional. removeTracks destroys the audioSources
        /// </summary>
        /// <param name="clip"></param>
        public static MusicTrack Stop(AudioClip clip, float fadeTime = float.NaN, bool removeTrack = true)
        {
            var x = Get(clip, false);
            if (x != null) x.Stop(fadeTime, removeTrack);
            return x;
        }

        /// <summary>
        /// Gets or adds a new music track to the player
        /// </summary>
        public static MusicTrack Play(AudioClip clip, float weight, float volume, float fadeTime = 1, bool loop = true, string name = "")
        {
            var mt = Get(clip, true);
            mt.source.loop = loop;
            mt.weight = weight;
            mt.volume = volume;
            mt.fadeTime = fadeTime;
            mt.removeIfSilent = false;
            mt.name = name;
            return mt;
        }

        public static MusicTrack PlayLayered(AudioClip background, AudioClip foreground, float weight, float volume, float fadeTime = 1, bool loop = true, string name = "")
        {
            var mt = Get(background, true, foreground);
            mt.source.loop = loop;
            mt.mixableSource.loop = loop;
            mt.weight = weight;
            mt.volume = volume;
            mt.fadeTime = fadeTime;
            mt.removeIfSilent = false;
            mt.name = name;
            return mt;
        }

        public static MusicTrack Get(AudioClip clip, bool createIfNotExists = false, AudioClip mixableClip = null)
        {
            var mt = Instance.tracks.Where(t => t.source.clip == clip).FirstOrDefault();
            if (mt == null && createIfNotExists)
            {
                mt = new MusicTrack();

                mt.source = Instance.gameObject.AddComponent<AudioSource>();
                mt.source.volume = 0;
                mt.source.outputAudioMixerGroup = Instance.mixerGroupMusic;
                mt.source.clip = clip;
                mt.source.bypassReverbZones = true;
                mt.source.priority = 0;

                if (mixableClip != null)
                {
                    mt.mixableSource = Instance.gameObject.AddComponent<AudioSource>();
                    mt.mixableSource.volume = 0;
                    mt.mixableSource.outputAudioMixerGroup = Instance.mixerGroupMusic;
                    mt.mixableSource.clip = mixableClip;
                    mt.mixableSource.bypassReverbZones = true;
                    mt.mixableSource.priority = 0;
                }

                var dst = AudioSettings.dspTime;
                mt.source.PlayScheduled(dst + 0.3f);
                mt.mixableSource?.PlayScheduled(dst + 0.3f);
                Instance.tracks.Add(mt);
            }
            return mt;
        }

        /// <summary>
        /// Gets or adds a new music track to the player then sets it as weight 1 are zeroes all other track weights
        /// </summary>
        public static MusicTrack PlaySolo(AudioClip clip, float volume, float fadeTime, float delayTime = 0, bool loop = true, bool stopAllOtherTracks = false, string name = "")
        {
            foreach (var t in Instance.tracks)
            {
                t.weight = 0;
                t.targetVolume = 0;
                if (stopAllOtherTracks) t.removeIfSilent = true;
            }
            var mt = Play(clip, 1, volume, fadeTime, loop);
            mt.delay = delayTime;
            mt.name = name;
            return mt;
        }

        public static MusicTrack PlaySoloLayered(AudioClip background, AudioClip foreground, float volume, float fadeTime, float delayTime = 0, bool loop = true, bool stopAllOtherTracks = false)
        {
            foreach (var t in Instance.tracks)
            {
                t.weight = 0;
                if (stopAllOtherTracks) t.removeIfSilent = true;
            }
            var mt = PlayLayered(background, foreground, 1, volume, fadeTime, loop);
            mt.delay = delayTime;
            return mt;
        }

        public static void ShutDown()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }
    }
}