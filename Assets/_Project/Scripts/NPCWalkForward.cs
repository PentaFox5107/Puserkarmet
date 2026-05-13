using UnityEngine;

public class NPCWalkForward : MonoBehaviour
{
    public float speed = 2f;
    public float lifetime = 10f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}