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

    [Header("Life Spawn")]
    public GameObject[] plantPrefabs;
    public int plantsToSpawn = 3;
    public GameObject[] animalPrefabs;
    public int animalsToSpawn = 2;
    public float spawnRadius = 3f;

    [Header("Events")]
    public UnityEvent onZoneRestored;

    private int itemsDelivered;
    private bool isRestored;

    public static int TotalZonesRestored = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (isRestored)
        {
            return;
        }

        Seed item = other.GetComponent<Seed>();
        if (item == null || item.isCarried || item.itemType != requiredItemType)
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

        this.GetComponent<AudioSource>()?.Play();

        TotalZonesRestored++;
        

        SpawnPrefabs(plantPrefabs, plantsToSpawn);
        SpawnPrefabs(animalPrefabs, animalsToSpawn);

        onZoneRestored?.Invoke();
        WindWorldDirector.ReportZoneRestored();
    }

    private void SpawnPrefabs(GameObject[] prefabs, int amount)
    {
        if (prefabs == null || prefabs.Length == 0 || amount <= 0)
        {
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null)
            {
                continue;
            }

            Vector2 offset2D = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = transform.position + new Vector3(offset2D.x, 0f, offset2D.y);

            Instantiate(prefab, spawnPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        }
    }
}
