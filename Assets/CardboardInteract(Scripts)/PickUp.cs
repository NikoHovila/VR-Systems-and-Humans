using UnityEngine;

public class PickUp : Interactive
{
    public new void Interact()
    {
        gameObject.SetActive(false);
    }
}
