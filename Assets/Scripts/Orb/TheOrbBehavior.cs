using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheOrbBehavior : MonoBehaviour {
    public GameBrain gameBrain;
    public GameObject orbInstructionGraphics;
    public Transform playerTransform;
    public float rotateSpeed = 100.0f;
    // For use in editor and testing to ignore the instruction text
    public bool usesOrbInstructionText = true;

    private Transform mainCamTransform;
    private MeshRenderer meshRenderer;

    void Start() {
        mainCamTransform = Camera.main.transform;

        meshRenderer = GetComponent<MeshRenderer>();

        if (usesOrbInstructionText) {
            StartCoroutine(DisplayOrbInstructionCo());
            orbInstructionGraphics.SetActive(true);
        }
        else {
            // if the text exists
            if (orbInstructionGraphics != null)
                orbInstructionGraphics.SetActive(false);
        }
    }

    void Update() {
        // Rotate the orb constantly
        transform.Rotate(Vector3.right * Time.deltaTime * rotateSpeed, Space.World);
    }
    void OnTriggerEnter2D(Collider2D coll) {
        // If the player comes into contact with the orb...
        if (coll.gameObject.layer == LayerMask.NameToLayer("Player")) {
            gameBrain.PlayerPickedUpOrb();
            // After sending signal, disable the mesh renderer to make the sphere invisible and then disable this script
            meshRenderer.enabled = false;
            enabled = false;
        }
    }

    IEnumerator DisplayOrbInstructionCo() {
        // Display instructions until the camera reaches the player
        while (!PlayerVisibleByCamera())  {
            yield return null;
        }
        orbInstructionGraphics.SetActive(false);
    }

    // Checks if the center of the player's transform is within the bounds of the screen
    private bool PlayerVisibleByCamera() {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(playerTransform.position);
        return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }
}
