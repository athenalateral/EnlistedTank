using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{   
    [Header("Inscribed")]
    public GameObject blackFadeOverlay;
    AudioSource startSound;
    bool startSoundWasStarted = false;

    // Ensure the overlay entity is active
    // (is inactive by default so it isn't in the way in the editor)
    void Awake(){
        if(blackFadeOverlay == null) return;
        blackFadeOverlay.SetActive(true);
    }

    // Start is called before the first frame update
    void Start(){
        // Make this button only clickable in areas where alpha >= 0.1
        Image button_image = this.GetComponent<Image>();
        if(button_image != null){
            button_image.alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    public void StartGame(){
        // Play the start sound when the button is pushed
        startSound = this.GetComponent<AudioSource>();
        if(startSound == null) return;

        // Stop any sounds from main if there are any playing
        AudioSource mainSound = Camera.main.GetComponent<AudioSource>();
        if(mainSound != null) mainSound.Stop();

        startSound.Play();
        startSoundWasStarted = true;

        // Make the screen fade to black
        FadeToBlack fadeToBlack = blackFadeOverlay.GetComponent<FadeToBlack>();
        if(fadeToBlack == null) return;
        fadeToBlack.startFade();
    }

    void Update(){
        // Only start the game when the start sound finishes
        if(startSoundWasStarted && !startSound.isPlaying){
            startSoundWasStarted = false;
            SceneManager.LoadScene("__Scene_0");
        }
    }
}
