using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using UnityEngine.Audio;

// Display

public class UIDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KeypressManager keypressManager;
    [SerializeField] private GhostSpawner ghostSpawner;
    [SerializeField] private FPSCounter fpsCounter;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private GameObject Debugger;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private AudioMixer audioMixer;

    private Exposure exposure;

    private const string MUSIC_VOLUME_PARAM = "MUSICVOLUME";
    private const string SFX_VOLUME_PARAM = "SFXVOLUME";


    void Awake()
    {
        keypressManager.OnVPressed.AddListener(ToggleVisibility);
        keypressManager.OnDownPressed.AddListener(DecreaseExposure);
        keypressManager.OnUpPressed.AddListener(IncreaseExposure);
        keypressManager.OnQPressed.AddListener(DecreaseMusicVolume);
        keypressManager.onEPressed.AddListener(IncreaseMusicVolume);
        keypressManager.OnAPressed.AddListener(DecreaseSFXVolume);
        keypressManager.OnDPressed.AddListener(IncreaseSFXVolume);
    }

    void Update()
    {
        if (logText == null) return;

        string text = "";

        if (ghostSpawner != null)
        {
            // int numGhosts = ghostSpawner.ghostsToSpawn;
            int numGhosts = ghostSpawner.GetGhosts();
            text += $"# Ghosts: {numGhosts}\n";
        }

        if (fpsCounter != null)
        {
            text += $"FPS: {fpsCounter.CurrentFPS:F1}\n" +
                    $"Avg FPS: {fpsCounter.AverageFPS:F1}";
        }

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out exposure);
            text += $"\n\nExposure: {Math.Abs(exposure.fixedExposure.value):F2}";
        }

        if(audioMixer != null)
        {
            audioMixer.GetFloat(MUSIC_VOLUME_PARAM, out float musicVolume);
            audioMixer.GetFloat(SFX_VOLUME_PARAM, out float sfxVolume);
            text += $"\n\nMusic Volume: {musicVolume:F1} dB" +
                    $"\nSFX Volume: {sfxVolume:F1} dB";
        }

        logText.text = text;
    }

    public void IncreaseExposure()
    {
        if (exposure != null)
        {
            exposure.fixedExposure.value -= 0.1f;
        }
    }

    public void DecreaseExposure()
    {
        if (exposure != null)
        {
            exposure.fixedExposure.value += 0.1f;
        }
    }
    
    public void IncreaseMusicVolume()
    {
        audioMixer.GetFloat(MUSIC_VOLUME_PARAM, out float currentVolume);
        currentVolume = Mathf.Clamp(currentVolume + 2f, -20f, 20f);
        audioMixer.SetFloat(MUSIC_VOLUME_PARAM, currentVolume);
    }

    public void DecreaseMusicVolume()
    {
        audioMixer.GetFloat(MUSIC_VOLUME_PARAM, out float currentVolume);
        currentVolume = Mathf.Clamp(currentVolume - 2f, -20f, 20f);
        audioMixer.SetFloat(MUSIC_VOLUME_PARAM, currentVolume);
    }

    public void IncreaseSFXVolume()
    {
        audioMixer.GetFloat(SFX_VOLUME_PARAM, out float currentVolume);
        currentVolume = Mathf.Clamp(currentVolume + 2f, -20f, 20f);
        audioMixer.SetFloat(SFX_VOLUME_PARAM, currentVolume);
    }

    public void DecreaseSFXVolume()
    {
        audioMixer.GetFloat(SFX_VOLUME_PARAM, out float currentVolume);
        currentVolume = Mathf.Clamp(currentVolume - 2f, -20f, 20f);
        audioMixer.SetFloat(SFX_VOLUME_PARAM, currentVolume);
    }
    

    public void ToggleVisibility()
    {

        if (Debugger != null)
        {
            Debugger.SetActive(!Debugger.activeSelf);
        }
    }
    
}
