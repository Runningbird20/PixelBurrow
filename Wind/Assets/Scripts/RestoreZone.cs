using UnityEngine;
using UnityEngine.Events;

public class RestoreZone : MonoBehaviour
{
    [Header("Requirements")]
    public Seed.NatureItemType requiredItemType = Seed.NatureItemType.Seed;
    public int itemsRequired = 2;

    [Header("Visual State")]
    public GameObject restoredVisuals;
    public GameObject barrenVisuals;
    public ParticleSystem restoreBurst;

    public ZonePlantSpawner plantSpawner;

    [Header("Events")]
    public UnityEvent onZoneRestored;

    private int itemsDelivered;
    private bool isRestored;

    public static int TotalZonesRestored = 0;
    public static bool pondRestored = false;

    private void Awake()
    {
        if (plantSpawner == null)
        {
            plantSpawner = GetComponent<ZonePlantSpawner>();
        }

        if (plantSpawner == null)
        {
            plantSpawner = GetComponentInChildren<ZonePlantSpawner>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRestored)
        {
            return;
        }

        Seed item = GetSeedFromCollider(other);
        if (item == null || item.isCarried || item.itemType != requiredItemType || !item.hasBeenPickedUp)
        {
            return;
        }

        itemsDelivered++;
        Destroy(item.gameObject);

        if (itemsDelivered < itemsRequired)
        {
            BarrenAudioDriver.PlayBarrenAudio();
        }

        if (itemsDelivered >= itemsRequired)
        {
            Restore();
        }
    }

    private void Restore()
    {
        isRestored = true;

        restoredVisuals.SetActive(true);

        if (plantSpawner == null)
        {
            plantSpawner = GetComponentInChildren<ZonePlantSpawner>();
        }

        if (plantSpawner != null)
        {
            plantSpawner.SpawnPlants();
        }
        else
        {
            Debug.LogWarning($"RestoreZone '{name}' has no ZonePlantSpawner assigned or found.");
        }
        
        if (barrenVisuals != null)
        {
            barrenVisuals.SetActive(false);
        }

        if (restoredVisuals != null)
        {
            restoredVisuals.SetActive(true);
        }

        if (restoreBurst != null)
        {
            restoreBurst.Play();
        }

        Transform visuals = restoredVisuals.transform;

        if (restoredVisuals != null && visuals.CompareTag("Pond"))
        {
            pondRestored = true;
        }

        AudioSource sfxPlayer = this.GetComponent<AudioSource>();

        if (sfxPlayer != null)
        {
            sfxPlayer.Play();
        }

        TotalZonesRestored++;

        onZoneRestored?.Invoke();
        WindWorldDirector.ReportZoneRestored();
    }

    private Seed GetSeedFromCollider(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        Seed seed = other.GetComponent<Seed>();
        if (seed != null)
        {
            return seed;
        }

        seed = other.GetComponentInParent<Seed>();
        if (seed != null)
        {
            return seed;
        }

        if (other.attachedRigidbody == null)
        {
            return null;
        }

        return other.attachedRigidbody.GetComponent<Seed>();
    }
}
