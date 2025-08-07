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
    public UnityEvent OnUPressed;
    public UnityEvent OnRPressed;

    void Update()
    {
        // ESC quit game (and invoke any extra listeners)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapePressed?.Invoke();
            QuitGame();
        }

        // W, L, U, and R can be extended via the inspector or from code
        if (Input.GetKeyDown(KeyCode.W)) OnWPressed?.Invoke(); // Toggle test walls / skin
        if (Input.GetKeyDown(KeyCode.L)) OnLPressed?.Invoke(); // Toggle lights (sun vs mood)
        if (Input.GetKeyDown(KeyCode.U)) OnUPressed?.Invoke(); // Toggle UI element (displays ghosts)
        if (Input.GetKeyDown(KeyCode.R)) OnRPressed?.Invoke(); // Toggle RenderTextures
    }

    // ───── Private helpers ─────
    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // stop Play Mode
#else
        Application.Quit();                                // close build
#endif
    }
}
