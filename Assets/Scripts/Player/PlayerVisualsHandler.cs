using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This system was set up for more colors but only two are used
public class PlayerVisualsHandler : MonoBehaviour {
    // Colors and gradients should go in order: green, orange
    public Color[] colorStates;
    public Gradient[] trailGradients;
    // The outline around the player sprite
    public SpriteRenderer outlineColor;
    public TrailRenderer trailRenderer;

    private string currentColorType = "green";
    private string currentTransitionColor = "none";
    private Coroutine colorChangeDelayCo;
    private bool colorChangeDelayCoOn = false;

    public void SetPlayerColorsIfNeeded(string colorType) {
        // ignore the call if the color type was already set or is being transitioned into
        if (colorType.Equals(currentColorType) ||
            colorType.Equals(currentTransitionColor)) {
            return;
        }

        // if a color is still being transitioned, stop it and allow for the new color
        if (colorChangeDelayCoOn && colorChangeDelayCo != null) {
            StopCoroutine(colorChangeDelayCo);
            colorChangeDelayCoOn = false;
            currentTransitionColor = "none";
        }

        // For green and orange specifically, start a delay to ensure that
        // the color change is necessary since strobbing colors is bad
        if (colorType.Equals("green") || colorType.Equals("orange")) {
            colorChangeDelayCoOn = true;
            currentTransitionColor = colorType;
            colorChangeDelayCo = StartCoroutine(DelayBeforeColorChange(colorType));
        }
        else {
            Debug.LogError("Did not recognize player color type: " + colorType);
        }
    }

    public bool CheckForVisualsGroundedState() {
        // Green indicates that the player has been grounded for long enough for a color change
        return currentColorType.Equals("green");
    }

    IEnumerator DelayBeforeColorChange(string colorType) {
        yield return new WaitForSeconds(0.1f);

        if (colorType.Equals("green")) {
            outlineColor.color = colorStates[0];
            trailRenderer.colorGradient = trailGradients[0];
        }
        else if (colorType.Equals("orange")) {
            outlineColor.color = colorStates[1];
            trailRenderer.colorGradient = trailGradients[1];
        }
        
        // at this point the color is finished transitioning and a new color could begin
        currentColorType = colorType;
        currentTransitionColor = "none";
        colorChangeDelayCoOn = false;
    }
}
