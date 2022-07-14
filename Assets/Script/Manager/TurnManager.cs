using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;

/// <summary>Gère les changements de tours, les changements de phases, et qui joue.</summary>
public class TurnManager : NetworkBehaviour
{

	// *************** //
	// ** Variables ** // Toutes les variables sans distinctions
	// *************** //

	public int TurnNumber;
	public EventHandler<PlayerArgs> changeTurnEvent;
	public Player currentPlayer = Player.Red;
	public Phase currentPhase = Phase.Placement;
	GameObject finishTurnButton;
	public Animator visualFeedback;

	bool canChangeTurn = true;

	public static TurnManager Instance;

	// ******************** //
	// ** Initialisation ** // Fonctions de départ, non réutilisable
	// ******************** //

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

		StartCoroutine(InitGame());
	}

	IEnumerator InitGame()
	{
		yield return new WaitForSeconds(0.01f);
		RpcFunctions.Instance.CmdFirstTurn();
		finishTurnButton = GameObject.Find("finishTurn");
	}

	// *************** //
	// ** Fonctions ** // Fonctions réutilisables ailleurs
	// *************** //

	/// <summary>Passe au tour suivant.</summary>
	public void ChangeTurn()
	{
		if (!SynchroManager.Instance.canPlayTurn())
		{
			return;
		}
		if (!canChangeTurn)
		{
			return;
		}

		RpcFunctions.Instance.CmdChangeTurn();
	}

	[ClientRpc]
	public void RpcFirstTurn()
	{
		StartCoroutine(waitForEvent());
	}

	IEnumerator waitForEvent()
	{
		while (!LoadingManager.Instance.isGameReady())
			yield return new WaitForEndOfFrame();

		changeTurnEvent(this, new PlayerArgs(currentPlayer, currentPhase));
	}

	[ClientRpc]
	public void RpcChangeTurn()
	{
		TackleBehaviour.Instance.SetupRandomList();

		switch (currentPlayer)
		{
			case Player.Red:
				currentPlayer = Player.Blue;
				break;
			case Player.Blue:
				currentPlayer = Player.Red;
				break;
		}
		TurnNumber++;

		if (TurnNumber == 2)
			ChangePhase(Phase.Deplacement);

    if (HoverManager.Instance.hoveredCase)
    {
      HoverManager.Instance.changeColorExit(currentPhase);
      HoverManager.Instance.changeSpriteExit();
    }
    changeTurnEvent(this, new PlayerArgs(currentPlayer, currentPhase));
	}

	/// <summary>Passe à la phase indiquée.</summary>
	public void ChangePhase(Phase newPhase)
	{
		currentPhase = newPhase;
	}

	/// <summary>Désactive et grise le bouton "Passer le tour"</summary>
	public void DisableFinishTurn()
	{
		if (!canChangeTurn)
			return;

        if (finishTurnButton == null)
            return;
		
		if (finishTurnButton.GetComponent<Button>().interactable != false)
		{
 			finishTurnButton.GetComponent<Button>().interactable = false;
			canChangeTurn = false;
		}
	}

	/// <summary>Réactive et dégrise le bouton "Passer le tour"</summary>
	public IEnumerator EnableFinishTurn()
	{
		if (canChangeTurn)
			yield return null;
		
		finishTurnButton = GameObject.Find("finishTurn");
		if (finishTurnButton.GetComponent<Button>().interactable != true)
		{
			finishTurnButton.GetComponent<Button>().enabled = false;
			yield return new WaitForSeconds(0.01f);
			finishTurnButton.GetComponent<Button>().enabled = true;
			finishTurnButton.GetComponent<Image>().color = new Color(0.8f, 0.7f, 0f);
			finishTurnButton.GetComponent<Button>().interactable = true;
			canChangeTurn = true;
		}
	}

	/// <summary>Affiche combien de tours se sont passés depuis le début de la manche.</summary>
	public int GetTurnNumber()
	{
		return TurnNumber;
	}

	/// <summary>Affiche combien de cycles de tours (les deux joueurs ont joués leurs tours) se sont passés depuis le début de la manche.</summary>
	public int GetCycleTurnNumber()
	{
		return TurnNumber / 2;
	}

	public void HoverButton()
	{
		UIManager.Instance.UIIsHovered = true;
	}

	public void UnhoverButton()
	{
		UIManager.Instance.UIIsHovered = false;
	}
}

