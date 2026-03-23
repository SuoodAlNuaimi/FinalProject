using UnityEngine;

/// <summary>
/// ShipController — Gives the player a fun, floaty spaceship feel.
/// Attach to your spaceship GameObject.
/// Requires a Rigidbody component (set Gravity Scale to 0, Drag to 2).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    [Header("Movement")]
    public float thrustForce = 20f;       // Forward/back speed
    public float strafeForce = 10f;       // Left/right strafing
    public float verticalForce = 10f;     // Up/down movement
    public float maxSpeed = 30f;          // Speed cap

    [Header("Rotation")]
    public float pitchSpeed = 60f;        // Look up/down (mouse Y)
    public float yawSpeed = 80f;          // Turn left/right (mouse X)
    public float rollSpeed = 50f;         // Roll (Q/E keys)

    [Header("Boost")]
    public float boostMultiplier = 2.5f;
    public float boostDuration = 2f;
    public float boostCooldown = 5f;

    [Header("Camera")]
    public Transform cameraTarget;        // Assign a child "CameraTarget" transform
    public float cameraSmoothing = 5f;

    // Private state
    private Rigidbody rb;
    private float boostTimer = 0f;
    private float boostCooldownTimer = 0f;
    private bool isBoosting = false;
    private Camera mainCam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 1.5f;
        rb.angularDamping = 3f;

        mainCam = Camera.main;

        // Lock and hide cursor for mouse look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleBoostInput();
        HandleCursor();
    }

    private void FixedUpdate()
    {
        HandleThrust();
        HandleRotation();
        ClampSpeed();
    }

    private void LateUpdate()
    {
        FollowCamera();
    }

    // ─── Thrust ──────────────────────────────────────────────────────────────

    private void HandleThrust()
    {
        float thrust    = Input.GetAxis("Vertical");       // W/S or Up/Down
        float strafe    = Input.GetAxis("Horizontal");     // A/D or Left/Right
        float vertical  = 0f;

        if (Input.GetKey(KeyCode.Space))    vertical =  1f;
        if (Input.GetKey(KeyCode.LeftControl)) vertical = -1f;

        float currentBoost = isBoosting ? boostMultiplier : 1f;

        Vector3 force = (transform.forward * thrust * thrustForce)
                      + (transform.right   * strafe * strafeForce)
                      + (transform.up      * vertical * verticalForce);

        rb.AddForce(force * currentBoost, ForceMode.Acceleration);
    }

    // ─── Rotation ────────────────────────────────────────────────────────────

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float roll   = 0f;

        if (Input.GetKey(KeyCode.Q)) roll =  1f;
        if (Input.GetKey(KeyCode.E)) roll = -1f;

        Vector3 torque = new Vector3(-mouseY * pitchSpeed,
                                      mouseX * yawSpeed,
                                      roll   * rollSpeed);

        rb.AddTorque(transform.TransformDirection(torque) * Time.fixedDeltaTime,
                     ForceMode.Acceleration);
    }

    // ─── Boost ───────────────────────────────────────────────────────────────

    private void HandleBoostInput()
    {
        // Cooldown countdown
        if (boostCooldownTimer > 0f)
            boostCooldownTimer -= Time.deltaTime;

        // Active boost countdown
        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                isBoosting = false;
                boostCooldownTimer = boostCooldown;
            }
        }

        // Trigger boost (Left Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isBoosting && boostCooldownTimer <= 0f)
        {
            isBoosting = true;
            boostTimer = boostDuration;
        }
    }

    // ─── Speed Cap ───────────────────────────────────────────────────────────

    private void ClampSpeed()
    {
        float limit = maxSpeed * (isBoosting ? boostMultiplier : 1f);
        if (rb.linearVelocity.magnitude > limit)
            rb.linearVelocity = rb.linearVelocity.normalized * limit;
    }

    // ─── Camera Follow ───────────────────────────────────────────────────────

    private void FollowCamera()
    {
        if (cameraTarget == null || mainCam == null) return;

        mainCam.transform.position = Vector3.Lerp(
            mainCam.transform.position,
            cameraTarget.position,
            Time.deltaTime * cameraSmoothing
        );
        mainCam.transform.rotation = Quaternion.Lerp(
            mainCam.transform.rotation,
            cameraTarget.rotation,
            Time.deltaTime * cameraSmoothing
        );
    }

    // ─── Cursor Toggle ───────────────────────────────────────────────────────

    private void HandleCursor()
    {
        // Press Escape to unlock cursor (e.g. to click UI)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // Press F to re-lock
        if (Input.GetKeyDown(KeyCode.F))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // ─── Public Helpers ──────────────────────────────────────────────────────

    public bool IsBoostReady() => boostCooldownTimer <= 0f && !isBoosting;
    public float BoostCooldownRemaining() => Mathf.Max(0f, boostCooldownTimer);
    public float CurrentSpeed() => rb.linearVelocity.magnitude;
}
