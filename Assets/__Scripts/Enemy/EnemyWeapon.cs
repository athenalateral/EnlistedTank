using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float fireRate = 1f;

    public float damage = 1f;

    protected Transform hero;
    protected float fireCooldown;

    protected virtual void Start()
    {
        if (Hero.S != null)
        {
            hero = Hero.S.transform;
        }
        else
        {
            Debug.LogWarning("Hero reference not found!");
        }
    }

    protected virtual void Update()
    {
        HandleShooting();
    }

    protected virtual void HandleShooting()
    {
        if (hero == null) return;

        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            FireAtHero();
            fireCooldown = 1f / fireRate;
        }
    }

    protected virtual void FireAtHero()
    {
        Vector3 direction = (hero.position - transform.position).normalized;

        direction = (hero.position - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);

        GameObject proj = Instantiate(projectilePrefab, transform.position, rot);

        ProjectileEnemy p = proj.GetComponent<ProjectileEnemy>();
        if (p != null)
        {
            p.vel = direction * projectileSpeed;
        }

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }
    }
}
