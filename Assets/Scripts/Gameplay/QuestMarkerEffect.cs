using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class QuestMarkerEffect : MonoBehaviour
{
    [Header("Blinking light")]
    [Tooltip("Assign a separate light GameObject, not this marker GameObject.")]
    [SerializeField] private GameObject blinkingLight;
    [Tooltip("Seconds between each light toggle.")]
    [SerializeField, Min(0.01f)] private float blinkIntervalSeconds = 0.5f;

    [Header("Spinning")]
    [SerializeField] private Vector3 spinAxis = Vector3.up;
    [SerializeField] private float spinSpeedDegreesPerSecond = 45f;

    private bool lightIsOn = true;
    private float blinkTimer;
    private bool soundIsPlaying;
    private AudioSource soundSource;

    private void Awake()
    {
        soundSource = GetComponent<AudioSource>();
        soundSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        blinkTimer = 0f;
        lightIsOn = true;

        if (blinkingLight != null)
            blinkingLight.SetActive(true);

        if (soundSource.clip != null)
            StartCoroutine(StartLoopingSoundNextFrame());
    }

    private IEnumerator StartLoopingSoundNextFrame()
    {
        yield return null;

        if (!isActiveAndEnabled || soundSource.clip == null)
            yield break;

        soundSource.loop = true;
        soundSource.Play();
        soundIsPlaying = true;
    }

    private void OnDisable()
    {
        if (soundIsPlaying && soundSource != null)
        {
            soundSource.Stop();
            soundIsPlaying = false;
        }
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;

        if (spinAxis.sqrMagnitude > 0f && spinSpeedDegreesPerSecond != 0f)
            transform.Rotate(spinAxis.normalized, spinSpeedDegreesPerSecond * deltaTime, Space.Self);

        if (blinkingLight == null)
            return;

        float blinkInterval = Mathf.Max(blinkIntervalSeconds, 0.01f);
        blinkTimer += deltaTime;
        if (blinkTimer < blinkInterval)
            return;

        int toggleCount = Mathf.FloorToInt(blinkTimer / blinkInterval);
        blinkTimer -= toggleCount * blinkInterval;

        if ((toggleCount & 1) != 0)
        {
            lightIsOn = !lightIsOn;
            blinkingLight.SetActive(lightIsOn);
        }
    }
}
