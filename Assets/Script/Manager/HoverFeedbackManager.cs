using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HoverFeedbackManager : NetworkBehaviour
{

	public static HoverFeedbackManager Instance;

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
		StartCoroutine(LateOnEnable());
	}

	IEnumerator LateOnEnable()
	{
		yield return new WaitForEndOfFrame();
		EventManager.newHoverEvent += OnNewHover;
	}

	void OnDisable()
	{
		EventManager.newHoverEvent -= OnNewHover;
	}

	void OnNewHover(object sender, HoverArgs e)
	{
		ClearMovement();

		if (e.hoveredPersonnage != null && e.hoveredPersonnage.persoCase != null && GameManager.Instance.actualAction != PersoAction.isCasting)
			ShowMovement(e.hoveredPersonnage);
	}

	/// <summary>Remet à 0 les cases de mouvement afficher par ShowMovement.</summary>
	public void ClearMovement()
	{
		foreach (CaseData newCase in CaseManager.Instance.GetAllCase())
		{
			newCase.ChangeStatut(Statut.None, Statut.canMovePrevisu);
		}
	}

	/// <summary>Colore les cases sur lesquelles ce personnage peut se déplacer au max de son déplacement.</summary>
	public void ShowMovement(PersoData perso)
	{
		List<CaseData> caseList = new List<CaseData>();
		int movement = 0;
		if (perso.owner == GameManager.Instance.currentPlayer)
			movement = perso.actualPointMovement;
		else
		{
			movement = perso.maxPointMovement;
		}
		caseList.Add(perso.persoCase);

		for (int i = 0; i < movement; i++)
		{
			List<CaseData> caseListCheck = new List<CaseData>();
			caseListCheck.AddRange(caseList);

			foreach (CaseData newCase in caseListCheck)
			{
				if (newCase.GetTopLeftCase() != null && newCase.GetTopLeftCase().casePathfinding == PathfindingCase.Walkable && !caseList.Contains(newCase.GetTopLeftCase()))
				{
					caseList.Add(newCase.GetTopLeftCase());
				}
				if (newCase.GetTopRightCase() != null && newCase.GetTopRightCase().casePathfinding == PathfindingCase.Walkable && !caseList.Contains(newCase.GetTopRightCase()))
				{
					caseList.Add(newCase.GetTopRightCase());
				}
				if (newCase.GetBottomLeftCase() != null && newCase.GetBottomLeftCase().casePathfinding == PathfindingCase.Walkable && !caseList.Contains(newCase.GetBottomLeftCase()))
				{
					caseList.Add(newCase.GetBottomLeftCase());
				}
				if (newCase.GetBottomRightCase() != null && newCase.GetBottomRightCase().casePathfinding == PathfindingCase.Walkable && !caseList.Contains(newCase.GetBottomRightCase()))
				{
					caseList.Add(newCase.GetBottomRightCase());
				}
			}
		}
		foreach (CaseData newCase in caseList)
		{
			newCase.ChangeStatut(Statut.canMovePrevisu);
		}
	}
}
