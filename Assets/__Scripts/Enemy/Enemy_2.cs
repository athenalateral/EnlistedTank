using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_2 : Enemy
{
    [Header("Enemy_2 Fields")]
    public float lifeTime = 10f;
    public float sinEccentricity = 0.6f;
    public AnimationCurve rotCurve;

    [Header("Mine Settings")]
    public GameObject minePrefab;
    public float mineDropRate = 0.5f;

    [SerializeField] private float birthTime;
    [SerializeField] private Vector3 p0, p1;

    private Quaternion baseRotation;
    private float nextMineTime;

    void Start()
    {
        p0 = Vector3.zero;
        p0.x = -bndCheck.camWidth - bndCheck.radius;
        p1.y = Random.Range(-bndCheck.camHeight, bndCheck.camHeight);

        if (Random.value > 0.5f)
        {
            p0.x *= -1;
            p1.x *= -1;
        }

        birthTime = Time.time;
        transform.position = p0;
        transform.LookAt(p1, Vector3.back);
        baseRotation = transform.rotation;

        nextMineTime = Time.time + mineDropRate;
    }

    public override void Move()
    {
        float u = (Time.time - birthTime) / lifeTime;

        if (u > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        float shipRot = rotCurve.Evaluate(u) * 360;
        transform.rotation = baseRotation * Quaternion.Euler(-shipRot, 0, 0);

        u = u + sinEccentricity * Mathf.Sin(u * Mathf.PI * 2);

        pos = (1 - u) * p0 + u * p1;

        DropMine();
    }

    void DropMine()
    {
        if (Time.time >= nextMineTime)
        {
            Instantiate(minePrefab, transform.position, Quaternion.identity);
            nextMineTime = Time.time + mineDropRate;
        }
    }
}