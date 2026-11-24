using UnityEngine;

public class ObjectName : MonoBehaviour
{
    [SerializeField] private string displayName;

    public string DisplayName => displayName;
}
