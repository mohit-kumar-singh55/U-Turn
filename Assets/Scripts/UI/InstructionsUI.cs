using UnityEngine;

public class InstructionsUI : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SceneLoader.LoadScene(3);
        }
    }
}
