using UnityEngine;

public class WindController : MonoBehaviour
{
    public float speed = 6f;
    public float smooth = 5f;

    Vector3 velocity;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v).normalized * speed;

        velocity = Vector3.Lerp(velocity, move, smooth * Time.deltaTime);

        transform.Translate(velocity * Time.deltaTime, Space.World);
    }
}