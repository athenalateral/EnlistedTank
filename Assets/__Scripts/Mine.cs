using UnityEngine;

public class Mine : MonoBehaviour
{
    public float speed = 3f;
    public float damage = 1f;

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);

        if (transform.position.y < -10f)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Mine hit: " + other.name);

        // Hit Player
        if (other.CompareTag("Player"))
        {
            Hero hero = other.GetComponent<Hero>();

            if (hero != null)
                hero.TakeDamage(damage);

            Destroy(gameObject);
        }

        // Hit by Player Projectile
        if (other.CompareTag("ProjectileHero"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}