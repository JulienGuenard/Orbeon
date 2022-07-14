using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MusicManager : NetworkBehaviour {

    public static MusicManager Instance;

    public AudioClip musicPlacement;
    public AudioClip musicDeplacement;
    public AudioClip musicDeplacement2;

    AudioSource audioS;

    int musicInt = 1;

    public override void OnStartClient()
    {
        if (Instance == null)
            Instance = this;
        StartCoroutine(waitForInit());
    }

    IEnumerator waitForInit()
    {
        while (!LoadingManager.Instance.isGameReady())
            yield return new WaitForEndOfFrame();
        Init();
    }

    private void Init()
    {
        audioS = GameObject.Find("AudioSource").GetComponent<AudioSource>();
        audioS.PlayOneShot(musicPlacement);
        StartCoroutine(FadeIn(0.01f));
    }

    private IEnumerator FadeIn(float speed)
    {
        audioS.volume = 0;
        for (float i = 0; i < 1; i += speed)
        {
            yield return new WaitForSeconds(0.01f);
            audioS.volume = i;
        }
    }

    private IEnumerator FadeOut(float speed, AudioClip clip)
    {
        audioS.volume = 1;
        for (float i = 1; i > 0; i -= speed)
        {
            yield return new WaitForSeconds(0.01f);
            audioS.volume = i;
        }
        audioS.Stop();
        audioS.PlayOneShot(clip);
        StartCoroutine(FadeIn(0.01f));

        if (clip == musicDeplacement)
            StartCoroutine(SwitchMusic(clip.length, musicDeplacement2));

        if (clip == musicDeplacement2)
            StartCoroutine(SwitchMusic(clip.length, musicDeplacement));
    }

    IEnumerator SwitchMusic(float lenght, AudioClip clip)
    {
        yield return new WaitForSeconds(lenght - 1f);
        StartCoroutine(FadeOut(0.05f, clip));
    }

    private void Update()
    {
        if (GameManager.Instance.currentPhase == Phase.Deplacement && musicInt != 2)
        {
            musicInt = 2;
            StartCoroutine(FadeOut(0.05f, musicDeplacement));
        }
    }
}
