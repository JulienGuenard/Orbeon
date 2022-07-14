using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Prototype.NetworkLobby;
using System;

public class AutoLobby : MonoBehaviour
{       
    string sceneName;

	void Awake()
	{
        sceneName = SceneManager.GetActiveScene().name;
		if (GameObject.FindObjectOfType<LobbyManager>() != null)
		{
			Destroy(this.gameObject);
			return;
		}
		StartCoroutine(ClearErrors());
		DontDestroyOnLoad(this.gameObject);
		SceneManager.LoadScene("Lobby réseau", LoadSceneMode.Single);
	}

	void Update()
	{
		if (SceneManager.GetActiveScene().name == "Lobby réseau")
			LaunchGame();
	}

	IEnumerator ClearErrors()
	{
		yield return new WaitForSeconds(0.02f);
		Debug.ClearDeveloperConsole();  
	}

	void LaunchGame()
	{
        GameObject.Find("LobbyManager").GetComponent<LobbyManager>().playScene = sceneName;
        GameObject.Find("MainPanel").GetComponent<LobbyMainMenu>().OnClickHost();
		Destroy(this.gameObject);
	}
}
