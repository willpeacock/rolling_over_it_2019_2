using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameBrain : MonoBehaviour {
    public PlayerController playerController;
    // Should at least contain the player
    // Simply moves all of these transforms up with the game ends
    public Transform[] elevatorTransforms;
    public PlayerInput playerInput;
    // This should be the main virtual camera that follows the player in the game
    public CinemachineVirtualCamera followCam;
    // This should be the transform of the actual orb sphere
    public Transform orbSphereTransform;
    public string menuSceneName = "MainMenu";

    private Rigidbody2D playerRB;

    void Start() {
        playerRB = playerController.GetComponent<Rigidbody2D>();
    }

    void Update() {
        // Hitting the designated menu button can bring back the menu at any time
        if (playerInput.GetMenuButtonDown()) {
            GoToMenu();
        }
    }

    private void GoToMenu() {
        // Load the current active scene again
        SceneManager.LoadScene(menuSceneName);
    }

    public void PlayerPickedUpOrb() {
        // Disable the player's movement and physics, and freeze the camera
        playerController.SetPlayerCanMove(false);
        playerController.StopRollingSoundIfNeeded();
        playerRB.velocity = Vector2.zero;
        playerRB.isKinematic = true;
        followCam.m_Follow = null;

        StartCoroutine(EndGame());
    }

    IEnumerator EndGame() {
        // Move the player over towards the orb's center while waiting for the two second counter to finish
        Transform playerTransform = playerRB.transform;
        float counter = 0.0f;
        while (counter < 2.0f) {
            playerTransform.position = Vector3.MoveTowards(playerTransform.position, orbSphereTransform.position, Time.deltaTime * 2.0f);
            counter += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(2.0f);
        // Elevate the 'elevator transforms' for the given time
        counter = 0;
        while (counter < 5.0f) {
            foreach(Transform elevatorTransform in elevatorTransforms) {
                elevatorTransform.Translate(Vector2.up * Time.deltaTime * 4.0f, Space.World);
            }
            counter += Time.deltaTime;
            yield return null;
        }

        GoToMenu();
    }
}
