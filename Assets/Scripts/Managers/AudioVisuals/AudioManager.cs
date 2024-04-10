using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioEventsSO audioEvents;
    private Dictionary<EnumsBase.audio, AudioSource> sources = new Dictionary<EnumsBase.audio, AudioSource>();
    
    private void Awake()
    {
        audioEvents.OnAudioSourceLoaded += LoadAudioSource;
        audioEvents.OnAudioSourcePlayed += PlayAudioSource;

        #region Load Source into the AudioManager
        foreach (AudioEventsSO.Sound s in audioEvents.sounds)
        {
            GameObject source = gameObject;
            audioEvents.AudioSourceLoad(ref source, new AudioEventsSO.AudioEventsArgs
            {
                audio = s.audio
            });
            AudioSource audioSource = source.GetComponent<AudioSource>();
            sources.Add(s.audio, audioSource);
        }
        #endregion

    }

    #region Load Audio Source
    private void LoadAudioSource(ref GameObject source, AudioEventsSO.AudioEventsArgs e)
        => LoadAudioSource(ref source, e.sound);
    private void LoadAudioSource(ref GameObject source, AudioEventsSO.Sound sound)
    {
        AudioSource audioSource = source.AddComponent<AudioSource>();
        audioSource.clip = sound.audioClip;
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;
        audioSource.playOnAwake = false;
    }
    #endregion

    #region Play Audio Source
    private void PlayAudioSource(ref GameObject source, AudioEventsSO.AudioEventsArgs e)
    {
        AudioSource audioSource = null;
        if (source != null)
            audioSource = source.GetComponent<AudioSource>();
        else
            audioSource = sources[e.audio];

        if(audioSource != null)
        {
            audioSource.Play();
            if (e.durationPercent < 1)
            {
                StartCoroutine(DelayCall(audioSource.clip.length * e.durationPercent, 
                    () => audioSource.Stop()));
            }
        }
    }
    private IEnumerator DelayCall(float timer, Action action)
    {
        yield return new WaitForSeconds(timer);
        action();
    }
    #endregion

    private void OnDisable()
    {
        audioEvents.OnAudioSourceLoaded -= LoadAudioSource;
        audioEvents.OnAudioSourcePlayed -= PlayAudioSource;
    }

}


