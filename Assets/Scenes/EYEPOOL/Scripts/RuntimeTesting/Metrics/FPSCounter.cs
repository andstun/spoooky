using UnityEngine;

/// <summary>
///  Keeps a count of FPS frames, which can be used by the UI Display. 
/// </summary>
public class FPSCounter : MonoBehaviour
{
    public float CurrentFPS { get; private set; }
    public float AverageFPS { get; private set; }

    [SerializeField] private int averageSampleSize = 60;
    private float[] frameTimes;
    private int frameCount;

    void Awake()
    {
        frameTimes = new float[averageSampleSize];
    }

    void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        CurrentFPS = 1f / deltaTime;

        // Track for moving average
        frameTimes[frameCount % averageSampleSize] = CurrentFPS;
        frameCount++;

        float total = 0f;
        int samples = Mathf.Min(frameCount, averageSampleSize);
        for (int i = 0; i < samples; i++) total += frameTimes[i];
        AverageFPS = total / samples;
    }
}
