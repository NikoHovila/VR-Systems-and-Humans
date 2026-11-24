using TMPro;
using UnityEngine;

public class NameDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Camera viewCamera;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private LayerMask displayLayer = ~0;

    private void Awake()
    {
        if (viewCamera == null)
            viewCamera = Camera.main;

        if (nameText != null)
            nameText.text = string.Empty;
    }

    private void Update()
    {
        if (nameText == null)
            return;

        if (viewCamera == null)
            viewCamera = Camera.main;

        if (viewCamera == null ||
            !Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out RaycastHit hit, maxDistance, displayLayer))
        {
            nameText.text = string.Empty;
            return;
        }

        ObjectName objectName = hit.collider.GetComponentInParent<ObjectName>();
        nameText.text = objectName != null ? objectName.DisplayName : string.Empty;
    }
}
