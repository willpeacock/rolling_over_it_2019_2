using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour {
    // The name of whatever level you want the play button to launch
    public string levelName = "SampleLevel";
    public AudioSource mainMenuMusic;

    private bool fadeMusicCoOn = false;

    public void BeginLevel() {
        // First fade out the music to prevent audio clipping sounds
        if (!fadeMusicCoOn) {
            fadeMusicCoOn = true;
            StartCoroutine(FadeMainMenuMusicThenBeginLevel());
        }
    }

    public void ExitGame() {
        // Currently prevents the button from activating when a level is being started
        if (!fadeMusicCoOn)
            Application.Quit();
    }

    IEnumerator FadeMainMenuMusicThenBeginLevel() {
        while (mainMenuMusic.volume > 0) {
            mainMenuMusic.volume -= Time.deltaTime * 5.0f;
            yield return null;
        }
        mainMenuMusic.volume = 0;
        mainMenuMusic.Stop();
        // Loading a scene might take some time
        SceneManager.LoadScene(levelName);
    }
}
