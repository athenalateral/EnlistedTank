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
        
        // Spawn warning circle
        GameObject warning = Instantiate(warningCirclePrefab);
        warning.transform.position = strikePos;
        WarningCircle warningScript = warning.GetComponent<WarningCircle>();
        if(warningScript != null){
            warningScript.strikeDelay = strikeDelay;
            warningScript.strikeRadius = strikeRadius;
        }

        // Spawn bomb
        GameObject bomb = Instantiate(bombPrefab);
        Vector3 bombStart = transform.position;
        bomb.transform.position = bombStart;
        Bomb bombScript = bomb.GetComponent<Bomb>();
        if(bombScript != null){
            bombScript.strikePos = strikePos;
            bombScript.strikeDamage = strikeDamage;
            bombScript.strikeRadius = strikeRadius;
            bombScript.strikeDelay = strikeDelay;
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