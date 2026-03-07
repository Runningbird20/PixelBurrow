using UnityEngine;

public class WindWorldDirector : MonoBehaviour
{
    [Header("World Progress")]
    public int totalZones = 2;

    [Header("Atmosphere")]
    public AudioSource musicSource;
    public float baseMusicPitch = 0.92f;
    public float maxMusicPitch = 1.1f;
    public Light directionalLight;
    public Gradient worldColorByRestoration;

    private static WindWorldDirector instance;
    private static int restoredZones;

    private void Awake()
    {
        instance = this;
        restoredZones = 0;
        ApplyWorldMood();
    }

    public static void ReportZoneRestored()
    {
        restoredZones++;

        if (instance != null)
        {
            instance.ApplyWorldMood();
        }
    }

    private void ApplyWorldMood()
    {
        float progress = totalZones <= 0 ? 1f : Mathf.Clamp01((float)restoredZones / totalZones);

        if (musicSource != null)
        {
            musicSource.pitch = Mathf.Lerp(baseMusicPitch, maxMusicPitch, progress);
        }

        if (directionalLight != null)
        {
            directionalLight.color = worldColorByRestoration.Evaluate(progress);
        }
    }
}
