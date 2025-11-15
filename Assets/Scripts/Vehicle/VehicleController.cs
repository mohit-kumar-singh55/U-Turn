using Gilzoide.UpdateManager;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody), typeof(CinemachineImpulseSource))]
public class VehicleController : AManagedBehaviour, IFixedUpdatable, ILateUpdatable
{
    [Header("Spline")]
    [SerializeField] SplineContainer spline;
    [SerializeField] float lookAheadDist = 2f;     // How far ahead to look for steering

    [Header("Car Settings")]
    [SerializeField] float accelRate = 10f;        // Acceleration when holding space
    [SerializeField] float decelRate = 2f;         // Deceleration when not holding space
    [SerializeField] float maxSpeed = 50f;
    [SerializeField] float minSpeed = 25f;
    [SerializeField] float turnSharpnessThreshold = 35f; // Degrees where it's considered a "hard turn"
    [SerializeField, Range(0f, 1f)] float percentMaxSpeedToGoOffTrack = 0.7f; // percent of max speed, if currentSpeed goes above it on hard turn, vehicle will go off track
    [SerializeField] ParticleSystem leftSkidParticle;
    [SerializeField] ParticleSystem rightSkidParticle;

    private Rigidbody rb;
    private float splineLength;
    private float t;        // Car's position along spline [0..1]
    private bool controlEnabled = true;
    private float currentSpeed = 0f;
    private CinemachineImpulseSource cineImpulseSouce;
    private UIManager uiManager;
    // private AudioManager audioManager;

    void Awake()
    {
        ValidateFields();
    }

    void Start()
    {
        // audioManager = AudioManager.Instance;
        uiManager = UIManager.Instance;
        rb = GetComponent<Rigidbody>();
        cineImpulseSouce = GetComponent<CinemachineImpulseSource>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        splineLength = spline.CalculateLength();
    }

    public void ManagedFixedUpdate()
    {
        if (controlEnabled)
        {
            // Find nearest spline position
            CalculateNearestPoint();

            // Rotate vehicle
            CalculateAndSetRotation();

            // Move vehicle
            MoveVehicle();
        }

        // Check steepness for possible failure
        if (CanGoOffTrack())
        {
            SendVehicleOffTrack();

            /* 
            * => Decide off track velocity for different angles
            * 0° → perfectly straight section.
            * 10°-20° → gentle curve.
            * 30°+ → sharp turn.
            * 60°+ → U-turn territory.
            */
        }
    }

    public void ManagedLateUpdate()
    {
        uiManager.SetSpeedText(Mathf.RoundToInt(currentSpeed));
    }

    void CalculateNearestPoint()
    {
        // Find nearest spline position
        SplineUtility.GetNearestPoint(spline.Spline, transform.position, out var nearest, out float nearestT);
        t = nearestT;
    }

    void CalculateAndSetRotation()
    {
        // Calculate look-ahead target
        float lookT = Mathf.Clamp01(t + lookAheadDist / splineLength);
        Vector3 targetPos = spline.EvaluatePosition(lookT);

        // Direction towards target
        Vector3 dirToTarget = (targetPos - transform.position).normalized;
        dirToTarget.y = 0f; // Keep flat on ground

        // Rotate towards target
        Quaternion targetRot = Quaternion.LookRotation(dirToTarget, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
    }

    void MoveVehicle()
    {
        // Handle acceleration / deceleration
        currentSpeed = rb.linearVelocity.magnitude;

        if (Input.GetKey(KeyCode.Space))
        {
            PlaySkidParticles();
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, accelRate * Time.fixedDeltaTime);
            // audioManager.PlayThrottlingSound();
        }
        else
            currentSpeed = Mathf.Lerp(currentSpeed, minSpeed, decelRate * Time.fixedDeltaTime);

        // Apply forward velocity
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    bool CanGoOffTrack()
    {
        float turnAngle = GetTurnAngle(t);
        // Debug.Log("Turn angle: " + turnAngle);
        return controlEnabled && turnAngle > turnSharpnessThreshold && currentSpeed > maxSpeed * percentMaxSpeedToGoOffTrack;
    }

    void SendVehicleOffTrack()
    {
        // Debug.Log("Missed turn! Car goes off track!");
        controlEnabled = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(transform.forward * 2000f, ForceMode.Impulse);
        rb.AddTorque((transform.forward + Vector3.up) * 3000f, ForceMode.Impulse);

        // gameover
        // audioManager.StopAllAudios();
        Invoke(nameof(TriggerGameLose), 2f);
        enabled = false;
    }

    float GetTurnAngle(float tPos)
    {
        float checkDist = 1f;
        float tAhead = Mathf.Clamp01(tPos + checkDist / splineLength);
        float tBehind = Mathf.Clamp01(tPos - checkDist / splineLength);

        Vector3 aheadPos = spline.EvaluatePosition(tAhead);
        Vector3 behindPos = spline.EvaluatePosition(tBehind);

        Vector3 forwardAhead = (aheadPos - transform.position).normalized;
        Vector3 forwardBehind = (transform.position - behindPos).normalized;

        return Vector3.Angle(forwardBehind, forwardAhead);
    }

    void TriggerGameLose() => GameManager.Instance.TriggerLose();

    void PlaySkidParticles()
    {
        if (leftSkidParticle.isEmitting || rightSkidParticle.isEmitting) return;

        leftSkidParticle.Play();
        rightSkidParticle.Play();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(LAYERS.ENVIRONMENT))
            cineImpulseSouce.GenerateImpulse();
    }

    void ValidateFields()
    {
        Assert.IsNotNull(spline, "Spline not provided");
        Assert.IsNotNull(leftSkidParticle, "Left skid particle not provided");
        Assert.IsNotNull(rightSkidParticle, "Right skid particle not provided");
    }
}
