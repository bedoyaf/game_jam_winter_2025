using Unity.VisualScripting;
using UnityEngine;

public class CandleFlicker : MonoBehaviour
{
    [Header("Light Reference")]
    [SerializeField] private Light flameLight;
    [SerializeField] private CandleBehavior candleBehavior;

    [Header("Intensity Flickering")]
    [SerializeField] private bool flickerIntensity = true;
    [SerializeField] private float baseIntensity = 1.5f;
    [SerializeField] private float intensityVariation = 0.3f;
    [SerializeField] private float intensitySpeed = 5f;
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Range Flickering")]
    [SerializeField] private bool flickerRange = true;
    [SerializeField] private float baseRange = 3f;
    [SerializeField] private float rangeVariation = 0.5f;
    [SerializeField] private float rangeSpeed = 3f;

    [Header("Color Flickering")]
    [SerializeField] private bool flickerColor = true;
    [SerializeField] private Color baseColor = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color flickerColorVariation = new Color(0.1f, 0.05f, 0f);
    [SerializeField] private float colorSpeed = 2f;

    [Header("Position Flickering")]
    [SerializeField] private bool flickerPosition = false;
    [SerializeField] private Vector3 positionVariation = new Vector3(0.02f, 0.03f, 0.02f);
    [SerializeField] private float positionSpeed = 4f;

    [Header("Advanced Settings")]
    [SerializeField] private bool usePerlinNoise = true;
    [SerializeField] private float noiseOffsetX = 0f;
    [SerializeField] private float noiseOffsetY = 100f;
    [SerializeField] private float noiseOffsetZ = 200f;
    [SerializeField] private float noiseOffsetW = 300f;

    [Header("Random Flicker Events")]
    [SerializeField] private bool enableRandomFlickers = true;
    [SerializeField] private float randomFlickerChance = 0.02f;
    [SerializeField] private float randomFlickerIntensityMultiplier = 0.5f;
    [SerializeField] private float randomFlickerDuration = 0.1f;

    [Header("Wind Effect")]
    [SerializeField] private bool enableWindEffect = false;
    [SerializeField] private float windStrength = 0f;
    [SerializeField] private Vector3 windDirection = Vector3.right;
    [SerializeField] private float windSpeed = 1f;

    [Header("Sprite Deformation")]
    [SerializeField] private Transform flameMesh; // Reference to the child with MeshRenderer
    [SerializeField] private bool deformSprite = true;
    [SerializeField] private Vector2 scaleVariation = new Vector2(0.1f, 0.15f);
    [SerializeField] private float scaleSpeed = 4f;
    [SerializeField] private float spriteSquashStretch = 0.8f; // How much Y affects X (inverse relationship)

    private Vector3 initialMeshScale;

    private Vector3 initialPosition;
    private float randomFlickerTimer = 0f;
    private float randomFlickerTimeLeft = 0f;

    private void Start()
    {
        if (flameLight == null)
        {
            flameLight = GetComponent<Light>();
            if (flameLight == null)
            {
                Debug.LogError("CandleFlicker: No Light component found!");
                enabled = false;
                return;
            }
        }



        initialPosition = flameLight.transform.localPosition;

        if (baseIntensity == 0) baseIntensity = flameLight.intensity;
        if (baseRange == 0) baseRange = flameLight.range;

        if (flameMesh != null)
        {
            initialMeshScale = flameMesh.localScale;
        }
    }

