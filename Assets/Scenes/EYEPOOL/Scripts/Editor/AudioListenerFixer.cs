using UnityEngine;
using UnityEditor;

/// <summary>
/// There are a lot of camera objects floating around the scene, so this tool
/// disables all but one audio listener to avoid triggering Unity warnings
/// </summary>
public class AudioListenerFixer
{
    [MenuItem("Tools/Remove Extra AudioListeners")]
    public static void FixAudioListeners()
    {
        var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length <= 1)
        {
            // Debug.Log("Only one AudioListener found. No changes made.");
            return;
        }

        // Debug.LogWarning($"Found {listeners.Length} AudioListeners. Disabling extras...");

        for (int i = 1; i < listeners.Length; i++)
        {
            listeners[i].enabled = false;
            // Debug.Log($"Disabled AudioListener on GameObject: {listeners[i].gameObject.name}");
        }
    }
}
