using UnityEngine;

public class SeekingMissile : MonoBehaviour
{
    public float speed = 50f;
    public float turnSpeed = 240f;

    private Transform target;
    private ProjectileHero proj;

    void Awake()
    {
        proj = GetComponent<ProjectileHero>();
        Debug.Log("[MISSILE] Awake -> ProjectileHero found: " + (proj != null));
    }

    void Start()
    {
        Debug.Log("[MISSILE] Start -> Searching for first target...");
        FindNearestEnemy();
    }

    void Update()
    {
        if (target == null)
        {
            Debug.Log("[MISSILE] No target. Reacquiring...");
            FindNearestEnemy();
        }

        if (target != null)
        {
            Vector3 dir = target.position - transform.position;

            float desiredAngle =
                Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

            float currentAngle = transform.eulerAngles.z;

            float newAngle = Mathf.MoveTowardsAngle(
                currentAngle,
                desiredAngle,
                turnSpeed * Time.deltaTime
            );

            Debug.Log(
                "[MISSILE] Tracking: " + target.name +
                " | Pos: " + target.position +
                " | CurrentAngle: " + currentAngle.ToString("F1") +
                " | DesiredAngle: " + desiredAngle.ToString("F1") +
                " | NewAngle: " + newAngle.ToString("F1")
            );

            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        else
        {
            Debug.Log("[MISSILE] No valid target found.");
        }

        proj.vel = transform.up * speed;

        Debug.Log("[MISSILE] Velocity: " + proj.vel);
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Debug.Log("[MISSILE] Enemies found: " + enemies.Length);

        float best = Mathf.Infinity;
        target = null;

        foreach (GameObject e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);

            Debug.Log("[MISSILE] Candidate: " + e.name +
                      " | Distance: " + dist.ToString("F2"));

            if (dist < best)
            {
                best = dist;
                target = e.transform;
            }
        }

        if (target != null)
        {
            Debug.Log("[MISSILE] Target locked: " + target.name +
                      " | Distance: " + best.ToString("F2"));
        }
        else
        {
            Debug.Log("[MISSILE] No enemies available to lock.");
        }
    }
}