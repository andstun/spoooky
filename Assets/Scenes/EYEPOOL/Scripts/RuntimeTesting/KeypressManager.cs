using UnityEngine;
using UnityEngine.Events;      // lets you wire callbacks in the inspector

/// <summary>
/// Fires UnityEvents when specific keys are pressed.  
/// Add listeners in code or directly in the inspector.
/// </summary>

public class KeypressManager : MonoBehaviour
{
    // ───── Public events you can hook into ─────
    [Header("Key-press callbacks")]
    public UnityEvent OnEscapePressed;
    public UnityEvent OnWPressed;
    public UnityEvent OnLPressed;
    public UnityEvent OnVPressed;
    public UnityEvent OnRPressed;
    public UnityEvent OnDownPressed;
    public UnityEvent OnUpPressed;
    public UnityEvent OnAPressed;
    public UnityEvent OnDPressed;
    public UnityEvent OnQPressed;
    public UnityEvent onEPressed;
    void Update()
    {
        // ESC quit game (and invoke any extra listeners)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapePressed?.Invoke();
            QuitGame();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            OnRPressed?.Invoke();
            RestartGame();
        }

        // W, L, U, and T can be extended via the inspector or from code
        if (Input.GetKeyDown(KeyCode.W)) OnWPressed?.Invoke(); // Toggle test walls / skin
        if (Input.GetKeyDown(KeyCode.L)) OnLPressed?.Invoke(); // Toggle lights (sun vs mood)
        if (Input.GetKeyDown(KeyCode.V)) OnVPressed?.Invoke(); // Toggle UI element (displays ghosts)
        if (Input.GetKeyDown(KeyCode.R)) OnRPressed?.Invoke(); // Toggle Scene Restart
        if (Input.GetKeyDown(KeyCode.T)) OnRPressed?.Invoke(); // Toggle RenderTextures
        if (Input.GetKeyDown(KeyCode.DownArrow)) OnDownPressed?.Invoke(); // Decrease Exposure
        if (Input.GetKeyDown(KeyCode.UpArrow)) OnUpPressed?.Invoke(); // Increase Exposure
        if (Input.GetKeyDown(KeyCode.A)) OnAPressed?.Invoke(); // Decrease SFX Volume
        if (Input.GetKeyDown(KeyCode.D)) OnDPressed?.Invoke(); // Increase SFX Volume
        if (Input.GetKeyDown(KeyCode.Q)) OnQPressed?.Invoke(); // Decrease Music Volume
        if (Input.GetKeyDown(KeyCode.E)) onEPressed?.Invoke(); // Increase Music Volume
    }

    // ───── Private helpers ─────
    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // stop Play Mode
#else
        Application.Quit();                                // close build
#endif
    }
}
