using UnityEngine;

// It's generally good practice to separate your calls to input
// since it allows for easier adjustments
public class PlayerInput : MonoBehaviour {
    public float GetHorizontalInput() {
        return Input.GetAxis("Horizontal");
    }

    public bool GetJumpButtonDown() {
        return Input.GetButtonDown("Jump");
    }

    public bool GetMenuButtonDown() {
        return Input.GetKeyDown("escape");
    }
}
