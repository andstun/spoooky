using UnityEngine;

public class QuitShortcut : MonoBehaviour
{
    void Update()
    {
        // Press ESC to quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
