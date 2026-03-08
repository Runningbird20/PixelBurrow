using UnityEngine;
using System.Collections.Generic;

public class ZonePlantSpawner : MonoBehaviour
{
    [Header("Plant Prefabs")]
    public GameObject[] plantPrefabs;

    [Header("Spawn Settings")]
    public int plantCount = 10;
    public float yOffset = 0f;
    public bool spawnOnStart = false;
    public LayerMask groundMask = ~0;
    public float raycastStartHeight = 10f;
    public bool useTaggedSpawnAreas = true;
    public string spawnAreaTag = "PlantSpawnZone";

    [Header("Placement")]
    public int maxAttemptsPerPlant = 20;
    public int rotationChecksPerSpot = 8;

    [Header("Random Scale")]
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    [Header("Parent")]
    public Transform plantParent;

    private bool hasSpawned = false;
    private Collider spawnAreaCollider;
    private readonly List<Collider> spawnedPlantColliders = new List<Collider>();

    private void Awake()
    {
        spawnAreaCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnPlants();
        }
    }

    public void SpawnPlants()
    {
        if (hasSpawned) return;

        if (plantPrefabs == null || plantPrefabs.Length == 0)
        {
            Debug.LogWarning($"ZonePlantSpawner '{name}' has no plant prefabs assigned.");
            return;
        }

        if (spawnAreaCollider == null)
        {
            Debug.LogWarning($"ZonePlantSpawner '{name}' needs a collider on the same object.");
            return;
        }

        hasSpawned = true;
        spawnedPlantColliders.Clear();

        Transform parentToUse = plantParent != null ? plantParent : transform;

        for (int i = 0; i < plantCount; i++)
        {
            GameObject prefab = plantPrefabs[Random.Range(0, plantPrefabs.Length)];
            if (prefab == null)
            {
                continue;
            }

            GameObject plant = Instantiate(prefab, Vector3.zero, Quaternion.identity, parentToUse);
            float randomScale = Random.Range(scaleRange.x, scaleRange.y);
            plant.transform.localScale *= randomScale;

            Collider[] plantColliders = GetSolidPlantColliders(plant);

            bool foundValidSpot = false;
            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;

            for (int attempt = 0; attempt < maxAttemptsPerPlant; attempt++)
            {
                if (!TryGetSpawnPosition(out Vector3 candidatePos))
                {
                    continue;
                }

                if (TryFindFittingRotation(plant, plantColliders, candidatePos, out Quaternion candidateRot))
                {
                    spawnPos = candidatePos;
                    spawnRot = candidateRot;
                    foundValidSpot = true;
                    break;
                }
            }

            if (!foundValidSpot)
            {
                Destroy(plant);
                Debug.Log($"Skipped plant {i + 1}, could not find a non-colliding placement.");
                continue;
            }

            plant.transform.SetPositionAndRotation(spawnPos, spawnRot);
            RegisterPlantColliders(plantColliders);
        }
    }

    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;

        Bounds bounds = spawnAreaCollider.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        float rayStartY = bounds.max.y + raycastStartHeight;
        Vector3 rayStart = new Vector3(x, rayStartY, z);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastStartHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            spawnPosition = new Vector3(x, hit.point.y + yOffset, z);
            return true;
        }

        spawnPosition = new Vector3(x, bounds.min.y + yOffset, z);
        return true;
    }

    private bool TryFindFittingRotation(GameObject candidatePlant, Collider[] candidateColliders, Vector3 candidatePos, out Quaternion chosenRotation)
    {
        int checks = Mathf.Max(1, rotationChecksPerSpot);
        float yawStep = 360f / checks;
        float startYaw = Random.Range(0f, 360f);

        for (int i = 0; i < checks; i++)
        {
            Quaternion testRotation = Quaternion.Euler(0f, startYaw + (i * yawStep), 0f);
            candidatePlant.transform.SetPositionAndRotation(candidatePos, testRotation);
            Physics.SyncTransforms();

            if (!HasPlantCollision(candidateColliders))
            {
                chosenRotation = testRotation;
                return true;
            }
        }

        chosenRotation = Quaternion.identity;
        return false;
    }

    private bool HasPlantCollision(Collider[] candidateColliders)
    {
        for (int i = spawnedPlantColliders.Count - 1; i >= 0; i--)
        {
            if (spawnedPlantColliders[i] == null)
            {
                spawnedPlantColliders.RemoveAt(i);
            }
        }

        for (int i = 0; i < candidateColliders.Length; i++)
        {
            Collider candidate = candidateColliders[i];
            if (candidate == null || !candidate.enabled || candidate.isTrigger)
            {
                continue;
            }

            for (int j = 0; j < spawnedPlantColliders.Count; j++)
            {
                Collider other = spawnedPlantColliders[j];
                if (other == null || !other.enabled || other.isTrigger)
                {
                    continue;
                }

                if (Physics.ComputePenetration(
                    candidate, candidate.transform.position, candidate.transform.rotation,
                    other, other.transform.position, other.transform.rotation,
                    out _, out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void RegisterPlantColliders(Collider[] plantColliders)
    {
        for (int i = 0; i < plantColliders.Length; i++)
        {
            Collider col = plantColliders[i];
            if (col == null || !col.enabled || col.isTrigger)
            {
                continue;
            }

            spawnedPlantColliders.Add(col);
        }
    }

    private Collider[] GetSolidPlantColliders(GameObject plantObject)
    {
        Collider[] allColliders = plantObject.GetComponentsInChildren<Collider>();
        if (allColliders == null || allColliders.Length == 0)
        {
            return new Collider[0];
        }

        List<Collider> solidColliders = new List<Collider>(allColliders.Length);
        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            if (col == null || !col.enabled || col.isTrigger)
            {
                continue;
            }

            solidColliders.Add(col);
        }

        return solidColliders.ToArray();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
#endif
}
