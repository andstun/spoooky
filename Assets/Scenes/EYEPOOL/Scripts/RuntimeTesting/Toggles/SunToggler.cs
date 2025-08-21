using UnityEngine;

public class SunToggler : MonoBehaviour
{
    [SerializeField] private Light sunLight;
    [SerializeField] private KeypressManager keypressManager;
    void Awake()
    {
        keypressManager.OnLPressed.AddListener(ToggleSun);
    }

    void ToggleSun()
    {
        sunLight.enabled = !sunLight.enabled;
    }
}