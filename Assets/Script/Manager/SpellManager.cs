using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

/// <summary>Gère tous les sorts.</summary>
public class SpellManager : NetworkBehaviour
{
	// *************** //
	// ** Variables ** // Toutes les variables sans distinctions
	// *************** //

	/// <summary>Montre à quelle portée les personnages vont être projetés avant de le lancer</summary>
	public SpellData selectedSpell;
	public bool spellSuccess = false;

	public bool isSpellCasting = false;

	public static SpellManager Instance;

	public EventHandler<PlayerArgs> changeTurnEvent;

	public CaseData lastCaseUsed;

	public List<string> spellName = new List<string>();
	public List<int> spellUse = new List<int>();

    int summonTargetNumber = 0;
    public bool absorbCasterFXDone = false;

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
		EventManager.newHoverEvent += OnNewHover;
		TurnManager.Instance.changeTurnEvent += OnChangeTurn;
	}

	void OnDisable()
	{
		if (LoadingManager.Instance != null && LoadingManager.Instance.isGameReady())
		{
			EventManager.newClickEvent -= OnNewClick;
			EventManager.newHoverEvent -= OnNewHover;
			TurnManager.Instance.changeTurnEvent -= OnChangeTurn;
		}
	}

	// *************** //
	// ** Events **    // Appel de fonctions au sein de ce script grâce à des events
	// *************** //

	void OnChangeTurn(object sender, PlayerArgs e)
	{ // Lorsqu'un joueur termine son tour
		for (int i = 0; i < spellUse.Count; i++)
		{
			spellUse[i] = 0;
		}
  }

  void OnNewHover(object sender, HoverArgs e)
	{
		if (GameManager.Instance.actualAction != PersoAction.isCasting
		    || HoverManager.Instance.hoveredCase == null)
			return;

        StartCoroutine(OnNewHoverDelay());
	}

    IEnumerator OnNewHoverDelay()
    {
        yield return new WaitForEndOfFrame();
        selectedSpell.ShowAllFeedbacks();
        SelectionManager.Instance.selectedPersonnage.RotateTowards(HoverManager.Instance.hoveredCase.gameObject);
    }

	void OnNewClick()
	{ // Lors d'un click sur une case

		if (GameManager.Instance.actualAction != PersoAction.isCasting)
			return;

		SpellCaseClick();
	}

	// *************** //
	// ** Fonctions ** // Fonctions réutilisables ailleurs
	// *************** //

	/// <summary>Active le tooltip pour le sort</summary>
	public void SpellTooltipON(int IDSpell)
	{
		UIManager.Instance.UIIsHovered = true;
		Tooltip.tooltipObj = ChooseSpell(IDSpell);
		UIManager.Instance.tooltip.SetActive(true);
	}

	/// <summary>Désactive le tooltip pour le sort</summary>
	public void SpellTooltipOFF()
	{
		UIManager.Instance.UIIsHovered = false;
		UIManager.Instance.tooltip.SetActive(false);
	}

	/// <summary>Montre comment le sort doit être lancé, un précast. 
	/// Mettre un chiffre correspondant à l'ordre des boutons de sorts du personnage (0 = spell1; 1 = spell2)</summary>
	public void SpellButtonClick(int IDSpell)
	{
		if (GameManager.Instance.actualAction == PersoAction.isCasting)
		{
			spellSuccess = false;
			StartCoroutine(SpellEnd());
		}

		if (SelectionManager.Instance.selectedCase == null)
			return;

		if (GameManager.Instance.currentPhase == Phase.Placement)
			return;

		selectedSpell = ChooseSpell(IDSpell);

		// enough PA? (global PA/mana)
		if (GameManager.Instance.currentPlayer == Player.Red && ManaManager.Instance.manaActuelRed < selectedSpell.costPA)
		{
			selectedSpell = null;
			return;
		}

		if (GameManager.Instance.currentPlayer == Player.Blue && ManaManager.Instance.manaActuelBlue < selectedSpell.costPA)
		{
			selectedSpell = null;
			return;
		}

		RpcFunctions.Instance.CmdSpellButtonClick(IDSpell);
	}

	[ClientRpc]
	public void RpcSpellButtonClick(int IDSpell)
	{
		PersoData selectedPersonnage = SelectionManager.Instance.selectedPersonnage;
		selectedPersonnage.animator.SetBool("Idle", false);
		selectedPersonnage.animator.SetBool("Cast", true);
		ManaManager.Instance.Actived();

		if (selectedSpell == null)// spell exist?
      return;

		if (selectedPersonnage == null && !selectedSpell.rotateSummon)// perso exist?
      return;

		GameManager.Instance.actualAction = PersoAction.isCasting;
		SelectionManager.Instance.DisablePersoSelection();
		TurnManager.Instance.DisableFinishTurn();

		selectedSpell.newRangeList();
		selectedSpell.newTargetList();
	}

	/// <summary>Le sort est lancé à un endroit</summary>
	public void SpellCaseClick()
	{
    if (selectedSpell.rotateSummon && !MenuRotateContexuel.Instance.gameObject.activeSelf) // rotate summon se lance après avoir utilisé le MenuRotateContextuel
    {
      selectedSpell.ApplyEffect(SummonManager.Instance.lastSummonInstancied.gameObject);
      return;
    }

    spellSuccess = false;

		CaseData hoveredCase = HoverManager.Instance.hoveredCase;

		if (((Statut.canTarget & hoveredCase.statut) != Statut.canTarget)
		    || (hoveredCase.personnageData != null && hoveredCase.personnageData.spellHit.Contains(selectedSpell))
		    || (hoveredCase.ballon != null && hoveredCase.ballon.spellHit.Contains(selectedSpell)))
		{
			StartCoroutine(SpellEnd());
			return;
		}

		for (int i = 0; i < SpellManager.Instance.spellName.Count; i++)
		{
			if (spellName[i] == selectedSpell.name && spellUse[i] >= selectedSpell.maxUsePerTurn)
			{
				StartCoroutine(SpellEnd());
				return;
			}
			if (spellName[i] == selectedSpell.name)
			{
				spellUse[i]++;
				UIManager.Instance.ChangeSpriteSpellButton(SelectionManager.Instance.selectedPersonnage);
			}
		}

		if (HoverManager.Instance.hoveredBallon)
		{
			HoverManager.Instance.hoveredBallon.spellHit.Add(selectedSpell);
		}
		if (HoverManager.Instance.hoveredPersonnage)
		{
			HoverManager.Instance.hoveredPersonnage.spellHit.Add(selectedSpell);
		}

		spellSuccess = true;
		GameManager.Instance.manaGlobalActual -= selectedSpell.costPA;

		if (ManaManager.Instance == null)
			return;

    ManaManager.Instance.ChangeActualMana(SelectionManager.Instance.selectedPersonnage.owner, selectedSpell.costPA);

    if (SummonManager.Instance.lastSummonInstancied != null && !selectedSpell.summonOnCross) // normal summon
		{
			SummonInvoc();
		}
		if (SummonManager.Instance.crossSummonList != null && selectedSpell.summonOnCross) // cross summon
		{
			foreach (SummonData item in SummonManager.Instance.crossSummonList)
			{
				if (item.transform.position.x > 500)
				{
					Destroy(item.gameObject);
					continue;
				}
				item.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
				item.GetComponent<Animator>().enabled = true;
				item.GetComponent<BoxCollider2D>().enabled = true;
				GameObject ownerCircle = item.originPoint.GetChild(0).gameObject;
				if (item.owner == Player.Red)
				{
					ownerCircle.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.4f);
				}
				if (item.owner == Player.Blue)
				{
					ownerCircle.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1, 0.4f);
				}
			}
			SummonManager.Instance.crossSummonList.Clear();
		}

        absorbCasterFXDone = false;

        foreach (CaseData obj in CaseManager.listAllCase)
		{
			if ((Statut.atAoE & obj.statut) == Statut.atAoE)
			{
				if (selectedSpell.nextSpell) // sert juste pour determiner l'ancienne case de la tornade pour la tourner
				{
					lastCaseUsed = obj;
				}
				if (((ObjectType.AllyPerso & selectedSpell.affectedTarget) == ObjectType.AllyPerso) && obj.personnageData != null && SelectionManager.Instance.selectedPersonnage != obj.personnageData)
				{
					selectedSpell.ApplyEffect(obj.personnageData.gameObject);
                    absorbCasterFXDone = true;
                }

				if (((ObjectType.Ballon & selectedSpell.affectedTarget) == ObjectType.Ballon) && obj.ballon != null)
				{
					selectedSpell.ApplyEffect(obj.ballon.gameObject);
				}

        if (((ObjectType.Invoc & selectedSpell.affectedTarget) == ObjectType.Invoc) && obj.summonData != null)
        {
          summonTargetNumber++;
          selectedSpell.ApplyEffect(obj.summonData.gameObject);
        }

        if (((ObjectType.Self & selectedSpell.affectedTarget) == ObjectType.Self) && obj.personnageData != null && SelectionManager.Instance.selectedPersonnage == obj.personnageData)
        {
          selectedSpell.ApplyEffect(obj.personnageData.gameObject);
        }
      }
    }

        if (selectedSpell.manaIncrementPerSummon != 0)
            ManaManager.Instance.ChangeActualMana(GameManager.Instance.currentPlayer, selectedSpell.manaIncrementPerSummon * summonTargetNumber);

		StartCoroutine(SpellEnd());
	}

	public void SummonInvoc(SummonData item = null)
	{
		SummonData lastSummonInstancied = SummonManager.Instance.lastSummonInstancied;
		lastSummonInstancied.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
		lastSummonInstancied.GetComponent<Animator>().enabled = true;
		SummonManager.Instance.AddSummon(lastSummonInstancied);
		GameObject ownerCircle = lastSummonInstancied.originPoint.GetChild(0).gameObject;
		if (lastSummonInstancied.owner == Player.Red)
		{
			ownerCircle.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.4f);
		}
		if (lastSummonInstancied.owner == Player.Blue)
		{
			ownerCircle.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1, 0.4f);
		}
	}

	public IEnumerator SpellEnd()
	{
    if (SelectionManager.Instance.selectedPersonnage == null)
    {
      yield return null;
      StopCoroutine(SpellEnd());
    }

    SelectionManager.Instance.selectedPersonnage.animator.SetBool("Cast", false);
		SelectionManager.Instance.selectedPersonnage.animator.SetBool("Idle", true);
		ManaManager.Instance.Desactived();
		ManaManager.Instance.SpellButtonFeedbackOFF();
		GameManager.Instance.actualAction = PersoAction.isWaiting;
		foreach (CaseData obj in CaseManager.listAllCase)
		{
			obj.ChangeStatut(Statut.None, Statut.atRange);
			obj.ChangeStatut(Statut.None, Statut.atAoE);
			obj.ChangeStatut(Statut.None, Statut.atPush);
			obj.ChangeStatut(Statut.None, Statut.canTarget);
		}
 
		if (!spellSuccess)
		{
			if (SummonManager.Instance.lastSummonInstancied != null) // sort indirect
			{
				DestroyImmediate(SummonManager.Instance.lastSummonInstancied.gameObject);
			}
			if (SummonManager.Instance.crossSummonList != null) // prévisu cross
			{
				foreach (SummonData item in SummonManager.Instance.crossSummonList)
				{
					DestroyImmediate(item.gameObject);
				}
				SummonManager.Instance.crossSummonList.Clear();
			}
		}
		SummonManager.Instance.lastSummonInstancied = null;
		spellSuccess = false;
    yield return new WaitForSeconds(0.5f);
    GameManager.Instance.actualAction = PersoAction.isSelected;

		SelectionManager.Instance.EnablePersoSelection();
		MoveBehaviour.Instance.StopAllCoroutines();
		StartCoroutine(TurnManager.Instance.EnableFinishTurn());
	}

	/// <summary>Cible le bon sort entre les boutons</summary>
	SpellData ChooseSpell(int IDSpell)
	{
		PersoData selectedPersonnage = SelectionManager.Instance.selectedPersonnage;
		if (selectedPersonnage == null)
			return null;

		if (IDSpell == 0)
			return SelectionManager.Instance.selectedPersonnage.Spell1;

		if (IDSpell == 1)
			return SelectionManager.Instance.selectedPersonnage.Spell2;

		return null;
	}

	private void summonObj(SummonData summon)
	{
		summon.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
		summon.GetComponent<Animator>().enabled = true;
		SummonManager.Instance.AddSummon(summon);
		GameObject ownerCircle = summon.originPoint.GetChild(0).gameObject;
		if (summon.owner == Player.Red)
		{
			ownerCircle.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.4f);
		}
		if (summon.owner == Player.Blue)
		{
			ownerCircle.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1, 0.4f);
		}
	}
}
