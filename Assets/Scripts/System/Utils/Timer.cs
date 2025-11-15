using Gilzoide.UpdateManager;
using UnityEngine;

public class Timer : AManagedBehaviour, IUpdatable
{
    public static Timer Instance { get; private set; }

    [SerializeField] float totalTime = 30f;

    private float _timeLeft;
    private bool stopTimer = false;
    private UIManager uiManager;

    public void StopTimer(bool stop = true) => stopTimer = stop;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        uiManager = UIManager.Instance;

        _timeLeft = totalTime;
    }

    public void ManagedUpdate()
    {
        // 勝ったらタイマーを止める
        if (stopTimer) return;

        // タイマーが0になったらゲームオーバー
        if (_timeLeft == 0)
        {
            GameManager.Instance.TriggerLose();
            enabled = false;
        }

        if (_timeLeft > 0) _timeLeft -= Time.deltaTime;
        else _timeLeft = 0;

        uiManager.SetTimerText(_timeLeft);
    }
}