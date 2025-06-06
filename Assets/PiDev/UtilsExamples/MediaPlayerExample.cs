using PiDev;
using PiDev.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MediaPlayerExample : MonoBehaviour
{
    [Header("Play / PlaySolo")]
    public AudioClip music1;
    public AudioClip music2;
    public AudioClip music3;
    public AudioClip cutsceneMusic;

    [Header("Ambient Example")]
    public AudioClip ambientDay;
    public AudioClip ambientNight;
    public AudioClip ambientWater;

    [Header("Responsive example")]
    public AudioClip responsiveBackground;
    public AudioClip responsiveForeground;

    [Header("Blend example")]
    public AudioClip blendTrack1;
    public AudioClip blendTrack2;
    public AudioClip blendTrack3;
    public AnimationCurve weight1;
    public AnimationCurve weight2;
    public AnimationCurve weight3;

    private void Start()
    {
        MediaPlayer.instance.foregroundVolume = GameObject.Find("ResponsiveSlider").GetComponent<Slider>().value; // this will init the singleton
    }

    void PlayMainMusic(AudioClip music, float volume)
    {
        var mt = MediaPlayer.PlaySolo(music, 1, 1, stopAllOtherTracks: true);
        mt.name = "Music";
        mt.weight = 0; // Set weight to 0 to disable auto music blending.
        mt.targetVolume = volume; // for weight 0 you must manually control the targetVolume 
    }
    public void PlayMusic1() => PlayMainMusic(music1, 1);
    public void PlayMusic2() => PlayMainMusic(music2, 1);
    public void PlayMusic3() => PlayMainMusic(music3, 1f);

    public void PlayModal() => MediaPlayer.Play(cutsceneMusic, 10, 1, name: "Cutscene").priority = 2;
    public void StopModal() => MediaPlayer.Stop(cutsceneMusic, 2);
    public bool CutScene { set { if (value) PlayModal(); else StopModal(); } }

    public void PlayAmbientDay()
    {
        StopAmbients();
        MediaPlayer.Play(ambientDay, 0.3f, 0.7f, 4, name: "Ambient");
    }
    public void PlayAmbientNight()
    {
        StopAmbients();
        MediaPlayer.Play(ambientNight, 0.3f, 0.7f, 4, name: "Ambient");
    }

    public void PlayAmbientWater()
    {
        // StopAmbients(); - We will keep the other one playing
        MediaPlayer.Play(ambientWater, 0.7f, 1, name: "Ambient");
    }

    public void StopAmbients() => MediaPlayer.StopWithName("Ambient", removeTracks: true);

    public void PlayAmbientExample()
    {
        MediaPlayer.StopAll();
        PlayMusic1();
        PlayAmbientDay();
    }

    // Responsove 
    public void PlayResponsive()
    {
        MediaPlayer.StopAll();
        var mt = MediaPlayer.PlayLayered(responsiveBackground, responsiveForeground, 0, 1);
        mt.weight = 0;
        mt.targetVolume = 1;
    }

    public void SetForegroundVolume(float volume) => MediaPlayer.instance.foregroundVolume = volume;

    // Blend Tracks example
    MediaPlayer.MusicTrack track1;
    MediaPlayer.MusicTrack track2;
    MediaPlayer.MusicTrack track3;
    public void PlayBlendTracks()
    {
        MediaPlayer.StopAll();
        track1 = MediaPlayer.Play(blendTrack1, 0, 1, 1f, name: "Blending");
        track2 = MediaPlayer.Play(blendTrack2, 0, 1, 1f, name: "Blending");
        track3 = MediaPlayer.Play(blendTrack3, 0, 1, 1f, name: "Blending");
        SetTracksBlending(GameObject.Find("BlendSlider").GetComponent<Slider>().value);
    }

    public void SetTracksBlending(float factor)
    {
        if (track1 != null) track1.targetVolume = weight1.Evaluate(factor);
        if (track2 != null) track2.targetVolume = weight2.Evaluate(factor);
        if (track3 != null) track3.targetVolume = weight3.Evaluate(factor);
    }

    public void ExitApplication()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
    }

    // Shut up!
    public void StopAll()
    {
        MediaPlayer.StopAll();
    }
}
