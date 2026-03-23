using UnityEngine;

/// <summary>
/// PlanetOrbit — Makes a planet orbit around a central body (the Sun).
/// Attach to each planet GameObject.
/// </summary>
public class PlanetOrbit : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform orbitCenter;        // Drag the Sun here
    public float orbitSpeed = 10f;       // Degrees per second
    public float orbitRadius = 20f;      // Distance from Sun
    public float orbitTilt = 0f;         // Tilt angle of orbit plane

    [Header("Self Rotation")]
    public float selfRotationSpeed = 30f; // Spin on own axis

    private Vector3 orbitAxis;

    private void Start()
    {
        // Set starting position at correct radius
        if (orbitCenter != null)
        {
            orbitAxis = Quaternion.Euler(orbitTilt, 0, 0) * Vector3.up;
            Vector3 startOffset = new Vector3(orbitRadius, 0, 0);
            transform.position = orbitCenter.position + startOffset;
        }
    }

    private void Update()
    {
        if (orbitCenter == null) return;

        // Orbit around the sun
        transform.RotateAround(orbitCenter.position, orbitAxis, orbitSpeed * Time.deltaTime);

        // Spin on own axis
        transform.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime);
    }
}
