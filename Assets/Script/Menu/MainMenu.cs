using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Prototype.NetworkLobby;

public class MainMenu : MonoBehaviour
{

	public void PlayGame()
	{
		LobbyManager.Instance.mainMenuPanel.gameObject.SetActive(true);
		LobbyManager.Instance.mainMenuPanel.gameObject.GetComponent<LobbyMainMenu>().OnClickHost();
		//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void Parsec()
	{
		Application.OpenURL("https://parsecgaming.com/");
	}
}
