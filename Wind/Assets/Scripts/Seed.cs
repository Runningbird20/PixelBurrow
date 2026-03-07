using UnityEngine;

public class Seed : MonoBehaviour
{
    public bool isCarried = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PickUp()
    {
        isCarried = true;
        // this.GetComponent<AudioSource>().Play();
        rb.isKinematic = true;
    }

    public void Drop()
    {
        isCarried = false;
        rb.isKinematic = false;
    }
}
