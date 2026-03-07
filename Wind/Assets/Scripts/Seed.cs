using UnityEngine;

public class Seed : MonoBehaviour
{
    public enum NatureItemType
    {
        Seed,
        Petal,
        Cloud
    }

    [Header("Item Settings")]
    public NatureItemType itemType = NatureItemType.Seed;
    public bool isCarried = false;

    [Header("Carry Visual")]
    public Vector3 carryLocalOffset = Vector3.zero;

    private Rigidbody rb;
    private Collider cachedCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();
    }

    public void PickUp()
    {
        isCarried = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }
        this.GetComponent<AudioSource>().Play();
        rb.isKinematic = true;
    }

    public void Drop(float launchForce = 1.5f, Vector3 launchDirection = default)
    {
        isCarried = false;

        if (cachedCollider != null)
        {
            cachedCollider.enabled = true;
        }

        if (rb != null)
        {
            rb.isKinematic = false;

            if (launchDirection == default)
            {
                launchDirection = Vector3.up;
            }

            rb.AddForce(launchDirection.normalized * launchForce, ForceMode.Impulse);
        }
    }
}
