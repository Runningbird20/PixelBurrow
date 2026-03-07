using UnityEngine;

public class WindController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float acceleration = 4f;
    public float maxHeight = 6f;
    public float minHeight = 0f;

    [Header("Carry")]
    public Transform carryPoint;
    public float dropForce = 1.5f;
    public float pickupRadius = 4f;
    public Vector3 pickupOffset = new Vector3(0f, -2f, 0f);
    public LayerMask pickupMask = ~0;

    [Header("FX")]
    public ParticleSystem windTrail;

    private Seed carriedItem;
    private Vector3 velocity;

    private Camera mainCamera;
    private readonly Collider[] pickupHits = new Collider[16];

    private void Start()
    {
        mainCamera = Camera.main;

        if (carryPoint == null)
        {
            carryPoint = transform;
        }
    }

    private void Update()
    {
        UpdateMovement();
        TryPickupNearbyItem();
        HandleCarryInput();
        UpdateEffects();
    }

    private void UpdateMovement()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if (input.sqrMagnitude < 0.01f && Input.mousePresent)
        {
            input = MouseSteeringInput();
        }

        Vector3 targetVelocity = Vector3.ClampMagnitude(input, 1f) * speed;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * acceleration);

        transform.position += velocity * Time.deltaTime;

        Vector3 clampedPosition = transform.position;
        clampedPosition.y = 0;
        transform.position = clampedPosition;

        if (velocity.sqrMagnitude > 0.01f)
        {
            Vector3 lookDirection = new Vector3(velocity.x, 0f, velocity.z);
            transform.forward = Vector3.Slerp(transform.forward, lookDirection.normalized, Time.deltaTime * 8f);
        }
    }

    private Vector3 MouseSteeringInput()
    {
        if (mainCamera == null)
        {
            return Vector3.zero;
        }

        Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (!plane.Raycast(mouseRay, out float distance))
        {
            return Vector3.zero;
        }

        Vector3 hitPoint = mouseRay.GetPoint(distance);
        Vector3 direction = hitPoint - transform.position;
        direction.y = 0f;

        return direction.normalized;
    }

    private void TryPickupNearbyItem()
    {
        if (carriedItem != null)
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

            TryPickup(item);
            break;
        }
    }

    private void HandleCarryInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && carriedItem != null)
        {
            Vector3 dropDirection = transform.forward + Vector3.up * 0.6f;

            carriedItem.transform.parent = null;
            carriedItem.transform.position = transform.position + transform.forward + Vector3.up * 0.5f;
            carriedItem.Drop(dropForce, dropDirection);
            carriedItem = null;
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
        if (carriedItem != null)
        {
            return;
        }

        Seed item = GetSeedFromCollider(other);

        if (item != null && !item.isCarried)
        {
            TryPickup(item);
        }
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

    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.55f, 0.9f, 1f, 0.7f);
            Gizmos.DrawWireSphere(transform.position + pickupOffset, pickupRadius);
        }
    #endif
}
