using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactive : MonoBehaviour
{
    public InteractMode interactModeOverride = InteractMode.None;
    public float twistMultiplierOverride = 0f;
    public float dwellTimerOverride = 0f;

    private bool canInteract = true;

    public bool IsInteractable => canInteract && isActiveAndEnabled;
    public event System.Action<Interactive> Interacted;

    public void SetInteractable(bool interactable)
    {
        canInteract = interactable;

        if (!interactable && CameraInteract.selected == this)
            CameraInteract.selected = null;
    }

    public void NotifyInteracted()
    {
        if (IsInteractable)
            Interacted?.Invoke(this);
    }

    public void Interact()
    {
        Debug.Log("Interacted with " + transform.name);
    }
}
