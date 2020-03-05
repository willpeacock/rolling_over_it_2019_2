using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.U2D;

public class BulletBehavior : MonoBehaviour {
    public BulletExplosionBehavior explosionBehavior;
    public LayerMask collideLayerMask;
    public Transform backOfBullet;
    public Rigidbody2D rb;
    public SpriteShapeRenderer bulletColorSR;
    public AudioSource explosionSound;

    private bool hasExploded = false;

    // Called once every frame, use for non-physics actions
    void Update() {
        // Get angle based off rigidbody's velocity
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        // Set the sprite's angle so it looks to be following the trajectory
        transform.GetChild(0).rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void FireBullet(float launchSpeed = 50.0f) {
        // transform.right is relative to the player, so this will always be 'forward' for our uses
        rb.AddForceAtPosition(transform.right * launchSpeed, backOfBullet.position, ForceMode2D.Impulse);
    }

    public void SetBulletColor(Color newColor) {
        bulletColorSR.color = newColor;
    }

    void OnTriggerEnter2D(Collider2D coll) {
        // If the object that the bullet collided with is within the specified layer mask...
        if (!hasExploded && collideLayerMask == (collideLayerMask | (1 << coll.gameObject.layer))) {
            explosionSound.pitch = Random.Range(0.8f, 1f);
            explosionSound.Play();
            StartCoroutine(PlayExplosionThenDie());
            hasExploded = true;
        }
    }

    IEnumerator PlayExplosionThenDie() {
        // disable physics/gravity on bullet
        rb.isKinematic = true;
        // reset velocity so explosion stays in place
        rb.velocity = Vector2.zero;
        // then disable sprites of bullet
        transform.GetChild(0).gameObject.SetActive(false);

        // then enable the explosion force and display the graphics
        explosionBehavior.gameObject.SetActive(true);

        // wait a bit for explosion force's effect
        yield return new WaitForSeconds(0.2f);

        // then disable its explosion
        explosionBehavior.gameObject.SetActive(false);

        // then wait until audio finishes
        while (explosionSound.isPlaying) {
            yield return null;
        }

        // and then destroy the gameObject
        GameObject.Destroy(gameObject, 0.5f);
    }
}
