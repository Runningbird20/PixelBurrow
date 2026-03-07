using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 7f, -9f);
    public float smooth = 5f;

    public AudioSource music1;
    public AudioSource music2; 
    public AudioSource music3;
    public AudioSource music4;
    public AudioSource music5;

    public void MusicUpdate()
    {
        switch (RestoreZone.TotalZonesRestored)
        {
            case 1:
                music1.Play();
                music2.Play();
                music3.Play();
                music4.Play();
                music5.Play();
                break;
            case 2:
                music1.mute = true;
                music2.mute = false;
                break;
            case 3:
                music2.mute = true;
                music3.mute = false;
                break;
            case 4:
                music3.mute = true;
                music4.mute = false;
                break;
            case 5:
                music4.mute = true;
                music5.mute = false;
                break;
            default:
                break;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
    
    private int lastZoneCount = 0;

    private void Update()
    {
        if (RestoreZone.TotalZonesRestored != lastZoneCount)
        {
            lastZoneCount = RestoreZone.TotalZonesRestored;
            MusicUpdate();
        }
    }
}
