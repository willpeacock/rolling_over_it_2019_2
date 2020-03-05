using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.U2D;

public class CannonBehavior : MonoBehaviour {
    [Range(0.5f,5.0f)]
    public float timeBetweenFiring = 2.0f;
    public GameObject bulletObject;
    public Transform bulletSpawnPosition;
    public SpriteShapeRenderer cannonColorSR;
    public float bulletFireForce = 50.0f;

    private Transform playerTransform;
    private Animator cannonAnim;
    private AudioSource shootNoise;
    private bool fireBulletCoOn = true;
    void Start() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            playerTransform = player.transform;
        }
        else {
            Debug.LogError("Cannon requires a player object that is tagged 'Player' on its root object");
        }
        cannonAnim = GetComponent<Animator>();
        shootNoise = GetComponent<AudioSource>();
        StartCoroutine(FireBulletCo());
    }

    void Update() {
        // This whole block of code determines how the cannon aims for the player
        // It was adjusted manually until it worked well for a set bullet speed, this is not ideal
        Vector3 playerPosition = playerTransform.position;
        float distanceToPlayer = Mathf.Abs(playerPosition.x-transform.position.x);
        float yOffset = distanceToPlayer / Mathf.Clamp(1.0f / Mathf.Pow(0.07f * distanceToPlayer, 2), 1f, 10.0f);
        Vector2 targetLocation = new Vector2(playerPosition.x, playerPosition.y+yOffset);

        // Rotate the cannon based on the player's location
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(targetLocation.y - transform.position.y, targetLocation.x - transform.position.x) * Mathf.Rad2Deg);

        // Only fire bullets when the cannon is visible to the camera
        if (!fireBulletCoOn && VisibleByCamera()) {
            fireBulletCoOn = true;
            StartCoroutine(FireBulletCo());
        }
    }

    public void FireBullet() {
        // Create a new bullet object, set its color based off of the cannon, and set its initial speed
        BulletBehavior newBullet = Instantiate(bulletObject, bulletSpawnPosition.position, transform.rotation).GetComponent<BulletBehavior>();
        newBullet.SetBulletColor(cannonColorSR.color);
        newBullet.FireBullet(bulletFireForce);

        // Add variation in the shooting noise to avoid repetition in sound
        shootNoise.pitch = Random.Range(0.8f, 1.2f);
        shootNoise.Play();
    }

    // Checks if the center of the cannon's transform is within the bounds of the screen
    private bool VisibleByCamera() {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }

    IEnumerator FireBulletCo() {
        // While the cannon is still visible, fire bullets at a set pace
        // When the camera leaves view, the cannon might still fire off one more shot before stopping
        while (VisibleByCamera()) {
            yield return new WaitForSeconds(timeBetweenFiring);
            cannonAnim.Play("cannon_fire");
        }
        fireBulletCoOn = false;
    }
}
