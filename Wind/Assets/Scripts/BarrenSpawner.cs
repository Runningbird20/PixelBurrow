using System.Collections.Generic;
using UnityEngine;

public class BarrenSpawner : MonoBehaviour
{
    [Header("Rock Prefabs (Shape Variants)")]
    public GameObject[] rockPrefabs;

    [Header("Spawn Settings")]
    public int rockCount = 14;
    public float yOffset = 0f;
    public bool spawnOnStart = true;
    public LayerMask groundMask = ~0;
    public float raycastStartHeight = 10f;
    public int maxAttemptsPerRock = 20;
    public bool useTaggedSpawnAreas = true;
    public string spawnAreaTag = "BarrenZone";

    [Header("Rock Size")]
    public Vector2 uniformScaleRange = new Vector2(0.6f, 1.4f);
    public Vector2 horizontalStretchRange = new Vector2(0.85f, 1.2f);
    public Vector2 verticalStretchRange = new Vector2(0.75f, 1.15f);
    public float maxAllowedScaleAxis = 2.2f;

    [Header("Rock Look")]
    public Material[] rockMaterials;
    public bool randomizeTint = true;
    public Color minRockTint = new Color(0.35f, 0.35f, 0.35f, 1f);
    public Color maxRockTint = new Color(0.72f, 0.72f, 0.72f, 1f);

    [Header("Parent")]
    public Transform rockParent;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private bool hasSpawned;
    private Collider spawnAreaCollider;
    private readonly List<Collider> spawnedRockColliders = new List<Collider>();
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        spawnAreaCollider = GetComponent<Collider>();
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnRocks();
        }
    }

    public void SpawnRocks()
    {
        if (hasSpawned)
        {
            return;
        }

        if (rockPrefabs == null || rockPrefabs.Length == 0)
        {
            Debug.LogWarning($"BarrenSpawner '{name}' has no rock prefabs assigned.");
            return;
        }

        if (spawnAreaCollider == null)
        {
            Debug.LogWarning($"BarrenSpawner '{name}' needs a collider on the same object.");
            return;
        }

        hasSpawned = true;
        spawnedRockColliders.Clear();
        Transform parentToUse = rockParent != null ? rockParent : transform;

        for (int i = 0; i < rockCount; i++)
        {
            GameObject prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
            if (prefab == null)
            {
                continue;
            }

            GameObject rock = Instantiate(prefab, Vector3.zero, Quaternion.identity, parentToUse);
            ApplyRandomRockScale(rock.transform);
            ApplyRandomRockMaterialAndTint(rock);

            Collider[] rockColliders = GetSolidRockColliders(rock);
            bool placed = false;

            for (int attempt = 0; attempt < maxAttemptsPerRock; attempt++)
            {
                if (!TryGetSpawnPosition(out Vector3 candidatePos))
                {
                    continue;
                }

                Quaternion candidateRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                rock.transform.SetPositionAndRotation(candidatePos, candidateRot);
                Physics.SyncTransforms();

                if (!HasRockCollision(rockColliders))
                {
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                Destroy(rock);
                Debug.Log($"Skipped rock {i + 1}, could not find a valid non-overlapping spot.");
                continue;
            }

            RegisterRockColliders(rockColliders);
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

    private void ApplyRandomRockScale(Transform rockTransform)
    {
        float uniform = Random.Range(uniformScaleRange.x, uniformScaleRange.y);
        float stretchX = Random.Range(horizontalStretchRange.x, horizontalStretchRange.y);
        float stretchY = Random.Range(verticalStretchRange.x, verticalStretchRange.y);
        float stretchZ = Random.Range(horizontalStretchRange.x, horizontalStretchRange.y);

        Vector3 multiplier = new Vector3(stretchX, stretchY, stretchZ) * uniform;
        Vector3 scaled = Vector3.Scale(rockTransform.localScale, multiplier);

        float maxAxis = Mathf.Max(scaled.x, Mathf.Max(scaled.y, scaled.z));
        if (maxAxis > maxAllowedScaleAxis && maxAxis > 0f)
        {
            float factor = maxAllowedScaleAxis / maxAxis;
            scaled *= factor;
        }

        rockTransform.localScale = scaled;
    }

    private void ApplyRandomRockMaterialAndTint(GameObject rock)
    {
        Renderer[] renderers = rock.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        Material chosenMaterial = null;
        if (rockMaterials != null && rockMaterials.Length > 0)
        {
            chosenMaterial = rockMaterials[Random.Range(0, rockMaterials.Length)];
        }

        Color tint = Color.Lerp(minRockTint, maxRockTint, Random.value);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (chosenMaterial != null)
            {
                Material[] mats = renderer.sharedMaterials;
                if (mats != null && mats.Length > 0)
                {
                    for (int m = 0; m < mats.Length; m++)
                    {
                        mats[m] = chosenMaterial;
                    }

                    renderer.sharedMaterials = mats;
                }
                else
                {
                    renderer.sharedMaterial = chosenMaterial;
                }
            }

            if (!randomizeTint)
            {
                continue;
            }

            Material sourceMaterial = chosenMaterial != null ? chosenMaterial : renderer.sharedMaterial;
            if (sourceMaterial == null)
            {
                continue;
            }

            bool hasBaseColor = sourceMaterial.HasProperty(BaseColorId);
            bool hasLegacyColor = sourceMaterial.HasProperty(ColorId);
            if (!hasBaseColor && !hasLegacyColor)
            {
                continue;
            }

            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            propertyBlock.Clear();
            renderer.GetPropertyBlock(propertyBlock);

            if (hasBaseColor)
            {
                propertyBlock.SetColor(BaseColorId, tint);
            }

            if (hasLegacyColor)
            {
                propertyBlock.SetColor(ColorId, tint);
            }

            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private bool HasRockCollision(Collider[] candidateColliders)
    {
        for (int i = spawnedRockColliders.Count - 1; i >= 0; i--)
        {
            if (spawnedRockColliders[i] == null)
            {
                spawnedRockColliders.RemoveAt(i);
            }
        }

        for (int i = 0; i < candidateColliders.Length; i++)
        {
            Collider candidate = candidateColliders[i];
            if (candidate == null || !candidate.enabled || candidate.isTrigger)
            {
                continue;
            }

            for (int j = 0; j < spawnedRockColliders.Count; j++)
            {
                Collider other = spawnedRockColliders[j];
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

    private void RegisterRockColliders(Collider[] rockColliders)
    {
        for (int i = 0; i < rockColliders.Length; i++)
        {
            Collider col = rockColliders[i];
            if (col == null || !col.enabled || col.isTrigger)
            {
                continue;
            }

            spawnedRockColliders.Add(col);
        }
    }

    private Collider[] GetSolidRockColliders(GameObject rock)
    {
        Collider[] allColliders = rock.GetComponentsInChildren<Collider>();
        if (allColliders == null || allColliders.Length == 0)
        {
            return new Collider[0];
        }

        List<Collider> solid = new List<Collider>(allColliders.Length);
        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            if (col == null || !col.enabled || col.isTrigger)
            {
                continue;
            }

            solid.Add(col);
        }

        return solid.ToArray();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0.52f, 0.4f, 0.25f, 1f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
#endif
}
