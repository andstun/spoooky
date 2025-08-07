using UnityEngine;
using TMPro;

// Display

public class UIDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KeypressManager keypressManager;
    [SerializeField] private GhostSpawner ghostSpawner;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private CanvasGroup canvasGroup;
    private bool isDisabled = false;

    void Awake()
    {
        keypressManager.OnUPressed.AddListener(ToggleVisibility);
    }
    void Update() 
    {
        if (ghostSpawner != null && logText != null)
        {
            int numGhosts = ghostSpawner._ghostsToSpawn;
            float avgGhostSpeed = ghostSpawner.getAvgGhostMovementSpeed();

            logText.text = $"# Ghosts: {numGhosts}\n\nGhost Speed: {avgGhostSpeed:F2}";
        }
    }

    void ToggleVisibility()
    {
        isDisabled = !isDisabled;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = isDisabled ? 0 : 1;
            canvasGroup.interactable = !isDisabled;
            canvasGroup.blocksRaycasts = !isDisabled;
        }
    }
}
