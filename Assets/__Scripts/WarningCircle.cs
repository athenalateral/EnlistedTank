using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningCircle : MonoBehaviour
{
    public float strikeDelay;   // warning time / bomb travel time
    public float strikeRadius;

    void Update(){
        StartCoroutine(WarningCircleUpdate());
    }

    IEnumerator WarningCircleUpdate()
    {
        float t = 0f;

        while (t < strikeDelay)
        {
            t += Time.deltaTime;

            float u = t / strikeDelay;

            // make warning pulse
            float baseSize = strikeRadius * 2f;
            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.15f;
            this.transform.localScale = Vector3.one * baseSize * pulse;

            yield return null;
        }

        Destroy(gameObject);
    }
}