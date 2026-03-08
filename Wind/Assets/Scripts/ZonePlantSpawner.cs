using UnityEngine;
using System.Collections.Generic;

public class ZonePlantSpawner : MonoBehaviour
{
    [Header("Plant Prefabs")]
    public GameObject[] plantPrefabs;

    [Header("Spawn Settings")]
    public int plantCount = 10;
    public string plantSpawnZoneTag = "PlantSpawnZone";
    public float yOffset = 0f;
    public bool spawnOnStart = false;
    public LayerMask groundMask = ~0;
    public float raycastStartHeight = 100f;

    [Header("Random Scale")]
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    [Header("Parent")]
    public Transform plantParent;

    private bool hasSpawned = false;
    private readonly List<Collider> spawnZoneColliders = new List<Collider>();

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnPlants();
        }
    }

    public void SpawnPlants()
    {
        if (hasSpawned)
        {
            Debug.Log($"ZonePlantSpawner '{name}' skipped: plants already spawned.");
            return;
        }

        if (plantPrefabs == null || plantPrefabs.Length == 0)
        {
            Debug.LogWarning($"ZonePlantSpawner '{name}' has no plant prefabs assigned.");
            return;
        }

        if (plantCount <= 0)
        {
            Debug.LogWarning($"ZonePlantSpawner '{name}' has plantCount <= 0.");
            return;
        }

        if (!CacheSpawnZones())
        {
            return;
        }

        hasSpawned = true;

        Transform parentToUse = plantParent != null ? plantParent : transform;

        for (int i = 0; i < plantCount; i++)
        {
            GameObject prefab = plantPrefabs[Random.Range(0, plantPrefabs.Length)];
            if (prefab == null) continue;

            if (!TryGetSpawnPosition(out Vector3 spawnPos))
            {
                continue;
            }

            Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject plant = Instantiate(prefab, spawnPos, randomRot, parentToUse);

            float randomScale = Random.Range(scaleRange.x, scaleRange.y);
            plant.transform.localScale *= randomScale;
        }
    }

    private bool CacheSpawnZones()
    {
        spawnZoneColliders.Clear();

        if (string.IsNullOrWhiteSpace(plantSpawnZoneTag))
        {
            Debug.LogWarning($"ZonePlantSpawner '{name}' has an empty plant spawn tag.");
            return false;
        }

        GameObject[] taggedZones;
        try
        {
            taggedZones = GameObject.FindGameObjectsWithTag(plantSpawnZoneTag);
        }
        catch (UnityException)
        {
            Debug.LogWarning($"Tag '{plantSpawnZoneTag}' is not defined. Add it in Project Settings > Tags and Layers.");
            return false;
        }

        for (int i = 0; i < taggedZones.Length; i++)
        {
            Collider[] colliders = taggedZones[i].GetComponentsInChildren<Collider>();
            for (int j = 0; j < colliders.Length; j++)
            {
                if (colliders[j] != null)
                {
                    spawnZoneColliders.Add(colliders[j]);
                }
            }
        }

        if (spawnZoneColliders.Count == 0)
        {
            Debug.LogWarning($"ZonePlantSpawner '{name}' found no colliders on objects tagged '{plantSpawnZoneTag}'.");
            return false;
        }

        return true;
    }

    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;

        spawnZoneColliders.RemoveAll(collider => collider == null);
        if (spawnZoneColliders.Count == 0)
        {
            if (!CacheSpawnZones())
            {
                return false;
            }
        }

        Collider zoneCollider = spawnZoneColliders[Random.Range(0, spawnZoneColliders.Count)];
        Bounds bounds = zoneCollider.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        float rayStartY = bounds.max.y + Mathf.Max(1f, raycastStartHeight);
        Vector3 rayStart = new Vector3(x, rayStartY, z);
        float rayLength = Mathf.Max(10f, raycastStartHeight * 2f);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            spawnPosition = new Vector3(x, hit.point.y + yOffset, z);
            return true;
        }

        spawnPosition = new Vector3(x, bounds.min.y + yOffset, z);
        return true;
    }
}
