using UnityEngine;
using TMPro;

// Display

public class UIDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KeypressManager keypressManager;
    [SerializeField] private GhostSpawner ghostSpawner;
    [SerializeField] private FPSCounter fpsCounter;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private CanvasGroup canvasGroup;
    private bool isDisabled = false;

    void Awake()
    {
        keypressManager.OnUPressed.AddListener(ToggleVisibility);
    }
    void Update()
    {
        if (logText == null) return;

        string text = "";

        if (ghostSpawner != null)
        {
            // int numGhosts = ghostSpawner.ghostsToSpawn;
            int numGhosts = ghostSpawner.GetGhosts();
            float avgGhostSpeed = ghostSpawner.getAvgGhostMovementSpeed();
            text += $"# Ghosts: {numGhosts}\nGhost Speed: {avgGhostSpeed:F2}\n\n";
        }

        if (fpsCounter != null)
        {
            text += $"FPS: {fpsCounter.CurrentFPS:F1}\n" +
                    $"Avg FPS: {fpsCounter.AverageFPS:F1}";
        }

        logText.text = text;
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
