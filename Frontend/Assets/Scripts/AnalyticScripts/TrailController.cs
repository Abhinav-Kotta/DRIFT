using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class SpeedBasedTrail : MonoBehaviour
{
    private TrailRenderer trailRenderer;

    public Gradient speedColorGradient; // Assign a Gradient in the Inspector
    private DroneMover droneMover;

    void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        droneMover = GetComponent<DroneMover>();
    }

    void Update()
    {
        float speed = droneMover.CalculateVelocityMagnitude();

        // Normalize the speed between 0 and 1 (0 speed = 0, 10 speed = 1)
        float normalizedSpeed = Mathf.Clamp01(speed / 10f);

        // Get color based on normalized speed
        Color currentColor = speedColorGradient.Evaluate(normalizedSpeed);

        // Apply color only to the latest vertex
        SetTrailColor(currentColor);
    }

    void SetTrailColor(Color color)
    {
        int positions = trailRenderer.positionCount;

        if (positions > 0)
        {
            Gradient gradient = trailRenderer.colorGradient;
            GradientColorKey[] colorKeys = gradient.colorKeys;
            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;

            colorKeys[colorKeys.Length - 1] = new GradientColorKey(color, 1f);
            gradient.SetKeys(colorKeys, alphaKeys);

            trailRenderer.colorGradient = gradient;
        }
    }
}