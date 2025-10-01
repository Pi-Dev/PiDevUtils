using PiDev.SoundFlow;
using UnityEngine;
using UnityEngine.UI;
using static PiDev.SoundFlow.FlowTrackBase;

public class SoundFlowExample : MonoBehaviour
{
    [Header("Play / PlaySolo")]
    public AudioClip music1;
    public AudioClip music2;
    public AudioClip music3;

    [Header("Ambient Example")]
    public AudioClip ambientDay;
    public AudioClip ambientNight;
    public AudioClip ambientWater;

    public BasicFlowTrack ModalMusicTrack;

    [Header("Responsive Examples")]
    public LayeredFlowTrack layeredTrack;
    public AdaptiveFlowTrack adaptiveTrack;

    // Implementation
    SoundFlowPlayer sf => SoundFlowPlayer.instance;

    private void Start()
    {
        float responsiveSliderVolume = GameObject.Find("IntensitySlider").GetComponent<Slider>().value;
        SetAdaptiveIntensity(responsiveSliderVolume);
    }

    BasicFlowTrack PlayMainMusic(AudioClip music, float volume, float weight)
    {
        SoundFlowPlayer.instance.StopWithName("Music", unload: false);
        var mt = SoundFlowPlayer.instance.Play(music, weight, volume, name: "Music");
        return mt;
    }
    public void PlayMusic1() => PlayMainMusic(music1, 1, 1);
    public void PlayMusic2() => PlayMainMusic(music2, 1, 1);
    public void PlayMusic3() => PlayMainMusic(music3, 1, 1);

    public void PlayModal() { sf.Play(ModalMusicTrack, 1); ModalMusicTrack.settings.mute = false;  } 
    public void StopModal() { ModalMusicTrack.settings.mute = true; } 

    public bool CutScene { set { if (value) PlayModal(); else StopModal(); } }

    // Faders
    Fading fade4s => new Fading() { startTime = 4, resumeTime = 4, stopTime = 2, pauseTime = 1 };
    Fading fade1s => new Fading() { startTime = 1, resumeTime = 1, stopTime = 1, pauseTime = 1 };

    public void PlayAmbientDay()
    {
        StopAmbients();
        var track = sf.Play(ambientDay, 0.3f, 0.7f, name: "Ambient", fading: fade4s);
    }
    public void PlayAmbientNight()
    {
        StopAmbients();
        var track = sf.Play(ambientNight, 0.3f, 0.7f, name: "Ambient", fading: fade4s);
    }

    public void PlayAmbientWater()
    {
        sf.Play(ambientWater, 0.7f, 1, name: "Ambient");
    }

    public void StopAmbients() => sf.StopWithName("Ambient", 1, true);

    public void PlayAmbientExample()
    {
        sf.StopAll();
        PlayMusic1();
        PlayAmbientDay();
    }

    // Responsive Layered exmple
    public void PlayLayered()
    {
        sf.StopAll();
        sf.Play(layeredTrack, 1);
    }

    public void SetLayeredDrumsVolume(float volume)
    {
        layeredTrack.SetLayerWeight("Melody", volume);
    }

    // Responsive Adaptive example
    public void PlayAdaptive()
    {
        sf.StopAll();
        sf.Play(adaptiveTrack, 1);
    }

    public void SetAdaptiveIntensity(float factor)
    {
        adaptiveTrack.intensity = factor;
    }

    // Utils
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
        SoundFlowPlayer.instance.StopAll();
    }
}
