using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float fallSpeed = 20f;
    public float explosionRadius = 3f;
    public float damage = 10f;

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // should use boundary script
        if (transform.position.y < -20f)
        {
            Explode();
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        Explode();
    }

    void Explode()
    {
        // TODO: Add explosion VFX

        // damage player if nearby
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider c in hits)
        {
            if (c.CompareTag("Player"))
            {
                Debug.Log("Player hit by bomb!");
            }
        }

        Destroy(gameObject);
    }
}