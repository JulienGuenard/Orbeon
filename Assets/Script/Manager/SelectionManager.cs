using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Gère la selection du joueur, que ce soit un personnage selectionné, une case ou un ballon.</summary>
public class SelectionManager : NetworkBehaviour
{

	// *************** //
	// ** Variables ** // Toutes les variables sans distinctions
	// *************** //

	public CaseData selectedLastCase;
	public PersoData selectedLastPersonnage;
	public BallonData selectedBallon;
	public CaseData selectedCase;
	public PersoData selectedPersonnage;

	bool isDisablePersoSelection = false;
  
	public static SelectionManager Instance;

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
		Init();
	}

	private void Init()
	{
		EventManager.newClickEvent += OnNewClick;
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

	private void Update()
	{
		if (selectedPersonnage == null)
    {
        UIManager.Instance.ResetSpells();
      return;
    }

    if (selectedPersonnage.Spell1 != null)
		{
			if ((selectedPersonnage.owner == Player.Red && ManaManager.Instance.manaActuelRed < selectedPersonnage.Spell1.costPA)
			    || (selectedPersonnage.owner == Player.Blue && ManaManager.Instance.manaActuelBlue < selectedPersonnage.Spell1.costPA))
			{
				UIManager.Instance.GreyButton("spell 1");
			} else
			{
				//	UIManager.Instance.UngreyButton("spell 1");
			}
		}

		if (selectedPersonnage.Spell2 != null)
		{
			if ((selectedPersonnage.owner == Player.Red && ManaManager.Instance.manaActuelRed < selectedPersonnage.Spell2.costPA)
			    || (selectedPersonnage.owner == Player.Blue && ManaManager.Instance.manaActuelBlue < selectedPersonnage.Spell2.costPA))
			{
				UIManager.Instance.GreyButton("spell 2");
			} else
			{
				//	UIManager.Instance.UngreyButton("spell 2");
			}
		}
	}

	// *************** //
	// ** Events **    // Appel de fonctions au sein de ce script grâce à des events
	// *************** //

	public void OnNewClick()
	{ // Lors d'un click sur une case

		if (isDisablePersoSelection || GameManager.Instance.actualAction == PersoAction.isCasting)
			return;

    PersoData hoveredPersonnage = HoverManager.Instance.hoveredPersonnage;
		Phase currentPhase = TurnManager.Instance.currentPhase;
		Player currentPlayer = TurnManager.Instance.currentPlayer;
		PersoAction actualAction = GameManager.Instance.actualAction;
		CaseData hoveredCase = HoverManager.Instance.hoveredCase;
		List<Transform> pathes = MoveBehaviour.Instance.movePathes;
		Color selectedColor = ColorManager.Instance.selectedColor;
		Color moveColor = ColorManager.Instance.moveColor;
		Color caseColor = ColorManager.Instance.caseColor;

    selectedLastCase = selectedCase;
		switch (currentPhase)
		{
			case (Phase.Placement):
      UIManager.Instance.ChangeSpriteSpellButton(selectedPersonnage);
      return; // c'est le scriptPlacementBehaviour qui s'occupe des clicks de phase de placement
			case (Phase.Deplacement):
				if (hoveredPersonnage != null && hoveredPersonnage.owner == currentPlayer)
				{ // changement de personnage selectionné
        if (hoveredPersonnage.actualPointResistance <= 0)
        {
          return;
        }
        SelectPerso(hoveredCase, hoveredPersonnage, selectedColor, currentPhase, currentPlayer, actualAction);
				}
				break;
		}
		if (selectedPersonnage != null)
		{
			InfoPerso.Instance.stats.updatePm(Instance.selectedPersonnage.actualPointMovement);
			InfoPerso.Instance.stats.updatePr(Instance.selectedPersonnage.actualPointResistance);
			InfoPerso.Instance.stats.updatePo(Instance.selectedPersonnage.shotStrenght);
		}
	}

	void OnChangeTurn(object sender, PlayerArgs e)
	{ // Lorsqu'un joueur termine son tour
		if (TurnManager.Instance.TurnNumber != 0)
			Deselect();

		switch (e.currentPhase)
		{
			case Phase.Deplacement:
				ResetSelection(ColorManager.Instance.caseColor);
				break;
			case Phase.Placement:
				StartCoroutine(preSelectFirstPerso());
				break;
		}
	}

	IEnumerator preSelectFirstPerso()
	{
		while (RosterManager.Instance.listHero.Count != 8)
			yield return new WaitForSeconds(0.01f);

		SelectPersoInPortraits();
	}

	// *************** //
	// ** Fonctions ** // Fonctions réutilisables ailleurs
	// *************** //

	public void ResetSelection(Color caseColor)
	{
		if (selectedCase != null)
		{
			selectedCase.ChangeStatut(Statut.None, Statut.isSelected);
		}
		selectedCase = null;
		selectedPersonnage = null;
	}

	public void Deselect()
	{
		CaseManager.Instance.RemovePath();
		MoveBehaviour.Instance.movePathes.Clear();
		GameManager.Instance.actualAction = PersoAction.isSelected;
		UIManager.Instance.HideStats();

		if (selectedLastCase != null)
			selectedLastCase.ChangeStatut(Statut.None, Statut.isSelected);

    UIManager.Instance.ResetSpells();
		selectedPersonnage = null;
		selectedCase = null;
	}

	public void SelectPerso(CaseData hoveredCase, PersoData hoveredPersonnage, Color selectedColor, Phase currentPhase, Player currentPlayer, PersoAction actualAction)
	{
		Deselect();
		selectedPersonnage = hoveredPersonnage;
		selectedCase = selectedPersonnage.persoCase;

		UIManager.Instance.ChangeSpriteSpellButton(selectedPersonnage);
		InfoPerso.Instance.PersoSelected(hoveredPersonnage);

		selectedCase.ChangeStatut(Statut.isSelected);
		GameManager.Instance.actualAction = PersoAction.isSelected;

        if (selectedLastPersonnage != null)
        selectedLastPersonnage.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 1);

        if (GameManager.Instance.currentPhase == Phase.Placement)
            selectedPersonnage.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0);

        selectedLastPersonnage = selectedPersonnage;
    }

	public void DisablePersoSelection()
	{
		isDisablePersoSelection = true;
	}

	public void EnablePersoSelection()
	{
		isDisablePersoSelection = false;
	}

	public void SelectPersoInPortraits()
	{
		if (TurnManager.Instance.currentPlayer == Player.Red)
		{
			selectedPersonnage = RosterManager.Instance.listHero[0];

			InfoPerso.Instance.stats.updatePm(selectedPersonnage.actualPointMovement);
			InfoPerso.Instance.stats.updatePr(selectedPersonnage.actualPointResistance);
			InfoPerso.Instance.stats.updatePo(selectedPersonnage.shotStrenght);
		}
		if (TurnManager.Instance.currentPlayer == Player.Blue)
		{
			selectedPersonnage = RosterManager.Instance.listHero[4];

			InfoPerso.Instance.stats.updatePm(selectedPersonnage.actualPointMovement);
			InfoPerso.Instance.stats.updatePr(selectedPersonnage.actualPointResistance);
			InfoPerso.Instance.stats.updatePo(selectedPersonnage.shotStrenght);
		}
	}
}
