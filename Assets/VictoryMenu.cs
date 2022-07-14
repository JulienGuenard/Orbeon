using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prototype.NetworkLobby;
using UnityEngine.Networking;

public class VictoryMenu : NetworkBehaviour
{

	public GameObject autoLobby;

	public void Rejouer()
	{
		//NetworkManager.Shutdown();
		//	GameObject newAuto = (GameObject)Instantiate(autoLobby, transform.position, Quaternion.identity);
		//	StartCoroutine(MenuFade.Instance.FadeIn("MenuThenPlay"));
	}

	public void MenuPrincipal()
	{
		StartCoroutine(MenuFade.Instance.FadeIn("Menu"));
	}

	public void Quitter()
	{
		Application.Quit();
	}
}
