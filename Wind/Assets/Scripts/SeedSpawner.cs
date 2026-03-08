using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedSpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public Seed seedPrefab;
    public int targetSeedCount = 10;
    public bool useTaggedSpawnAreas = true;
    public string spawnAreaTag = "SeedSpawn";
    public Transform[] spawnPoints;
    public float spawnRadius = 0.5f;
    public float spawnHeight = 0.35f;

    [Header("Respawn")]
    public bool respawnSeeds = true;
    public float respawnDelay = 2f;

    private readonly List<Seed> activeSeeds = new List<Seed>();
    private readonly List<Collider> spawnAreaColliders = new List<Collider>();
    private int nextSpawnPointIndex;

    private void Start()
    {

        CacheTaggedSpawnAreas();

        int desiredCount = Mathf.Max(0, targetSeedCount);
        for (int i = 0; i < desiredCount; i++)
        {
            SpawnSeed();
        }
    }

    private void Update()
    {
        int consumedCount = RemoveMissingSeeds();
        if (!respawnSeeds || consumedCount <= 0)
        {
            return;
        }

        for (int i = 0; i < consumedCount; i++)
        {
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private int RemoveMissingSeeds()
    {
        int removed = 0;

        for (int i = activeSeeds.Count - 1; i >= 0; i--)
        {
            if (activeSeeds[i] == null)
            {
                activeSeeds.RemoveAt(i);
                removed++;
            }
        }

        return removed;
    }

    private IEnumerator RespawnAfterDelay()
    {
        if (respawnDelay > 0f)
        {
            yield return new WaitForSeconds(respawnDelay);
        }

        SpawnSeed();
    }

    private void SpawnSeed()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Seed spawnedSeed = Instantiate(seedPrefab, spawnPosition, spawnRotation);
        activeSeeds.Add(spawnedSeed);
    }

    private Vector3 GetSpawnPosition()
    {
        if (TryGetTaggedSpawnPosition(out Vector3 taggedPosition))
        {
            return taggedPosition;
        }

        Transform point = GetNextSpawnPoint();
        if (point != null)
        {
            return point.position;
        }

        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        return transform.position + new Vector3(randomOffset.x, spawnHeight, randomOffset.y);
    }

    private Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int index = nextSpawnPointIndex % spawnPoints.Length;
            nextSpawnPointIndex++;

            if (spawnPoints[index] != null)
            {
                return spawnPoints[index];
            }
        }

        return null;
    }

    private void CacheTaggedSpawnAreas()
    {
        spawnAreaColliders.Clear();

        if (!useTaggedSpawnAreas || string.IsNullOrWhiteSpace(spawnAreaTag))
        {
            return;
        }

        GameObject[] taggedAreas;
        try
        {
            taggedAreas = GameObject.FindGameObjectsWithTag(spawnAreaTag);
        }
        catch (UnityException)
        {
            Debug.LogWarning($"Spawn tag '{spawnAreaTag}' is not defined. SeedSpawner will use spawn points or radius fallback.");
            return;
        }

        for (int i = 0; i < taggedAreas.Length; i++)
        {
            Collider[] colliders = taggedAreas[i].GetComponentsInChildren<Collider>();
            for (int j = 0; j < colliders.Length; j++)
            {
                if (colliders[j] != null)
                {
                    spawnAreaColliders.Add(colliders[j]);
                }
            }
        }

        if (spawnAreaColliders.Count == 0)
        {
            Debug.LogWarning($"No colliders found on objects tagged '{spawnAreaTag}'. SeedSpawner will use spawn points or radius fallback.");
        }
    }

    private bool TryGetTaggedSpawnPosition(out Vector3 position)
    {
        position = default;

        if (!useTaggedSpawnAreas)
        {
            return false;
        }

        spawnAreaColliders.RemoveAll(collider => collider == null);
        if (spawnAreaColliders.Count == 0)
        {
            CacheTaggedSpawnAreas();
        }

        if (spawnAreaColliders.Count == 0)
        {
            return false;
        }

        Collider area = spawnAreaColliders[Random.Range(0, spawnAreaColliders.Count)];
        Bounds bounds = area.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        float y = bounds.min.y + spawnHeight;

        position = new Vector3(x, y, z);
        return true;
    }
}
