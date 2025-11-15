using UnityEngine;

public class LapManager : MonoBehaviour
{
    [SerializeField] int totalLaps = 15;
    [SerializeField] string lapTriggerTag = "LapTrigger";       // Apply it on the vehicle
    [SerializeField] ParticleSystem[] lapVFXs;

    private int _lapsCompleted = 0;

    void Start()
    {
        UIManager.Instance.SetLapsText(_lapsCompleted, totalLaps);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(lapTriggerTag))
        {
            _lapsCompleted++;
            UIManager.Instance.SetLapsText(_lapsCompleted, totalLaps);
            if (lapVFXs.Length > 0) lapVFXs[Random.Range(0, lapVFXs.Length)].Play();
        }

        if (_lapsCompleted == totalLaps) GameManager.Instance.TriggerWin();
    }
}
