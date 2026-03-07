using UnityEngine;

public class WindController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float acceleration = 4f;
    public float maxHeight = 6f;
    public float minHeight = 0f;
    private Rigidbody rb;

    [Header("Carry")]
    public Transform carryPoint;
    public float dropForce = 1.5f;
    public float pickupRadius = 0.5f;
    public Vector3 pickupOffset = Vector3.zero;
    public LayerMask pickupMask = ~0;
    private bool insideZone = false;

    [Header("FX")]
    public ParticleSystem windTrail;

    private Seed carriedItem;
    private Vector3 velocity;
    private float pickupCooldownTimer = 0f;
    public float pickupCooldownDuration = 0.5f;
    private Camera mainCamera;
    private readonly Collider[] pickupHits = new Collider[16];

    private void OnTriggerExit(Collider other)
    {
        if (IsRestoreZoneTrigger(other))
        {
            insideZone = false;
            Debug.Log("Exited restore zone");
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;

        rb = GetComponent<Rigidbody>();

        if (carryPoint == null)
        {
            carryPoint = transform;
        }
    }

    private void Update()
    {
        if (pickupCooldownTimer > 0f)
        {
            pickupCooldownTimer -= Time.deltaTime;
        }
        UpdateMovement();
        TryPickupNearbyItem();
        HandleCarryInput();
        UpdateEffects();
    }

    private void UpdateMovement()
    {
        Vector3 input = GetWasdInput();

        Vector3 targetVelocity = Vector3.ClampMagnitude(input, 1f) * speed;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * acceleration);

        if (velocity.sqrMagnitude > 0.01f)
        {
            Vector3 lookDirection = new Vector3(velocity.x, 0f, velocity.z);
            transform.forward = Vector3.Slerp(transform.forward, lookDirection.normalized, Time.deltaTime * 8f);
        }
    }

    private Vector3 GetWasdInput()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            x -= 1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            x += 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            z -= 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            z += 1f;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Vector3 cameraForward = Vector3.forward;
        Vector3 cameraRight = Vector3.right;

        if (mainCamera != null)
        {
            cameraForward = mainCamera.transform.forward;
            cameraRight = mainCamera.transform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            if (cameraForward.sqrMagnitude > 0.0001f)
            {
                cameraForward.Normalize();
            }
            else
            {
                cameraForward = Vector3.forward;
            }

            if (cameraRight.sqrMagnitude > 0.0001f)
            {
                cameraRight.Normalize();
            }
            else
            {
                cameraRight = Vector3.right;
            }
            if (velocity.sqrMagnitude > 0.01f)
            {
                Vector3 lookDirection = new Vector3(velocity.x, 0f, velocity.z);
                transform.forward = Vector3.Slerp(transform.forward, lookDirection.normalized, Time.deltaTime * 8f);
            }

            Vector3 worldMove = cameraRight * x + cameraForward * z;
            return Vector3.ClampMagnitude(worldMove, 1f);
        }
        return Vector3.zero;
    }

    private void TryPickupNearbyItem()
    {
        if (carriedItem != null || pickupCooldownTimer > 0f)
        {
            return;
        }

        Vector3 pickupCenter = transform.position + pickupOffset;
        int hits = Physics.OverlapSphereNonAlloc(
            pickupCenter,
            pickupRadius,
            pickupHits,
            pickupMask,
            QueryTriggerInteraction.Collide);

       for (int i = 0; i < hits; i++)
        {

            Seed item = GetSeedFromCollider(pickupHits[i]);
            if (item == null || item.isCarried)
            {
                continue;
            }

            Debug.Log("Trying to pick up: " + item.name);
            TryPickup(item);
            break;
        }
    }

    private void HandleCarryInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed | carriedItem: " + (carriedItem != null) + " | insideZone: " + insideZone);
        }

        if (Input.GetKeyDown(KeyCode.Space) && carriedItem != null && insideZone)
        {
            Vector3 dropDirection = transform.forward + Vector3.up * 0.6f;

            carriedItem.transform.parent = null;
            carriedItem.transform.position = transform.position + transform.forward + Vector3.up * 0.5f;
            carriedItem.Drop(dropForce, dropDirection);
            carriedItem = null;

            pickupCooldownTimer = pickupCooldownDuration;

            Debug.Log("Dropped seed in restore zone");
        }
    }

    private void UpdateEffects()
    {
        if (windTrail == null)
        {
            return;
        }

        var emission = windTrail.emission;
        emission.rateOverTime = Mathf.Lerp(8f, 35f, Mathf.Clamp01(velocity.magnitude / speed));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsRestoreZoneTrigger(other))
        {
            insideZone = true;
            Debug.Log("Entered restore zone");
        }
    }

    private bool IsRestoreZoneTrigger(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.GetComponent<RestoreZone>() != null)
        {
            return true;
        }

        return other.GetComponentInParent<RestoreZone>() != null;
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

    private void TryPickup(Seed item)
    {
        if (item == null || item.isCarried || carriedItem != null)
        {
            return;
        }

        if (!insideZone)
        {
        carriedItem = item;
        item.PickUp();
        item.transform.parent = carryPoint;
        item.transform.localPosition = item.carryLocalOffset;
        }
    }

    // Backward-compatible alias in case existing scene scripts/events still refer to the old method name.
    private void TryPickUp(Seed item)
    {
        TryPickup(item);
    }
    
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.55f, 0.9f, 1f, 0.7f);
            Gizmos.DrawWireSphere(transform.position + pickupOffset, pickupRadius);
        }
    #endif
}
