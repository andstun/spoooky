using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// Able to log the frames recorded by an FPSCounter object to a .csv on local drive
/// to help with FPS diagnostic and graphing
/// </summary>
public class FPSLogger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FPSCounter fpsCounter;

    [Header("Settings")]
    [SerializeField] private float logInterval = 1f;  // seconds between log entries
    [SerializeField] private string fileName = "fps_log.csv";

    private string filePath;
    private float timer;

    void Start()
    {
        if (fpsCounter == null)
            fpsCounter = FindAnyObjectByType<FPSCounter>();

        // Build full file path in persistent data folder
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        // Write header row
        var header = "Time (s),FPS,Average FPS\n";
        File.WriteAllText(filePath, header, Encoding.UTF8);

        // Debug.Log($"[FPSLogger] Logging to {filePath}");
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= logInterval && fpsCounter != null)
        {
            string line = $"{Time.time:F2},{fpsCounter.CurrentFPS:F1},{fpsCounter.AverageFPS:F1}\n";
            File.AppendAllText(filePath, line, Encoding.UTF8);
            timer = 0f;
        }
    }
}
