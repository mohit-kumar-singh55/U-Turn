using Gilzoide.UpdateManager;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

/// <summary>
/// スプラインに沿った車両の移動、加減速処理、コース外れ時のゲームオーバー判定を行うクラス
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(CinemachineImpulseSource))]
public class VehicleController : AManagedBehaviour, IFixedUpdatable, ILateUpdatable
{
    [Header("Spline")]
    [SerializeField] SplineContainer spline;
    [SerializeField] float lookAheadDist = 2f;     // ステアリングの先読み距離

    [Header("Car Settings")]
    [SerializeField] float accelRate = 10f;        // スペース押下時の加速量
    [SerializeField] float decelRate = 2f;         // スペース未押下時の減速量
    [SerializeField] float maxSpeed = 50f;
    [SerializeField] float minSpeed = 25f;
    [SerializeField] float turnSharpnessThreshold = 35f; // 「急カーブ」とみなす角度
    [SerializeField, Range(0f, 1f)] float percentMaxSpeedToGoOffTrack = 0.7f; // ハードターン時に、currentSpeed がこの値（最大速度の％）を超えるとコースアウトする
    [SerializeField] ParticleSystem leftSkidParticle;
    [SerializeField] ParticleSystem rightSkidParticle;

    private Rigidbody rb;
    private float splineLength;
    private float t;        // スプライン上の車両位置［0...1］
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
        // *** init ***
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
            // スプライン上の最寄り点を取得
            CalculateNearestPoint();

            // 車両を回転させる
            CalculateAndSetRotation();

            // 車両を移動させる
            MoveVehicle();
        }

        // 失敗判定のためにカーブの急さをチェック
        if (CanGoOffTrack())
        {
            // コースアウト
            SendVehicleOffTrack();

            /*
            * => 角度に応じてコースアウト判定用の速度基準を決定 (仮)
            * 0°：直線
            * 10°〜20°：緩やかなカーブ
            * 30°以上：急カーブ
            * 60°以上：Uターンレベル
            */
        }
    }

    public void ManagedLateUpdate()
    {
        uiManager.SetSpeedText(Mathf.RoundToInt(currentSpeed));
    }

    void CalculateNearestPoint()
    {
        // スプライン上の最寄り点を取得
        SplineUtility.GetNearestPoint(spline.Spline, transform.position, out var nearest, out float nearestT);
        t = nearestT;
    }

    void CalculateAndSetRotation()
    {
        // 先読みターゲットを算出
        float lookT = Mathf.Clamp01(t + lookAheadDist / splineLength);
        Vector3 targetPos = spline.EvaluatePosition(lookT);

        // ターゲットへの方向
        Vector3 dirToTarget = (targetPos - transform.position).normalized;
        dirToTarget.y = 0f; // 無視

        // ターゲット方向へ回転させる
        Quaternion targetRot = Quaternion.LookRotation(dirToTarget, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
    }

    void MoveVehicle()
    {
        // 加減速処理
        currentSpeed = rb.linearVelocity.magnitude;

        if (Input.GetKey(KeyCode.Space))
        {
            PlaySkidParticles();
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, accelRate * Time.fixedDeltaTime);
            // audioManager.PlayThrottlingSound();
        }
        else
            currentSpeed = Mathf.Lerp(currentSpeed, minSpeed, decelRate * Time.fixedDeltaTime);

        // 前進速度を適用する
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    bool CanGoOffTrack()
    {
        float turnAngle = GetTurnAngle(t);
        // Debug.Log("Turn angle: " + turnAngle);
        return controlEnabled && turnAngle > turnSharpnessThreshold && currentSpeed > maxSpeed * percentMaxSpeedToGoOffTrack;
    }

    // コースアウト
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
        float checkDist = 1f;   // スプライン前後のチェック距離
        float tAhead = Mathf.Clamp01(tPos + checkDist / splineLength);  // スプライン上の少し先
        float tBehind = Mathf.Clamp01(tPos - checkDist / splineLength); // スプライン上の少し後ろ

        // スプライン上のワールド座標
        Vector3 aheadPos = spline.EvaluatePosition(tAhead);
        Vector3 behindPos = spline.EvaluatePosition(tBehind);

        // 車両の現在位置を基準にした方向ベクトル
        Vector3 forwardAhead = (aheadPos - transform.position).normalized;
        Vector3 forwardBehind = (transform.position - behindPos).normalized;

        // for visualization
        // Debug.DrawRay(transform.position, forwardAhead, Color.cyan, .5f, true);
        // Debug.DrawRay(transform.position, forwardBehind, Color.magenta, .5f, true);

        // これらの方向同士の角度がカーブの鋭さを表す
        return Vector3.Angle(forwardBehind, forwardAhead);
    }

    void TriggerGameLose() => GameManager.Instance.TriggerLose();

    // スキッドエフェクト
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
