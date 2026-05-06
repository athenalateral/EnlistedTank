using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject explosionPrefab;

    [Header("Dynamic")]
    public float strikeDelay;
    public float strikeRadius;
    public float strikeDamage;
    public Vector3 strikePos;

    private Vector3 bombStart;

    void Start(){
        bombStart = this.transform.position;
    }

    void Update()
    {
        StartCoroutine(BombUpdate());
    }

    IEnumerator BombUpdate()
    {
        float t = 0f;

        while (t < strikeDelay)
        {
            t += Time.deltaTime;

            float u = t / strikeDelay;

            // move bomb toward strike zone
            this.transform.position = Vector3.Lerp(bombStart, strikePos, u);

            yield return null;
        }

        DamagePlayerInZone();

        // Spawn explosion effect
        GameObject exp = Instantiate(explosionPrefab);
        exp.transform.position = strikePos;
        Explosion expScript = exp.GetComponent<Explosion>();
        if(expScript != null){
            expScript.strikeRadius = strikeRadius;
        }

        Destroy(gameObject);
    }

    void DamagePlayerInZone()
    {
        Collider[] hits = Physics.OverlapSphere(strikePos, strikeRadius);

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
}