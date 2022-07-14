using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Prototype.NetworkLobby;
using UnityEngine.SceneManagement;

public class MenuFade : MonoBehaviour
{

	public static MenuFade Instance;

	Image MenuFadeBlack;
    Animator MenuFadeBlackAnimator;

    void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		} else
		{
			Destroy(this.gameObject);
		}

		DontDestroyOnLoad(this.gameObject);
		MenuFadeBlack = GameObject.Find("MenuFadeBlack").GetComponent<Image>();
        MenuFadeBlackAnimator = GameObject.Find("MenuFadeBlack").GetComponent<Animator>();
    }

	public IEnumerator FadeIn(string mode)
	{
        MenuFadeBlackAnimator.GetComponent<Animator>().SetTrigger("FadeIn");

			yield return new WaitForSeconds(1f);


		if (mode == "Play")
			LobbyManager.Instance.StartHost();

		if (mode == "MenuThenPlay")
			SceneManager.LoadScene("Lobby réseau", LoadSceneMode.Single);

		if (mode == "Menu")
		{
			SceneManager.LoadScene("Lobby réseau", LoadSceneMode.Single);
			StartCoroutine(FadeOut());
		}
        yield break;
    }

	public IEnumerator FadeOut()
	{
        MenuFadeBlackAnimator.GetComponent<Animator>().SetTrigger("FadeOut");
        yield break;

        for (float i = 100f; i > -5f; i -= 3f)
		{
			yield return new WaitForSeconds(0.01f);
			MenuFadeBlack.color = new Color(i / 100f, i / 100f, i / 100f, i / 100f);
		}
	}
}
