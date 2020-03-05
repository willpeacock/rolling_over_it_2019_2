using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletExplosionBehavior : MonoBehaviour {
    public LayerMask collideLayerMask;
    // This is the highest possible strength of the bullet force at the center of the explosion
    // Adjusting this will currently mess up the cannon's aim
    public float maxForceStrength;

    private List<GameObject> hitObjects;
    private CircleCollider2D explosionColl;
    private float explosionRadius;
    private ExplosionManager explosionManager;
    
    private const float distanceFactorMultiplier = 10.0f;
    void Start() {
        hitObjects = new List<GameObject>();
        
        explosionColl = GetComponent<CircleCollider2D>();
        explosionRadius = explosionColl.radius;

        explosionManager = FindObjectOfType<ExplosionManager>();
        
        if (explosionManager == null) {
            Debug.LogError("Could not find object of type ExplosionManager in current scene");
        }
    }

    void OnTriggerEnter2D(Collider2D coll) {
        // if object collided with is in our defined layer mask
        if (collideLayerMask == (collideLayerMask | (1 << coll.gameObject.layer))) {
            // if this object has not already been affected by this explosion instance
            if (!hitObjects.Contains(coll.gameObject)) {
                // if the component is not found, it will return null and we can ignore it
                Rigidbody2D collRB = coll.gameObject.GetComponent<Rigidbody2D>();
                if (collRB != null) {
                    // call method in a custom script called ExplosionManager to change the material temporarily for this rigidbody to allow for more bounce
                    explosionManager.StartBouncyCoForRigidbody(collRB);

                    // get the vector pointing from the center of the explosion to the center of the collided transform
                    Vector2 explosiveForceDirection = coll.transform.position - transform.position;
                    // give the vector a magnitude of 1 so it can be used as a direction
                    explosiveForceDirection = explosiveForceDirection.normalized;

                    // get magnitude of force using an inverse square function of the distance so that closer objects get hit harder
                    float distanceFromExplosion = Vector2.Distance(coll.transform.position, transform.position);
                    // adjust the curve based on the radius of the explosion
                    float distanceFactor = (1.0f / explosionRadius) * distanceFactorMultiplier;
                    float forceStrength = maxForceStrength * ( 1.0f / Mathf.Pow(distanceFactor * distanceFromExplosion, 2) );
                    // make sure to clamp it because as distance approaches zero, the force approaches infinity, which is bad
                    forceStrength = Mathf.Clamp(forceStrength, 0.0f, maxForceStrength);
                    
                    // add a force with "Impulse" mode on as this is a force applied all at once
                    collRB.AddForceAtPosition(explosiveForceDirection * forceStrength, transform.position, ForceMode2D.Impulse);
                    hitObjects.Add(coll.gameObject);
                }
            }
        }
    }
}
