using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -6);
    public float smooth = 5f;

    public static void MusicStart()
    {
        switch (RestoreZone.numRestored)
        {
            case 1:
                Camera.main.GetComponent<AudioSource>().Play();
                break;
            // case 2:
            //     AudioManager.Instance.Play("Music2");
            //     break;
            // case 3:
            //     AudioManager.Instance.Play("Music3");
            //     break;
            default:
                break;
        }
    }

    void LateUpdate()
    {
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);

        transform.LookAt(target);
    }
}