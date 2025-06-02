using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace PiDev.Utilities
{
    public class MediaPlayerDebugger : MonoBehaviour
    {
        Text text;
        MediaPlayer mp = null;

        void Start()
        {
            text = GetComponent<Text>();
        }

        void Update()
        {
            if (!MediaPlayer.HasInstance)
            {
                text.text = "<b>MediaPlayer</b> Not Available";
                return;
            }

            if (mp == null) mp = MediaPlayer.Instance;
            string s = "";
            foreach (var track in mp.tracks)
                if (track != null) s += track.ToString() + "\n";
            text.text = s;
        }
    }
}