using UnityEngine;
using UnityEngine.UI;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * Licensed under MIT
 *
 * ============= Description =============
 * Used for debugging SoundFlowPlayer and its tracks.
 * 
 * ============= Usage =============
 * Add to a GameObject with a Text component.
 */

namespace PiDev.SoundFlow
{
    [RequireComponent(typeof(Text))]
    public class SoundFlowDebugger : MonoBehaviour
    {
        Text text; SoundFlowPlayer sf;

        void Start() { text = GetComponent<Text>(); }

        void Update()
        {
            if (SoundFlowPlayer.instance == null) { text.text = "<b>SoundFlow</b> Not Available"; return; }
            if (sf == null) sf = SoundFlowPlayer.instance;

            var tracks = sf._tracks;
            if (tracks == null || tracks.Count == 0) { text.text = "<i>No Tracks</i>"; return; }

            System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
            foreach (var t in tracks)
            {
                if (t == null) continue;
                sb.Append(t.ToString());
                sb.Append(" | V="); 
                
                sb.Append(t.state.currentVolume.ToString("0.00"));
                sb.Append("->"); 
                sb.Append( (t.settings.manualVolumeControl ? t.settings.targetVolume : t.state.rampTarget).ToString("0.00"));
                
                sb.Append(" W="); sb.Append(t.settings.weight.ToString("0.00"));
                sb.Append(" P="); sb.Append(t.settings.priority);
                sb.Append(" | ");
                if (t.settings.mute) sb.Append("M");
                if (t.settings.manualVolumeControl) sb.Append("U");
                if (t.settings.exclusiveMode) sb.Append("X");
                if (t.state.removeIfSilent) sb.Append("R");
                if (t.state.hasError) sb.Append(" ERROR");
                sb.Append('\n');
            }
            text.text = sb.ToString();
        }
    }
}
