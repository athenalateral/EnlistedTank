using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour{
    
    public float strikeRadius;

    // Update is called once per frame
    void Update(){
        StartCoroutine(ExplosionFX());
    }

    IEnumerator ExplosionFX()
    {
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

            this.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(gameObject);
    }
}
