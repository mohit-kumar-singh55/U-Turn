using UnityEngine;

/// <summary>
/// シーンをまたいでBGMを再生するために使用
/// </summary>
public class BgmPlayer : MonoBehaviour
{
    public static BgmPlayer Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
