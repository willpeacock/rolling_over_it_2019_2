using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    // Amount of thrust applied from user input when player is grounded
    public float groundThrust = 1200f;
    // Amount of thrust applied from user input when player is in the air
    public float airThrust = 500f;
    // Amount of thrust applied all at once upwards when the player jumps
    public float jumpThrust = 800f;
    // A cutoff point to stop the player from gaining more velocity from input
    public float maxThrustVelocity = 25.0f;
    // The distance from the center of the player to the ground to determine its "grounded" state
    public float distanceToGround = 0.85f;
    // Include all layers considered ground
    public LayerMask groundLayerMask;
    public AudioSource rollSound;
    public AudioSource jumpSound;
    public AudioSource hitSound;
    // Editor toggle to not have to wait for the intro cutscene to finish
    public bool delayBeforeStartOn = true;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private PlayerVisualsHandler visualsHandler;
    private PlayerInput playerInput;
    private bool playerCanMove = false;
    private Transform mainCamTransform;
    private bool rollingAudioIsPlaying = false;
    private bool rollingAudioFadingActive = false;
    private bool canPlayWhack = false;
    private bool playerMadeCollision = false;
    private Animator mainAnim;
    private float defaultRollVolume;

    void Start() {
        // All of these components are attatched directly to the same gameobject as the player
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        visualsHandler = GetComponent<PlayerVisualsHandler>();
        playerInput = GetComponent<PlayerInput>();
        mainAnim = GetComponent<Animator>();

        // Store this value for fading it in and out smoothly
        defaultRollVolume = rollSound.volume;

        mainCamTransform = Camera.main.transform;

        // Prevent moving during the custsene unless an editor setting is flipped on
        if (delayBeforeStartOn)
            StartCoroutine(DelayBeforeMoveEnableCo());
        else
            playerCanMove = true;

        // Begins coroutine that is used to handle the "impact sound" the player makes
        StartCoroutine(CheckForDelayInCollisionsForWhack());
    }

    void Update() {
        // If the player is grounded
        if (CheckForGround()) {
            // Tell the visuals handler to change the color to green
            visualsHandler.SetPlayerColorsIfNeeded("green");

            // Player jump
            if (playerCanMove && playerInput.GetJumpButtonDown()) {
                // Fade out rolling on ground audio if needed
                if (rollingAudioIsPlaying && !rollingAudioFadingActive) {
                    rollingAudioFadingActive = true;
                    StartCoroutine(FadeRollingSound());
                }
                // Play the jump sound with a randomized pitch to make it sound less grating
                jumpSound.pitch = Random.Range(0.8f, 1.2f);
                jumpSound.Play();
                // Play the jump animation
                mainAnim.Play("wilbur_jump");
                // Apply a IMPULSE force upwards, impulse has to be specified and sends a single force signal all at once
                rb.AddForce(Vector2.up * jumpThrust, ForceMode2D.Impulse);
            }
        }
        else {
            // If the player is not grounded, set color to orange
            visualsHandler.SetPlayerColorsIfNeeded("orange");
        }
    }

    // Runs at a fixed frame-rate
    // Put the majority of physics-based functionality in here
    // (There can be problems with missed input if it is checked in FixedUpdate)
    void FixedUpdate() {
        if (playerCanMove) {
            HandleHorizontalMovement();
        }
    }

    void HandleHorizontalMovement() {
        float horizontalInput = playerInput.GetHorizontalInput();

        // Set the volume of the rolling sound to be dependent on the speed of the player
        if (!rollingAudioFadingActive && rollingAudioIsPlaying) {
            rollSound.volume = Mathf.Clamp(rb.velocity.magnitude * (defaultRollVolume/10.0f),0, defaultRollVolume);
        }

        // Begin rolling sound if neccessary
        if (!rollingAudioIsPlaying && CheckForGround()) {
            if (Mathf.Abs(horizontalInput) > 0 || rb.velocity.magnitude > 1.0f) {
                rollSound.Play();
                rollingAudioIsPlaying = true;
            }
        }
        // Stop rolling sound if necessary
        else if (rollingAudioIsPlaying && !rollingAudioFadingActive) {
            if (!CheckForGround() || Mathf.Abs(horizontalInput) == 0 && rb.velocity.magnitude < 1.0f) {
                rollingAudioFadingActive = true;
                StartCoroutine(DelayBeforeFadeRollingSound(horizontalInput));
            }
        }

        // The direction and magnitude of the movement is determined by the horizontal input
        Vector2 moveVector = new Vector2(horizontalInput, 0);

        // Change the amount of control the player has on ground vs in air
        float thrust = CheckForGround() ? groundThrust : airThrust;
        
        // This is the core movement logic of the player
        // '''
        // It prevents force from being added through input when the player is moving too fast
        // UNLESS the player is trying to go in the opposite direction of the force
        // '''
        if (rb.velocity.magnitude <= maxThrustVelocity || Mathf.Sign(rb.velocity.x) != Mathf.Sign(horizontalInput)) {
            rb.AddForce(moveVector * thrust);
        }
    }

    public void SetPlayerCanMove(bool canMove) {
        playerCanMove = canMove;
    }

    public void StopRollingSoundIfNeeded() {
        StartCoroutine(FadeRollingSound());
    }

    private bool CheckForGround() {
        bool isGrounded = false;

        // Create three raycasts that shoot downwards at different positions on the player to check for ground
        // If any of them hit ground, the player is grounded
        for (int i = -1; i < 2; i++) {
            Vector3 rayStartPos = new Vector3(transform.position.x + i * (circleCollider.radius/1.5f), transform.position.y, transform.position.z);
            Vector3 rayEndPos = new Vector3(transform.position.x + i * (circleCollider.radius/1.5f), transform.position.y - distanceToGround, transform.position.z);
            Debug.DrawLine(rayStartPos, rayEndPos, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayStartPos, Vector2.down, distanceToGround, groundLayerMask);
            if (hit.collider != null) {
                isGrounded = true;
            }
        }

        return isGrounded;
    }

    IEnumerator DelayBeforeMoveEnableCo()  {
        // Wait for the camera to reach the player before enabling movement
        while (!VisibleByCamera()) {
            yield return null;
        }
        playerCanMove = true;
    }

    // Checks if the center of the player's transform is within the bounds of the screen
    private bool VisibleByCamera() {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }

    IEnumerator DelayBeforeFadeRollingSound(float horizontalInput)  {
        yield return new WaitForSeconds(0.5f);
        // If the rolling sound should still stop according to its conditions after this delay
        // Then it is time to stop the rolling sound
        if (!CheckForGround() || Mathf.Abs(horizontalInput) == 0 && rb.velocity.magnitude < 1.0f) {
            // Fade out the rolling sound
            while (rollSound.volume > 0) {
                rollSound.volume -= Time.deltaTime * 5.0f;
                yield return null;
            }
            rollSound.Stop();
            rollSound.volume = defaultRollVolume;
            rollingAudioIsPlaying = false;
        }
        rollingAudioFadingActive = false;
    }

    // An alternative coroutine that fades out the rolling sound immediately 
    IEnumerator FadeRollingSound()  {
        while (rollSound.volume > 0) {
            rollSound.volume -= Time.deltaTime * 5.0f;
            yield return null;
        }
        rollSound.Stop();
        rollSound.volume = defaultRollVolume;
        rollingAudioIsPlaying = false;
        rollingAudioFadingActive = false;
    }

    void OnCollisionEnter2D(Collision2D coll) {
        // Every time the player enters a collision, set playerMadeCollision to true
        playerMadeCollision = true;
        // If the player is deemed able to make a whack sound, make the sound upon ENTERING a NEW collision
        if (canPlayWhack) {
            hitSound.pitch = Random.Range(0.8f, 1.2f);
            hitSound.Play();
            canPlayWhack = false;
        }
    }

    void OnCollisionStay2D(Collision2D coll) {
        // Every time the player is seen touching a collision, set playerMadeCollision to true
        playerMadeCollision = true;
    }

    // Don't read too much into this logic, it's mostly tweaking to avoid audio spam
    IEnumerator CheckForDelayInCollisionsForWhack()  {
        while (gameObject.activeSelf) {
            float counter = 0;
            while (counter < 0.25f) {
                // Reset timer every time there is a recent collision
                // OR if the player has been grounded long enough to trigger its green color
                if (playerMadeCollision || visualsHandler.CheckForVisualsGroundedState()) {
                    counter = 0;
                    playerMadeCollision = false;
                }
                counter += Time.deltaTime;
                yield return null;
            }
            canPlayWhack = true;
            // Wait for whack sound to play and for canPlayWhack to be reset
            while (canPlayWhack)
                yield return null;
            
            // Then loop again with a new counter
        }
    }
}
