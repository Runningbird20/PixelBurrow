using UnityEngine;

public class RestoreZone : MonoBehaviour
{
    public static int numRestored = 0;

    public int seedsRequired = 2;
    private int seedsDelivered = 0;

    public GameObject restoredVisuals;
    public GameObject barrenVisuals;

    private bool isRestored = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isRestored) return;

        Seed seed = other.GetComponent<Seed>();

        if (seed != null && !seed.isCarried)
        {
            seedsDelivered++;

            if (seedsDelivered < seedsRequired) this.GetComponent<AudioSource>().Play();

            Destroy(seed.gameObject);

            Debug.Log("Seed delivered: " + seedsDelivered + "/" + seedsRequired);

            if (seedsDelivered >= seedsRequired)
            {
                Restore();
            }

            CameraFollow.MusicStart();
        }
    }

    void Restore()
    {
        isRestored = true;

        if (barrenVisuals != null)
            barrenVisuals.SetActive(false);

        if (restoredVisuals != null)
            restoredVisuals.SetActive(true);

        Debug.Log(gameObject.name + " restored!");

        numRestored++;
    }
}