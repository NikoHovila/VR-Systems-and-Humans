using UnityEngine;

public class Grabbable : Interactive
{
    [SerializeField] float grabSpeed = 5f;
    [Header("Hold Distance Mode")]
    [SerializeField] bool useFixedDistanceForFarObjects = false;
    [SerializeField] float fixedHoldDistance = 10f;
    public bool useGravity = true;

    int ballLayer;
    int defaultLayer;

    static Transform cam = null;

    Rigidbody rb;
    Collider col;

    float grabDistance = 0f;

    bool firstGrab = false;
    bool isGrabbed = false;

    static Grabbable currentGrabbedItem = null;

    public bool IsGrabbed => isGrabbed;
    public static bool IsAnyItemGrabbed => currentGrabbedItem != null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        CameraInteract cameraInteractor = FindFirstObjectByType<CameraInteract>();
        if (cameraInteractor != null)
            cam = cameraInteractor.transform;

        ballLayer = LayerMask.NameToLayer("Ball");
        defaultLayer = LayerMask.NameToLayer("Default");
    }

    private void Start()
    {
        if (ballLayer != -1 && defaultLayer != -1)
        {
            Physics.IgnoreLayerCollision(ballLayer, defaultLayer, true);
            gameObject.layer = ballLayer;
        }

        if (rb != null)
            rb.useGravity = false;
    }

    public new void Interact()
    {
        if (!firstGrab)
        {
            firstGrab = true;

            gameObject.layer = LayerMask.NameToLayer("Default");

            if (rb != null)
                rb.useGravity = useGravity;
        }

        if (!isGrabbed && currentGrabbedItem != null && currentGrabbedItem != this)
            return;

        if (useFixedDistanceForFarObjects && isGrabbed)
            return;

        isGrabbed = !isGrabbed;
        currentGrabbedItem = isGrabbed ? this : null;

        if (isGrabbed && cam != null)
        {
            float currentDistance = Vector3.Distance(cam.position, transform.position);
            grabDistance = useFixedDistanceForFarObjects && currentDistance > fixedHoldDistance
                ? Mathf.Max(0.1f, fixedHoldDistance)
                : currentDistance;
        }
    }

    void Update()
    {
        if (cam == null)
        {
            CameraInteract cameraInteractor = FindFirstObjectByType<CameraInteract>();
            if (cameraInteractor != null)
                cam = cameraInteractor.transform;
        }

        if (isGrabbed && cam != null && rb != null)
        {
            Vector3 targetPoint = cam.position + cam.forward * grabDistance;
            rb.linearVelocity = (targetPoint - transform.position) * grabSpeed;
        }
    }

    public void ForceReleaseGrab()
    {
        ForceReleaseGrab(0f, 0f, true, 0f);
    }

    public void ForceReleaseGrab(float linearVelocityMultiplier, float angularVelocityMultiplier, bool clampUpwardVelocity = true, float maxUpwardVelocity = 0f)
    {
        isGrabbed = false;
        if (currentGrabbedItem == this)
            currentGrabbedItem = null;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 reducedLinearVelocity = rb.linearVelocity * Mathf.Clamp01(linearVelocityMultiplier);
            if (clampUpwardVelocity && reducedLinearVelocity.y > maxUpwardVelocity)
                reducedLinearVelocity.y = maxUpwardVelocity;

            rb.linearVelocity = reducedLinearVelocity;
            rb.angularVelocity *= Mathf.Clamp01(angularVelocityMultiplier);
        }
    }

    public void ResetBallState()
    {
        firstGrab = false;
        isGrabbed = false;
        if (currentGrabbedItem == this)
            currentGrabbedItem = null;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (ballLayer == -1)
            ballLayer = LayerMask.NameToLayer("Ball");

        if (ballLayer != -1)
            gameObject.layer = ballLayer;
    }
}
