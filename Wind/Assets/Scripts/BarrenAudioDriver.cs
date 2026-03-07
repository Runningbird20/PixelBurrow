using UnityEngine;

public class BarrenAudioDriver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void PlayBarrenAudio()
    {
        var instance = FindObjectOfType<BarrenAudioDriver>();
        if (instance != null)
        {
            instance.GetComponent<AudioSource>()?.Play();
        }
    }
}
