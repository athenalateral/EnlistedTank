using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeFromBlack : MonoBehaviour{

    private Image fadeOverlayImage;

    [Header("Inscribed")]

    // Seconds it takes to fade from black
    public float fadeInTime = 0.75f;

    private bool fadeInStarted = false;

    void Start(){
        // The image should start out fully opaque
        fadeOverlayImage = this.GetComponent<Image>();
        if(fadeOverlayImage != null){
            fadeOverlayImage.enabled = true;
            Color overlayColor = fadeOverlayImage.color;
            fadeOverlayImage.color = new Color(
                overlayColor.r,
                overlayColor.g,
                overlayColor.b,
                1.0f
            );
        }
    }

    // Update is called once per frame
    void Update(){
        // use deltaTime to calculate how much to decrease the alpha by each frame
        if(fadeInStarted && fadeOverlayImage.color.a > 0.0f){
            Color overlayColor = fadeOverlayImage.color;
            float alpha = overlayColor.a;
            alpha -= Time.deltaTime / fadeInTime;
            if(alpha <= 0.0f){
                alpha = 0.0f;
                fadeOverlayImage.enabled = false;
            }
            fadeOverlayImage.color = new Color(
                overlayColor.r,
                overlayColor.g,
                overlayColor.b,
                alpha
            );
        }
    }

    // public funciton for fading from black
    public void startFade(){
        fadeInStarted = true;
    }
}
