using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class PlacementBehaviour : NetworkBehaviour
{
    // *************** //
    // ** Variables ** //
    // *************** //

    public bool autoPlace;

	[SerializeField]
	Material redMat;
	[SerializeField]
	Material blueMat;

	[Tooltip("Index du personnage à placer")]
	int persoToPlaceNumber = -1;



	public static PlacementBehaviour Instance;

	public override void OnStartClient()
	{
		if (Instance == null)
			Instance = this;
		StartCoroutine(waitForInit());
	}

	IEnumerator waitForInit()
	{
		while (!LoadingManager.Instance.isGameReady())
		{
			yield return new WaitForEndOfFrame();
		}
		Init();
	}

	private void Init()
	{
		EventManager.newClickEvent += OnNewClick;
		StartCoroutine(LateOnEnable());
       
	}

	IEnumerator LateOnEnable()
	{
		yield return new WaitForEndOfFrame();
       
        TurnManager.Instance.changeTurnEvent += OnChangeTurn;

        
    }

	void OnDisable()
	{
		if (LoadingManager.Instance != null && LoadingManager.Instance.isGameReady())
		{
			EventManager.newClickEvent -= OnNewClick;
			TurnManager.Instance.changeTurnEvent -= OnChangeTurn;
		}
	}

	void Update()
	{
        if (GameManager.Instance.currentPhase == Phase.Deplacement)
            this.enabled = false;

        if (autoPlace && RosterManager.Instance != null && RosterManager.Instance.listHero.Count == 8 && RosterManager.Instance.listHeroPlaced.Count == 0)
            AutoPlace();

        if (TurnManager.Instance == null)
			return;
		
		if (GameManager.Instance.currentPhase == Phase.Placement)
		{
			int allPlaced = 0;

			foreach (PersoData perso in RosterManager.Instance.listHeroPlaced)
			{
				if (GameManager.Instance.currentPlayer == Player.Red && perso.owner == Player.Red)
					allPlaced++;

				if (GameManager.Instance.currentPlayer == Player.Blue && perso.owner == Player.Blue)
					allPlaced++;
			}

			if (allPlaced == 4)
			{
				StartCoroutine(TurnManager.Instance.EnableFinishTurn());
			} else
			{
				TurnManager.Instance.DisableFinishTurn();
			}
		}
	}

	public void OnNewClick()
	{
		// Lors d'un click sur une case
		Phase currentPhase = TurnManager.Instance.currentPhase;
		Player currentPlayer = TurnManager.Instance.currentPlayer;
		CaseData hoveredCase = HoverManager.Instance.hoveredCase;

		if (currentPhase != Phase.Placement)
			return;

		// Place un perso ami sur une case vide
		if (currentPhase == Phase.Placement &&
		    HoverManager.Instance.hoveredCase != null && HoverManager.Instance.hoveredPersonnage == null &&
		    hoveredCase.casePathfinding == PathfindingCase.Walkable)
		{
			Statut statut = hoveredCase.statut;

			if ((Statut.placementRed & statut) == Statut.placementRed && currentPlayer == Player.Red)
			{
				CreatePerso(currentPhase, currentPlayer);
			}
			if ((Statut.placementBlue & statut) == Statut.placementBlue && currentPlayer == Player.Blue)
			{
				CreatePerso(currentPhase, currentPlayer);
			}
		}
        // si on selectionne un personnage à partir d'un portrait
        else if (HoverManager.Instance.hoveredPersonnage != null && HoverManager.Instance.hoveredCase == null)
		{
			SelectionManager.Instance.selectedPersonnage = HoverManager.Instance.hoveredPersonnage; // total forcage, préférer SelectPerso() in-game
			InfoPerso.Instance.PersoSelected(SelectionManager.Instance.selectedPersonnage); // total forcage, préférer SelectPerso() in-game

      UIManager.Instance.ChangeSpriteSpellButton(SelectionManager.Instance.selectedPersonnage);
      InfoPerso.Instance.stats.updatePm(SelectionManager.Instance.selectedPersonnage.actualPointMovement);
			InfoPerso.Instance.stats.updatePr(SelectionManager.Instance.selectedPersonnage.actualPointResistance);
			InfoPerso.Instance.stats.updatePo(SelectionManager.Instance.selectedPersonnage.shotStrenght);
		}

        // Fait disparaître un perso placé sur une case
      else if (HoverManager.Instance.hoveredPersonnage != null && HoverManager.Instance.hoveredCase != null && hoveredCase.personnageData != null &&
		             hoveredCase.personnageData.owner == currentPlayer)
		{
			SelectionManager.Instance.selectedPersonnage = HoverManager.Instance.hoveredPersonnage; // total forcage, préférer SelectPerso() in-game
			InfoPerso.Instance.PersoSelected(SelectionManager.Instance.selectedPersonnage);
			SelectionManager.Instance.selectedPersonnage.persoCase = null;

      UIManager.Instance.ChangeSpriteSpellButton(SelectionManager.Instance.selectedPersonnage);
      InfoPerso.Instance.PersoSelected(SelectionManager.Instance.selectedPersonnage);
			InfoPerso.Instance.PersoRemoved(SelectionManager.Instance.selectedPersonnage);
			ChangePersoPosition(null, SelectionManager.Instance.selectedPersonnage); // total forcage, préférer SelectPerso() in-game
		}
	}

	void OnChangeTurn(object sender, PlayerArgs e)
	{ // Lorsqu'un joueur termine son tour
		switch (e.currentPhase)
		{
			case Phase.Deplacement:
				break;
			case Phase.Placement:
				break;
		}
	}

    void AutoPlace()
    {
        CreatePersoPlacement(GameObject.Find("9 2").GetComponent<CaseData>(), RosterManager.Instance.listHero[0]);
        CreatePersoPlacement(GameObject.Find("7 4").GetComponent<CaseData>(), RosterManager.Instance.listHero[1]);
        CreatePersoPlacement(GameObject.Find("7 6").GetComponent<CaseData>(), RosterManager.Instance.listHero[2]);
        CreatePersoPlacement(GameObject.Find("9 8").GetComponent<CaseData>(), RosterManager.Instance.listHero[3]);
        CreatePersoPlacement(GameObject.Find("13 2").GetComponent<CaseData>(), RosterManager.Instance.listHero[4]);
        CreatePersoPlacement(GameObject.Find("15 4").GetComponent<CaseData>(), RosterManager.Instance.listHero[5]);
        CreatePersoPlacement(GameObject.Find("15 6").GetComponent<CaseData>(), RosterManager.Instance.listHero[6]);
        CreatePersoPlacement(GameObject.Find("13 8").GetComponent<CaseData>(), RosterManager.Instance.listHero[7]);
    }

	public void CreatePerso(Phase currentPhase, Player currentPlayer)
	{ // On créé un personnage sur une case
		CreatePersoPlacement(HoverManager.Instance.hoveredCase, SelectionManager.Instance.selectedPersonnage);
	}

	public void CreatePersoPlacement(CaseData hoveredCase, PersoData selectedPersonnage)
	{ //
        if (SelectionManager.Instance.selectedPersonnage == null)
            return;

		if (hoveredCase.casePathfinding == PathfindingCase.Walkable)
		{
			selectedPersonnage.transform.position = hoveredCase.transform.position - selectedPersonnage.originPoint.transform.localPosition;
			if (!RosterManager.Instance.listHeroPlaced.Contains(selectedPersonnage))
			{
				RosterManager.Instance.listHeroPlaced.Add(selectedPersonnage);
			}

			if (selectedPersonnage.GetComponent<PersoData>().owner == Player.Red)
			{
				selectedPersonnage.GetComponent<PersoData>().ChangeRotation(Direction.SudEst);
				// mettre icone perso rouge
			} else
			{
				selectedPersonnage.GetComponent<PersoData>().ChangeRotation(Direction.NordOuest);
				// mettre icone perso bleu
			}
			hoveredCase.personnageData = selectedPersonnage;
			HoverManager.Instance.hoveredPersonnage = selectedPersonnage;
			InfoPerso.Instance.PersoPlaced(selectedPersonnage);
		}
	}

	public void ChangePersoPosition(CaseData hoveredCase, PersoData selectedPersonnage)
	{ // Change la position d'un personnage déjà placé vers la case où a cliqué le joueur possesseur.

		if (hoveredCase == null)
		{
			RosterManager.Instance.listHeroPlaced.Remove(selectedPersonnage);
			selectedPersonnage.transform.position = Vector3.one * 999;
			return;
		}
		if (hoveredCase.GetComponent<CaseData>().casePathfinding == PathfindingCase.Walkable || selectedPersonnage != null)
		{
			SelectionManager.Instance.selectedCase = hoveredCase;
			SelectionManager.Instance.selectedLastPersonnage = SelectionManager.Instance.selectedPersonnage;
			SelectionManager.Instance.selectedLastPersonnage.transform.position = SelectionManager.Instance.selectedCase.transform.position + selectedPersonnage.originPoint.transform.position;
			if (selectedPersonnage != null && SelectionManager.Instance.selectedLastCase != null)
			{
				SelectionManager.Instance.selectedPersonnage = selectedPersonnage;
				SelectionManager.Instance.selectedPersonnage.transform.position = SelectionManager.Instance.selectedLastCase.transform.position + selectedPersonnage.originPoint.transform.position;
			} else
			{
				SelectionManager.Instance.selectedPersonnage = selectedPersonnage;
			}
        }
		SelectionManager.Instance.Deselect();
	}
}
