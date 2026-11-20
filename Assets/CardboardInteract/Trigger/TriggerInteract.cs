using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerInteract : MonoBehaviour
{
    // Minimum time to hold the trigger to interact with a Hold object.
    [SerializeField] float minObjectHoldTime = 1.5f;
    // Minimum time to hold the trigger for the global hold action.
    [SerializeField] float minGlobalHoldTime = 3f;
    // References to global action scripts
    [SerializeField] GlobalTriggerPress globalTriggerPress;
    [SerializeField] GlobalTriggerHold globalTriggerHold;


    // Visuals
    [SerializeField] GameObject holdCircle;
    [SerializeField] GameObject pressReticle;
    [SerializeField] GameObject baseReticle;
    [SerializeField] GameObject pressRing;
    [SerializeField] float pressRingPressedScale = 0.5f;    // scale 6 in editor
    private Vector3 pressRingOriginalScale;
    private Coroutine pressRingAnimation;
    [SerializeField] float pressAnimationDuration = 0.1f; // Animation duration
    
    [SerializeField] GameObject globalTriggerHoldProgress;
    


    // Timers for both
    private float objectHoldTimer;
    private float globalHoldTimer;
    // True after a hold action fires; prevents repeated activation during that press.
    private bool longPress;
    // True when the current trigger press started in global space (not looking at a Press/Hold interactable object).
    // This prevents a single continuous press from triggering interactions in both global space and object space when the user's gaze moves.
    private bool pressBeganInGlobalSpace;
    private bool globalHoldProgressStarted;
    private Interactive objectHoldTarget;


    void Start()
    {
        longPress = false;
        objectHoldTimer = minObjectHoldTime;
        globalHoldTimer = minGlobalHoldTime;

        // Visuals
        SetActiveIfAssigned(holdCircle, false);
        SetActiveIfAssigned(pressReticle, false);
        SetActiveIfAssigned(pressRing, false);
        if (pressRing != null)
            pressRingOriginalScale = pressRing.transform.localScale;

        SetActiveIfAssigned(globalTriggerHoldProgress, false);
    }

    void Update()
    {
        if (Touchscreen.current == null)
            return;
        
        // OBJECT INTERACT - Press
        // This is used for interacting with objects containing the "Interact" script, similar to Twist and Dwell
        if (CameraInteract.selected && CameraInteract.GetSelectedInteractionMode() == InteractMode.Press)
        {
            CancelObjectHoldWhilePressed();

            // Reset Global hold timer if gaze moved away from Global space
            globalHoldTimer = minGlobalHoldTime;

            // VISUALS
            // Turn off base
            SetActiveIfAssigned(baseReticle, false);
            // Turn off hold visual
            SetActiveIfAssigned(holdCircle, false);
            // Turn on press visual
            SetActiveIfAssigned(pressReticle, true);
            SetActiveIfAssigned(pressRing, true);
            // Turn off hold visual
            SetActiveIfAssigned(globalTriggerHoldProgress, false);

            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                pressBeganInGlobalSpace = false;

                if (pressRingAnimation != null)
                {
                    StopCoroutine(pressRingAnimation);
                }
                if (pressRing != null)
                    pressRingAnimation = StartCoroutine(AnimatePressRing());

                CameraInteract.InteractSelectedObject(CameraInteract.selected);
            }
        }

        // OBJECT INTERACT - Hold
        // This is used for interacting with objects containing the "Interact" script, similar to Twist and Dwell
        else if (CameraInteract.selected && CameraInteract.GetSelectedInteractionMode() == InteractMode.Hold)
        {
            // Reset Global hold timer if gaze moved away from Global space
            globalHoldTimer = minGlobalHoldTime;

            // Reset size of ring
            if (pressRingAnimation != null)
            {
                StopCoroutine(pressRingAnimation);
                pressRingAnimation = null;
            }
            if (pressRing != null)
                pressRing.transform.localScale = pressRingOriginalScale;

            // VISUALS
            // Turn on base and press reticle center
            SetActiveIfAssigned(baseReticle, true);
            SetActiveIfAssigned(pressReticle, true);
            // Turn off press ring
            SetActiveIfAssigned(pressRing, false);
            // Turn off hold visual
            SetActiveIfAssigned(globalTriggerHoldProgress, false);
            
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                longPress = false;
                objectHoldTimer = minObjectHoldTime;
                pressBeganInGlobalSpace = false;
                objectHoldTarget = CameraInteract.selected;
            }
            
            if (Touchscreen.current.primaryTouch.press.isPressed &&
                !longPress &&
                !pressBeganInGlobalSpace &&
                objectHoldTarget != null &&
                CameraInteract.selected == objectHoldTarget)
            {
                // set visuals on only after trigger is held down
                SetActiveIfAssigned(holdCircle, true);

                if (objectHoldTimer > 0f)
                {
                    objectHoldTimer = objectHoldTimer - Time.deltaTime;
                    // Animate hold
                    HoldCircleAnimation();
                }
                else
                {
                    longPress = true;

                    // Turn off hold visual
                    SetActiveIfAssigned(holdCircle, false);

                    CameraInteract.InteractSelectedObject(objectHoldTarget);
                }
            }
            else if (Touchscreen.current.primaryTouch.press.isPressed && objectHoldTarget != CameraInteract.selected)
            {
                objectHoldTarget = null;
                objectHoldTimer = minObjectHoldTime;
                SetActiveIfAssigned(holdCircle, false);
            }

            if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame && longPress == false && pressBeganInGlobalSpace == false)
            {
                // Turn off hold visual
                SetActiveIfAssigned(holdCircle, false);
                objectHoldTarget = null;
            }
        }

        // GLOBAL INTERACT
        // If you want to define some action when purely pressing the trigger, without needing a Press or Hold object to look at.
        // A short press and release invokes GlobalPress; a sufficiently long hold invokes GlobalHold.
        else
        {
            CancelObjectHoldWhilePressed();

            // Reset Object hold timer if gaze moved away from Object space
            objectHoldTimer = minObjectHoldTime;

            // Reset size of ring
            if (pressRingAnimation != null)
            {
                StopCoroutine(pressRingAnimation);
                pressRingAnimation = null;
            }
            if (pressRing != null)
                pressRing.transform.localScale = pressRingOriginalScale;

            // VISUALS
            // Turn off hold visual
            SetActiveIfAssigned(holdCircle, false);
            // Turn off press visual
            SetActiveIfAssigned(pressReticle, false);
            SetActiveIfAssigned(pressRing, false);
            // Turn on base
            SetActiveIfAssigned(baseReticle, true);
            
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                longPress = false;
                globalHoldTimer = minGlobalHoldTime;
                pressBeganInGlobalSpace = true;
                globalHoldProgressStarted = false;
            }

            if (Touchscreen.current.primaryTouch.press.isPressed && longPress == false && pressBeganInGlobalSpace == true && CameraInteract.selected == null)
            {   
                if (globalHoldTimer > 0f)
                {
                    // Control for accidental presses
                    if (globalHoldTimer > minGlobalHoldTime - 0.5f)
                    {
                        globalHoldTimer = globalHoldTimer - Time.deltaTime;
                    }
                    else
                    {
                        globalHoldProgressStarted = true;

                        // set visuals on only after trigger is held down
                        SetActiveIfAssigned(globalTriggerHoldProgress, true);

                        globalHoldTimer = globalHoldTimer - Time.deltaTime;

                        // Animate hold
                        GlobalHoldCircleAnimation();
                    }
                }
                else
                {
                    longPress = true;
                    // Turn off hold visual
                    SetActiveIfAssigned(globalTriggerHoldProgress, false);
                    // Execute logic defined in GlobalTriggerHold
                    if (globalTriggerHold != null)
                        globalTriggerHold.Execute();
                }
            }
            else
            {
                // Reset timer
                globalHoldTimer = minGlobalHoldTime;
                // Turn off hold visual
                SetActiveIfAssigned(globalTriggerHoldProgress, false);
            }

            if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame && longPress == false && pressBeganInGlobalSpace == true && globalHoldProgressStarted == false)
            {
                // Execute logic defined in GlobalTriggerPress
                if (globalTriggerPress != null)
                    globalTriggerPress.Execute();
            }
        }
    }

    void HoldCircleAnimation()
    {
        // Animate HoldCircle using the same shader formula as CameraInteract's DwellCircle.
        if (holdCircle == null || minObjectHoldTime <= 0f)
            return;

        Renderer holdRenderer = holdCircle.GetComponent<Renderer>();
        if (holdRenderer != null && holdRenderer.sharedMaterial != null)
            holdRenderer.sharedMaterial.SetFloat("_Cutoff", Mathf.Clamp(1.3f - (minObjectHoldTime - objectHoldTimer) / minObjectHoldTime * 1.3f, 0.0f, 1.0f));
    }

    void GlobalHoldCircleAnimation()
    {
        // Animate HoldCircle using the same shader formula as CameraInteract's DwellCircle.
        if (globalTriggerHoldProgress == null || minGlobalHoldTime <= 0.5f)
            return;

        Renderer holdRenderer = globalTriggerHoldProgress.GetComponentInChildren<Renderer>();
        if (holdRenderer != null && holdRenderer.sharedMaterial != null)
            holdRenderer.sharedMaterial.SetFloat("_Cutoff", Mathf.Clamp(1.3f - (minGlobalHoldTime - globalHoldTimer - 0.5f) / (minGlobalHoldTime - 0.5f) * 1.3f, 0.0f, 1.0f));
    }

    IEnumerator AnimatePressRing()
    {
        if (pressRing == null || pressAnimationDuration <= 0f)
            yield break;

        Vector3 largeScale = pressRingOriginalScale;
        Vector3 smallScale = largeScale * pressRingPressedScale;

        // Shrink
        for (float t = 0; t < pressAnimationDuration; t += Time.deltaTime)
        {
            pressRing.transform.localScale = Vector3.Lerp(largeScale, smallScale, t / pressAnimationDuration);

            yield return null;
        }

        pressRing.transform.localScale = smallScale;

        // Grow
        for (float t = 0; t < pressAnimationDuration; t += Time.deltaTime)
        {
            pressRing.transform.localScale = Vector3.Lerp(smallScale, largeScale, t / pressAnimationDuration);

            yield return null;
        }

        pressRing.transform.localScale = largeScale;
    }

    private static void SetActiveIfAssigned(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private void CancelObjectHoldWhilePressed()
    {
        if (!Touchscreen.current.primaryTouch.press.isPressed || objectHoldTarget == null)
            return;

        objectHoldTarget = null;
        objectHoldTimer = minObjectHoldTime;
        SetActiveIfAssigned(holdCircle, false);
    }
}
