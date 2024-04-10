using System;
using UnityEngine;
using UnityEngine.iOS;

[CreateAssetMenu(fileName = "AudioEventsSO", menuName = "ScriptableObject/AudioEventsSO")]
public class AudioEventsSO : ScriptableObject
{
    #region Sounds
    [SerializeField] private Sound[] _sounds;
    public Sound[] sounds { get { return _sounds; } }
    #endregion

    #region Audio Events Args
    public class AudioEventsArgs : EventArgs
    {
        public EnumsBase.audio audio { get; set; }
        public Sound sound { get; set; }
        public float durationPercent { get; set; }
    }
    #endregion

    public delegate void AudioEvents(ref GameObject source, AudioEventsArgs e);
    #region Loading Source Event
    public event AudioEvents OnAudioSourceLoaded;
    public void AudioSourceLoad(ref GameObject source, AudioEventsArgs e)
    {
        e.sound = Array.Find(sounds, s => s.audio == e.audio);
        OnAudioSourceLoaded?.Invoke(ref source, e);
    }
    #endregion

    #region Play Source Event
    public event AudioEvents OnAudioSourcePlayed;
    public void AudioSourcePlay(ref GameObject source, AudioEventsArgs e)
        => OnAudioSourcePlayed?.Invoke(ref source, e); 
    #endregion

    [System.Serializable]
    public class Sound
    {
        public EnumsBase.audio audio;

        public AudioClip audioClip;

        [Range(0f, 1f)]
        public float volume;
        [Range(0.1f, 3f)]
        public float pitch;
    }

}


