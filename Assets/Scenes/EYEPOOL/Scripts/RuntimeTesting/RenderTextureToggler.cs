using UnityEngine;

public class RenderTextureToggler : MonoBehaviour
{
    [SerializeField] KeypressManager keypressManager;
    [SerializeField] GameObject EYEPOOL_With_RenderTextures;
    private bool isVisible = false;
    void Awake()
    {
        keypressManager.OnRPressed.AddListener(ToggleRenderTextures);
    }

    void ToggleRenderTextures()
    {
        isVisible = !isVisible;
        EYEPOOL_With_RenderTextures.SetActive(isVisible);
    }
}