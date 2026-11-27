using UnityEngine;

/// <summary>
/// Adds a small, voice-driven motion to a character sign while its voice line plays.
/// The scene's SceneManager calls BeginSpeaking for the sign assigned to each line.
/// </summary>
public class VoiceReactiveSign : MonoBehaviour
{
    [Header("Sign")]
    [Tooltip("Optional child transform to animate. Leave empty to animate this object.")]
    [SerializeField] private Transform signVisual;

    [Header("Motion")]
    [SerializeField, Min(0f)] private float maximumBounce = 0.12f;
    [SerializeField, Min(0f)] private float maximumSwayDegrees = 5f;
    [SerializeField, Min(0f)] private float maximumPitchTiltDegrees = 4f;
    [SerializeField, Min(0f)] private float motionSpeed = 11f;
    [SerializeField, Min(0f)] private float loudnessSensitivity = 12f;
    [SerializeField, Range(0f, 1f)] private float minimumLoudness = 0.025f;
    [SerializeField, Range(0f, 1f)] private float pitchSensitivity = 0.35f;
    [SerializeField, Min(0f)] private float returnSpeed = 8f;

    private const int SampleCount = 256;
    private const int SpectrumCount = 128;
    private readonly float[] samples = new float[SampleCount];
    private readonly float[] spectrum = new float[SpectrumCount];

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private float loudness;
    private float brightness;
    private float speechStartKick;
    private AudioSource voiceSource;
    private bool isSpeaking;

    private void Awake()
    {
        if (signVisual == null)
            signVisual = transform;

        initialLocalPosition = signVisual.localPosition;
        initialLocalRotation = signVisual.localRotation;
    }

    /// <summary>Called by SceneManager when this sign is the speaker for a voice line.</summary>
    public void BeginSpeaking(AudioSource source)
    {
        voiceSource = source;
        isSpeaking = voiceSource != null;
        speechStartKick = 0.2f;
    }

    /// <summary>Called by SceneManager when this sign's voice line ends.</summary>
    public void EndSpeaking()
    {
        isSpeaking = false;
        voiceSource = null;
    }

    private void Update()
    {
        if (isSpeaking && voiceSource != null && voiceSource.isPlaying)
        {
            SampleVoice();
        }
        else
        {
            loudness = Mathf.MoveTowards(loudness, 0f, returnSpeed * Time.deltaTime);
            brightness = Mathf.MoveTowards(brightness, 0f, returnSpeed * Time.deltaTime);
        }

        AnimateSign();
    }

    private void SampleVoice()
    {
        voiceSource.GetOutputData(samples, 0);
        float sumSquares = 0f;
        for (int i = 0; i < samples.Length; i++)
            sumSquares += samples[i] * samples[i];

        float rms = Mathf.Sqrt(sumSquares / samples.Length);
        float targetLoudness = rms < minimumLoudness
            ? 0f
            : Mathf.Clamp01((rms - minimumLoudness) * loudnessSensitivity);
        loudness = Mathf.Lerp(loudness, targetLoudness, 1f - Mathf.Exp(-18f * Time.deltaTime));

        voiceSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
        float weightedSum = 0f;
        float magnitudeSum = 0f;
        for (int i = 1; i < spectrum.Length; i++)
        {
            weightedSum += spectrum[i] * i;
            magnitudeSum += spectrum[i];
        }

        float spectralCentroid = magnitudeSum > 0f ? weightedSum / magnitudeSum : 0f;
        float targetBrightness = Mathf.Clamp01((spectralCentroid - 5f) / 35f);
        brightness = Mathf.Lerp(brightness, targetBrightness, 1f - Mathf.Exp(-10f * Time.deltaTime));
    }

    private void AnimateSign()
    {
        float energy = Mathf.Clamp01(loudness + speechStartKick);
        float bounceWave = 0.5f + 0.5f * Mathf.Sin(Time.time * motionSpeed);
        float bounce = energy * bounceWave * maximumBounce;
        float sway = Mathf.Sin(Time.time * motionSpeed * 0.55f) * energy * maximumSwayDegrees;
        float pitchTilt = brightness * pitchSensitivity * maximumPitchTiltDegrees;

        Vector3 targetPosition = initialLocalPosition + Vector3.up * bounce;
        Quaternion motionRotation = Quaternion.Euler(-pitchTilt, 0f, sway);
        float blend = loudness > 0.001f ? 1f : 1f - Mathf.Exp(-returnSpeed * Time.deltaTime);

        signVisual.localPosition = Vector3.Lerp(signVisual.localPosition, targetPosition, blend);
        signVisual.localRotation = Quaternion.Slerp(
            signVisual.localRotation,
            initialLocalRotation * motionRotation,
            blend);

        speechStartKick = Mathf.MoveTowards(speechStartKick, 0f, 2f * Time.deltaTime);
    }
}
