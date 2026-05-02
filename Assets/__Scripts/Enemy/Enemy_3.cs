using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_3 : Enemy
{
    [Header("Movement")]
    public float lifeTime = 5f;
    public Vector2 midpointYRange = new Vector2(1.5f, 3f);
    public bool drawDebugInfo = true;

    [Header("Strike Attack")]
    public GameObject warningCirclePrefab;
    public GameObject bombPrefab;
    public GameObject explosionPrefab;

    public float strikeRate = 3f;      // time between attacks
    public float strikeDelay = 1.2f;   // warning time / bomb travel time
    public float strikeRadius = 2.5f;
    public float strikeDamage = 1f;

    [Tooltip("How far ahead on the Bezier path to target")]
    public float futurePathOffset = 0.18f;

    [Header("Private Fields")]
    [SerializeField] private Vector3[] points;
    [SerializeField] private float birthTime;

    private float lastStrikeTime;

    void Start()
    {
        points = new Vector3[3];

        points[0] = pos;

        float xMin = -bndCheck.camWidth + bndCheck.radius;
        float xMax = bndCheck.camWidth - bndCheck.radius;

        points[1] = Vector3.zero;
        points[1].x = Random.Range(xMin, xMax);

        float midYMult = Random.Range(midpointYRange.x, midpointYRange.y);
        points[1].y = -bndCheck.camHeight * midYMult;

        points[2] = Vector3.zero;
        points[2].y = pos.y;
        points[2].x = Random.Range(xMin, xMax);

        birthTime = Time.time;

        if (drawDebugInfo) DrawDebug();
    }

    void Update()
    {
        Move();

        if (Time.time - lastStrikeTime >= strikeRate)
        {
            BeginStrike();
            lastStrikeTime = Time.time;
        }

        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offDown))
        {
            Destroy(gameObject);
        }
    }

    public override void Move()
    {
        float u = (Time.time - birthTime) / lifeTime;

        if (u > 1f)
        {
            Destroy(gameObject);
            return;
        }

        transform.rotation = Quaternion.Euler(u * 180f, 0f, 0f);

        u = u - 0.1f * Mathf.Sin(u * Mathf.PI * 2f);

        pos = Utils.Bezier(u, points);
    }

    // =========================================================
    // ATTACK
    // =========================================================
    void BeginStrike()
    {
        float uNow = (Time.time - birthTime) / lifeTime;
        float futureU = Mathf.Clamp01(uNow + futurePathOffset);

        Vector3 strikePos = Utils.Bezier(futureU, points);

        GameObject warning = Instantiate(warningCirclePrefab);
        warning.transform.position = strikePos;

        StartCoroutine(BombStrike(strikePos, warning));
    }

    IEnumerator BombStrike(Vector3 strikePos, GameObject warning)
    {
        GameObject bomb = Instantiate(bombPrefab);

        Vector3 bombStart = transform.position;
        bomb.transform.position = bombStart;

        float t = 0f;

        while (t < strikeDelay)
        {
            t += Time.deltaTime;

            float u = t / strikeDelay;

            // move bomb toward strike zone
            bomb.transform.position = Vector3.Lerp(bombStart, strikePos, u);

            // make warning pulse
            if (warning != null)
            {
                float baseSize = strikeRadius * 2f;
                float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.15f;
                warning.transform.localScale = Vector3.one * baseSize * pulse;
            }

            yield return null;
        }

        if (bomb != null) Destroy(bomb);
        if (warning != null) Destroy(warning);

        yield return StartCoroutine(ExplosionFX(strikePos));

        DamagePlayerInZone(strikePos);
    }

    IEnumerator ExplosionFX(Vector3 strikePos)
    {
        GameObject exp = Instantiate(explosionPrefab);
        exp.transform.position = strikePos;

        float t = 0f;
        float duration = 0.45f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float u = t / duration;

            float scale;

            if (u < 0.5f)
                scale = Mathf.Lerp(0.2f, strikeRadius * 2f, u * 2f);
            else
                scale = Mathf.Lerp(strikeRadius * 2f, 0.1f, (u - 0.5f) * 2f);

            exp.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(exp);
    }

    void DamagePlayerInZone(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, strikeRadius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Hero hero = hit.GetComponent<Hero>();

                if (hero == null)
                    hero = hit.GetComponentInParent<Hero>();

                if (hero != null)
                    hero.TakeDamage(strikeDamage);
            }
        }
    }

    // =========================================================
    // DEBUG
    // =========================================================
    void DrawDebug()
    {
        Debug.DrawLine(points[0], points[1], Color.cyan, lifeTime);
        Debug.DrawLine(points[1], points[2], Color.yellow, lifeTime);

        float numSections = 20f;
        Vector3 prevPoint = points[0];

        for (int i = 1; i < numSections; i++)
        {
            float u = i / numSections;
            Vector3 pt = Utils.Bezier(u, points);

            Color col = Color.Lerp(Color.cyan, Color.yellow, u);

            Debug.DrawLine(prevPoint, pt, col, lifeTime);
            prevPoint = pt;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, strikeRadius);
    }
}