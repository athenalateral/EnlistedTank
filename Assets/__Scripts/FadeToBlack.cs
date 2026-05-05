using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeToBlack : MonoBehaviour{

    private Image fadeOverlayImage;

    [Header("Inscribed")]

    // Seconds it takes to fade to black
    public float fadeOutTime = 0.75f;

    private bool fadeOutStarted = false;

    void Start(){
        // The image should start out fully transparent
        fadeOverlayImage = this.GetComponent<Image>();
        if(fadeOverlayImage != null){
            fadeOverlayImage.enabled = false;
            Color overlayColor = fadeOverlayImage.color;
            fadeOverlayImage.color = new Color(
                overlayColor.r,
                overlayColor.g,
                overlayColor.b,
                0.0f
            );
        }
    }

    // Update is called once per frame
    void Update(){
        // use deltaTime to calculate how much to increase the alpha by each frame
        if(fadeOutStarted && fadeOverlayImage.color.a < 1.0f){
            Color overlayColor = fadeOverlayImage.color;
            float alpha = overlayColor.a;
            alpha += Time.deltaTime / fadeOutTime;
            if(alpha >= 1.0f){
                alpha = 1.0f;
            }
            fadeOverlayImage.color = new Color(
                overlayColor.r,
                overlayColor.g,
                overlayColor.b,
                alpha
            );
        }
    }

    // public funciton for fading to black
    public void startFade(){
        fadeOverlayImage.enabled = true;
        fadeOutStarted = true;
    }
}
