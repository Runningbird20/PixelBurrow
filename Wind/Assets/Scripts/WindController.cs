using UnityEngine;

public class WindController : MonoBehaviour
{
    public float speed = 5f;
    public Transform carryPoint;

    private Seed carriedSeed;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v);
        transform.position += move * speed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && carriedSeed != null)
        {
            carriedSeed.isCarried = false;
            carriedSeed.transform.parent = null;
            carriedSeed.transform.position = transform.position + transform.forward + Vector3.up * 0.5f;
            carriedSeed = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Touched: " + other.name);

        if (carriedSeed != null) return;

        Seed seed = other.GetComponent<Seed>();

        if (seed != null && !seed.isCarried)
        {
            carriedSeed = seed;
            seed.isCarried = true;
            seed.transform.parent = carryPoint;
            seed.transform.localPosition = Vector3.zero;
        }
    }
}