    private void Update()
    {

        if (candleBehavior == null || !candleBehavior.candle_turned_on)
        {
            if (flameLight.enabled)
            {
                flameLight.enabled = false;
            }
            return;
        }

        // Enable light if it was off
        if (!flameLight.enabled)
        {
            flameLight.enabled = true;
        }
        float time = Time.time;

        // Intensity flickering
        if (flickerIntensity)
        {
            float noise = usePerlinNoise
                ? Mathf.PerlinNoise(time * intensitySpeed, noiseOffsetX)
                : (Mathf.Sin(time * intensitySpeed) + 1f) * 0.5f;

            float curveValue = intensityCurve.Evaluate(noise);
            float intensity = baseIntensity + (curveValue - 0.5f) * 2f * intensityVariation;

            // Apply random flicker
            if (randomFlickerTimeLeft > 0)
            {
                intensity *= randomFlickerIntensityMultiplier;
                randomFlickerTimeLeft -= Time.deltaTime;
            }

            flameLight.intensity = intensity;
        }

        // Range flickering
        if (flickerRange)
        {
            float noise = usePerlinNoise
                ? Mathf.PerlinNoise(time * rangeSpeed, noiseOffsetY)
                : (Mathf.Sin(time * rangeSpeed + 1f) + 1f) * 0.5f;

            flameLight.range = baseRange + (noise - 0.5f) * 2f * rangeVariation;
        }

        // Color flickering
        if (flickerColor)
        {
            float noiseR = usePerlinNoise
                ? Mathf.PerlinNoise(time * colorSpeed, noiseOffsetZ)
                : (Mathf.Sin(time * colorSpeed + 2f) + 1f) * 0.5f;

            float noiseG = usePerlinNoise
                ? Mathf.PerlinNoise(time * colorSpeed + 5f, noiseOffsetZ + 5f)
                : (Mathf.Sin(time * colorSpeed + 3f) + 1f) * 0.5f;

            float noiseB = usePerlinNoise
                ? Mathf.PerlinNoise(time * colorSpeed + 10f, noiseOffsetZ + 10f)
                : (Mathf.Sin(time * colorSpeed + 4f) + 1f) * 0.5f;

            Color colorVariation = new Color(
                (noiseR - 0.5f) * 2f * flickerColorVariation.r,
                (noiseG - 0.5f) * 2f * flickerColorVariation.g,
                (noiseB - 0.5f) * 2f * flickerColorVariation.b
            );

            flameLight.color = baseColor + colorVariation;
        }

        // Position flickering
        if (flickerPosition)
        {
            float noiseX = usePerlinNoise
                ? Mathf.PerlinNoise(time * positionSpeed, noiseOffsetW)
                : Mathf.Sin(time * positionSpeed);

            float noiseY = usePerlinNoise
                ? Mathf.PerlinNoise(time * positionSpeed + 5f, noiseOffsetW + 5f)
                : Mathf.Sin(time * positionSpeed + 1f);

            float noiseZ = usePerlinNoise
                ? Mathf.PerlinNoise(time * positionSpeed + 10f, noiseOffsetW + 10f)
                : Mathf.Sin(time * positionSpeed + 2f);

            Vector3 offset = new Vector3(
                noiseX * positionVariation.x,
                noiseY * positionVariation.y,
                noiseZ * positionVariation.z
            );

            // Apply wind effect
            if (enableWindEffect)
            {
                float windNoise = Mathf.PerlinNoise(time * windSpeed, 500f);
                offset += windDirection.normalized * windStrength * windNoise;
            }

            flameLight.transform.localPosition = initialPosition + offset;
        }

        // Random flicker events
        if (enableRandomFlickers && randomFlickerTimeLeft <= 0)
        {
            if (Random.value < randomFlickerChance)
            {
                randomFlickerTimeLeft = randomFlickerDuration;
            }
        }


        if (deformSprite && flameMesh != null)
        {
            float noiseX = usePerlinNoise
                ? Mathf.PerlinNoise(time * scaleSpeed, noiseOffsetW + 100f)
                : (Mathf.Sin(time * scaleSpeed + 5f) + 1f) * 0.5f;

            float noiseY = usePerlinNoise
                ? Mathf.PerlinNoise(time * scaleSpeed + 7f, noiseOffsetW + 150f)
                : (Mathf.Sin(time * scaleSpeed + 6f) + 1f) * 0.5f;

            // Calculate scale variations
            float scaleX = 1f + (noiseX - 0.5f) * 2f * scaleVariation.x;
            float scaleY = 1f + (noiseY - 0.5f) * 2f * scaleVariation.y;

            // Add squash and stretch effect (when tall, make thinner)
            scaleX *= Mathf.Lerp(1f, 1f / scaleY, spriteSquashStretch);

            // Apply wind effect to mesh if enabled
            if (enableWindEffect)
            {
                float windNoise = Mathf.PerlinNoise(time * windSpeed, 600f);
                scaleX += windDirection.normalized.x * windStrength * windNoise * 0.5f;
            }

            flameMesh.localScale = new Vector3(
                initialMeshScale.x * scaleX,
                initialMeshScale.y * scaleY,
                initialMeshScale.z
            );
        }
    }

    // Public methods to control the candle dynamically
    public void SetWindStrength(float strength)
    {
        windStrength = strength;
    }

    public void SetWindDirection(Vector3 direction)
    {
        windDirection = direction.normalized;
    }

    public void SetIntensityMultiplier(float multiplier)
    {
        baseIntensity *= multiplier;
    }
}