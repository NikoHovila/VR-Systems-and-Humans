using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NameDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Camera viewCamera;
    [SerializeField] private List<GameObject> crosshairs = new List<GameObject>();
    [SerializeField] private Vector2 textAnchoredPosition = Vector2.zero;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private LayerMask displayLayer = ~0;
    private readonly Dictionary<GameObject, bool> crosshairInitialStates = new Dictionary<GameObject, bool>();

    private void Awake()
    {
        if (viewCamera == null)
            viewCamera = Camera.main;

        if (crosshairs != null)
        {
            foreach (GameObject crosshair in crosshairs)
            {
                if (crosshair != null && !crosshairInitialStates.ContainsKey(crosshair))
                    crosshairInitialStates.Add(crosshair, crosshair.activeSelf);
            }
        }

        if (nameText != null)
        {
            nameText.text = string.Empty;
            nameText.rectTransform.anchoredPosition = textAnchoredPosition;
        }
    }

    private void OnDisable()
    {
        SetCrosshairVisible(true);
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
            ClearDisplayedName();
            return;
        }

        ObjectName objectName = hit.collider.GetComponentInParent<ObjectName>();
        if (objectName == null || string.IsNullOrWhiteSpace(objectName.DisplayName))
        {
            ClearDisplayedName();
            return;
        }

        nameText.text = objectName.DisplayName;
        SetCrosshairVisible(false);
    }

    private void ClearDisplayedName()
    {
        nameText.text = string.Empty;
        SetCrosshairVisible(true);
    }

    private void SetCrosshairVisible(bool visible)
    {
        foreach (KeyValuePair<GameObject, bool> crosshairState in crosshairInitialStates)
        {
            if (crosshairState.Key != null)
                crosshairState.Key.SetActive(visible && crosshairState.Value);
        }
    }
}
