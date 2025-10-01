using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
    public bool playAsOneShot = false;
    public AudioMixerGroup mixer;
    
    [Header("Fade Settings")]
    public bool useFadeIn = false;
    [Range(0.1f, 10f)]
    public float fadeInTime = 1f;
    public bool useFadeOut = false;
    [Range(0.1f, 10f)]
    public float fadeOutTime = 1f;
    
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    // Start is called before the first frame update
    void Start()
    {
        Play("Game Start");
        Play("Main Theme");
        Play("Portal Sound");
        Play("Fireplace");
    }
    
    void Awake()
    {
        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.mixer;
        }
        
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        
        if (s.useFadeIn)
        {
            StartCoroutine(FadeIn(s, s.fadeInTime));
        }
        else
        {
            if (s.playAsOneShot)
            {
                s.source.PlayOneShot(s.clip, s.volume);
            }
            else
            {
                s.source.volume = s.volume;
                s.source.Play();                
            }

        }
    }
    
    public void PlayPoint(string name, Vector3 position)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        AudioSource.PlayClipAtPoint(s.clip, position, s.volume);
    }

    public void StopLoop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;

        if (s.useFadeOut)
        {
            StartCoroutine(FadeOut(s, s.fadeOutTime));
        }
        else
        {
            s.source.Stop();
        }
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        s.source.Pause();
    }
    
    public void UnPause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        s.source.UnPause();
    }

    public void IncreaseVolume(string name, float amount)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        s.source.volume = Mathf.Clamp(s.source.volume + amount, 0.1f, 1f);
    }

    public void DecreaseVolume(string name, float amount)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        s.source.volume = Mathf.Clamp(s.source.volume - amount, 0.1f, 1f);
    }
    
    // Fade in over specified time
    public void FadeIn(string name, float fadeTime)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        StartCoroutine(FadeIn(s, fadeTime));
    }
    
    // Fade out over specified time
    public void FadeOut(string name, float fadeTime)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        StartCoroutine(FadeOut(s, fadeTime));
    }
    
    private IEnumerator FadeIn(Sound sound, float fadeTime)
    {
        sound.source.volume = 0f;
        sound.source.Play();
        
        float startVolume = 0f;
        float targetVolume = sound.volume;
        
        while (sound.source.volume < targetVolume)
        {
            sound.source.volume += (targetVolume - startVolume) * Time.deltaTime / fadeTime;
            yield return null;
        }
        
        sound.source.volume = targetVolume;
    }
    
    private IEnumerator FadeOut(Sound sound, float fadeTime)
    {
        float startVolume = sound.source.volume;
        
        while (sound.source.volume > 0)
        {
            sound.source.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        
        sound.source.Stop();
        sound.source.volume = sound.volume; // Reset to original volume
    }
}
