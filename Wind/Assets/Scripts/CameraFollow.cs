using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 7f, -9f);
    public float smooth = 5f;

    [Header("Orbit")]
    public bool rotateWithRightClick = true;
    public float lookSensitivity = 3f;
    public float minPitch = 15f;
    public float maxPitch = 75f;
    public float minCameraY = 2f;

    public AudioSource music1;
    public AudioSource music2;
    public AudioSource music3;
    public AudioSource music4;
    public AudioSource music5;

    private float orbitYaw;
    private float orbitPitch;
    private float orbitDistance;
    private int lastZoneCount = 0;

    private void Start()
    {
        SyncOrbitFromOffset();
    }

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

    private void Update()
    {
        if (RestoreZone.TotalZonesRestored != lastZoneCount)
        {
            lastZoneCount = RestoreZone.TotalZonesRestored;
            MusicUpdate();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        UpdateOrbitInput();

        Vector3 desired = target.position + offset;
        desired.y = Mathf.Max(minCameraY, desired.y);

        Vector3 nextPosition = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
        nextPosition.y = Mathf.Max(minCameraY, nextPosition.y);
        transform.position = nextPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    private void UpdateOrbitInput()
    {
        if (!rotateWithRightClick || !Input.GetMouseButton(1))
        {
            return;
        }

        orbitYaw += Input.GetAxis("Mouse X") * lookSensitivity;
        orbitPitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        orbitPitch = Mathf.Clamp(orbitPitch, minPitch, maxPitch);

        offset = OrbitOffset();
    }

    private void SyncOrbitFromOffset()
    {
        orbitDistance = Mathf.Max(0.01f, offset.magnitude);
        Vector3 unit = offset / orbitDistance;
        orbitYaw = Mathf.Atan2(unit.x, unit.z) * Mathf.Rad2Deg;
        orbitPitch = Mathf.Asin(unit.y) * Mathf.Rad2Deg;
        orbitPitch = Mathf.Clamp(orbitPitch, minPitch, maxPitch);
        offset = OrbitOffset();
    }

    private Vector3 OrbitOffset()
    {
        Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
        return orbitRotation * Vector3.forward * orbitDistance;
    }
}
