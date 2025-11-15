using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct MenuBGMClip
{
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume;
}

public class MainMenuController : MonoBehaviour
{
    public GameObject fader;
    public Image faderImage;
    public GameObject mainPanel;
    public GameObject carPointLight;
    public AudioSource menuBGM;
    public MenuBGMClip[] menuBGMClips;

    void Start()
    {
        faderImage = fader.GetComponent<Image>();
        StartCoroutine(FlikkerLight());

        if (menuBGM == null || menuBGMClips.Length == 0)
        {
            Debug.LogWarning("No Audio Source or BGM Provided!");
            enabled = false;
            return;
        }

        PlayBGM();
    }

    void LateUpdate()
    {
        if (!menuBGM.isPlaying) PlayBGM();
    }

    public void LoadNewGame()
    {
        FadeOutScreen();
    }

    public void Quit() => Application.Quit();

    void FadeOutScreen()
    {
        if (!fader || !faderImage) return;

        fader.SetActive(true);
        StartCoroutine(SetColorAlphaValueAndVolume());
    }

    public void PlayBGM()
    {
        MenuBGMClip clip = menuBGMClips[UnityEngine.Random.Range(0, menuBGMClips.Length)];
        menuBGM.PlayOneShot(clip.Clip, clip.Volume);
    }


    /// <summary>
    /// スクリーンをフェードアウトし、BGMを停止する
    /// </summary>
    IEnumerator SetColorAlphaValueAndVolume()
    {
        while (faderImage.color.a < 1f)
        {
            Color newColor = faderImage.color;
            newColor.a += .1f;
            faderImage.color = newColor;

            if (menuBGM.isPlaying && menuBGM.volume > 0) menuBGM.volume -= .1f;

            yield return new WaitForSeconds(.04f);
        }

        menuBGM.Stop(); // フェードアウト後に音声を完全に停止させる
        SceneLoader.LoadScene(2);
    }

    IEnumerator FlikkerLight()
    {
        while (true)
        {
            carPointLight.SetActive(UnityEngine.Random.Range(0f, 1f) >= .5f);
            yield return new WaitForSeconds(.12f);
        }
    }
}
