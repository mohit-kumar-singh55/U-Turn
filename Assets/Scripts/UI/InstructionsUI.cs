using UnityEngine;

public class InstructionsUI : MonoBehaviour
{
    private const string INSTRUCTION_KEY = "Instructions";          // 0 = false (first time), 1 = true (not first time)

    void Awake()
    {
        // プレイヤーがゲームを初めて起動するときだけ指示uiを表示
        if (!CheckIfFirstTime())
        {
            gameObject.SetActive(false);
            return;
        }
        else Time.timeScale = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SavePref();
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }
    }

    bool CheckIfFirstTime() => !PlayerPrefs.HasKey(INSTRUCTION_KEY) || PlayerPrefs.GetInt(INSTRUCTION_KEY) == 0;

    void SavePref()
    {
        PlayerPrefs.SetInt(INSTRUCTION_KEY, 1);
        PlayerPrefs.Save();
    }
}
