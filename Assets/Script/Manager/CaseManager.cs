using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Networking;

/// <summary>Gère toute les cases du jeu, et permet de connaître la distance qui les sépare les uns des autres en x et y avec xCaseOffset et yCaseOffset.</summary>
public class CaseManager : NetworkBehaviour
{

	// *************** //
	// ** Variables ** // Toutes les variables sans distinctions
	// *************** //
	[Header("  Cases")]
	[Tooltip("La liste de toutes les cases de la grille.")]
	[ReadOnly]
	static public List<CaseData> listAllCase = new List<CaseData>();
	[Tooltip("Distance entre les cases en x.")]
	public float xCaseOffset = 1;
	[Tooltip("Distance entre les cases en y.")]
	public float yCaseOffset = 0.3f;

	[HideInInspector] public static CaseManager Instance;

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

	void Init()
	{
		if (listAllCase.Count != 0)
			listAllCase.Clear();
      
		foreach (GameObject cubeNew in GameObject.FindGameObjectsWithTag("case"))
		{
			listAllCase.Add(cubeNew.GetComponent<CaseData>());
			cubeNew.GetComponent<SpriteRenderer>().sortingOrder = -120 + int.Parse(cubeNew.name.Substring(0, 2));
		}
	}

	void Update()
	{
		/*foreach (CaseData obj in CaseManager.Instance.GetAllCase())
		{
			obj.ChangeColorByStatut();
		}*/
	}

    // *************** //
    // ** Fonctions ** // Fonctions réutilisables ailleurs
    // *************** //

    /// <summary>Remet la valeur de toutes les cases par défaut</summary> 
    void ClearAllCases()
	{
		foreach (CaseData newCase in listAllCase)
		{
			DisableAllColliders();
			newCase.ballon = null;
			newCase.casePathfinding = PathfindingCase.Walkable;
			newCase.personnageData = null;
			newCase.statut = 0;
			EnableAllColliders();
		}
	}

	/// <summary>Colore toutes les cases de la couleur choisi.</summary> 
	public void PaintAllCase(Color newColor)
	{
		foreach (CaseData newCase in listAllCase)
		{
			newCase.GetComponent<SpriteRenderer>().color = newColor;
		}
	}

	/// <summary>Change le sprite de toutes les cases avec le sprite choisi.</summary> 
	public void ChangeSpriteAllCase(Sprite newSprite)
	{
		foreach (CaseData newCase in listAllCase)
		{
			newCase.GetComponent<SpriteRenderer>().sprite = newSprite;
		}
	}

	/// <summary>Obtenir toutes les cases du jeu. </summary>
	public List<CaseData> GetAllCase()
	{
		List<CaseData> newList = new List<CaseData>();

		foreach (CaseData newCase in listAllCase)
		{
			newList.Add(newCase);
		}

		return newList;
	}

	/// <summary>Obtenir toutes les cases possédant un ballon.</summary>
	public List<CaseData> GetAllCaseWithBallon()
	{
		List<CaseData> newList = new List<CaseData>();

		foreach (CaseData newCase in listAllCase)
		{
			if (newCase.ballon != null)
				newList.Add(newCase);
		}
		return newList;
	}

	/// <summary>Obtenir toutes les cases possédant un personnage.</summary>
	public List<CaseData> GetAllCaseWithPersonnage()
	{
		List<CaseData> newList = new List<CaseData>();

		foreach (CaseData newCase in listAllCase)
		{
			if (newCase.personnageData != null)
				newList.Add(newCase);
		}
		return newList;
	}

	/// <summary>Obtenir toutes les cases sans personnage et sans ballon.</summary>
	public List<CaseData> GetAllCaseWithNothing()
	{
		List<CaseData> newList = new List<CaseData>();

		foreach (CaseData newCase in listAllCase)
		{
			if (newCase.personnageData == null && newCase.ballon == null)
				newList.Add(newCase);
		}
		return newList;
	}

	/// <summary>Obtenir toutes les cases avec le statut choisi.</summary>
	public List<CaseData> GetAllCaseWithStatut(Statut statut)
	{
		List<CaseData> newList = new List<CaseData>();

		foreach (CaseData newCase in listAllCase)
		{
			if ((statut & newCase.statut) == statut)
				newList.Add(newCase);
		}
		return newList;
	}

	/// <summary>Obtenir toutes les cases avec la couleur choisie.</summary>
	public List<CaseData> GetAllCaseWithColor(Color color)
	{
		List<CaseData> newList = new List<CaseData>();

		foreach (CaseData newCase in listAllCase)
		{
			SpriteRenderer SpriteR = newCase.GetComponent<SpriteRenderer>();
			if (SpriteR.color == color)
				newList.Add(newCase);
		}
		return newList;
	}

	public void RemovePath()
	{ // Cache la route de déplacement
		foreach (CaseData newCase in listAllCase)
		{
			newCase.ChangeStatut(Statut.None, Statut.canMove);
			newCase.ChangeStatut(Statut.None, Statut.canBeTackled);
		}
	}

	public void DisableAllColliders()
	{ // Cache la route de déplacement

		foreach (CaseData newCase in listAllCase)
		{
			newCase.GetComponent<PolygonCollider2D>().enabled = false;
		}
	}

	public void EnableAllColliders()
	{ // Cache la route de déplacement
		foreach (CaseData newCase in listAllCase)
		{
			if (newCase != null)
				newCase.GetComponent<PolygonCollider2D>().enabled = true;
		}
	}

	public bool CheckAdjacent(GameObject firstObj, GameObject secondObj)
	{ // Cette condition check si firstObj est à côté de secondObj (1case)

		float xCaseOffset = CaseManager.Instance.xCaseOffset;
		float yCaseOffset = CaseManager.Instance.yCaseOffset;

		float firstPosX = 0;
		float secondPosX = 0;
		float firstPosY = 0;
		float secondPosY = 0;

		// l'objet est-il un personnage ?
		if (firstObj.GetComponent<PersoData>() != null && firstObj.GetComponent<PersoData>().persoCase != null)
		{
			firstPosX = firstObj.GetComponent<PersoData>().persoCase.transform.position.x;
			firstPosY = firstObj.GetComponent<PersoData>().persoCase.transform.position.y;
		}

		if (secondObj.GetComponent<PersoData>() != null && secondObj.GetComponent<PersoData>().persoCase != null)
		{
			secondPosX = secondObj.GetComponent<PersoData>().persoCase.transform.position.x;
			secondPosY = secondObj.GetComponent<PersoData>().persoCase.transform.position.y;
		}
		// l'objet est-il un ballon ?
		if (firstObj.GetComponent<BallonData>() != null && firstObj.GetComponent<BallonData>().ballonCase != null)
		{
			firstPosX = firstObj.GetComponent<BallonData>().ballonCase.transform.position.x;
			firstPosY = firstObj.GetComponent<BallonData>().ballonCase.transform.position.y;
		}

		if (secondObj.GetComponent<BallonData>() != null && secondObj.GetComponent<BallonData>().ballonCase != null)
		{
			secondPosX = secondObj.GetComponent<BallonData>().ballonCase.transform.position.x;
			secondPosY = secondObj.GetComponent<BallonData>().ballonCase.transform.position.y;
		}
		// l'objet est-il une case ?
		if (firstObj.GetComponent<CaseData>() != null)
		{
			firstPosX = firstObj.transform.position.x;
			firstPosY = firstObj.transform.position.y;
		}

		if (secondObj.GetComponent<CaseData>() != null)
		{
			secondPosX = secondObj.transform.position.x;
			secondPosY = secondObj.transform.position.y;
		}

		if ((secondPosX < firstPosX + xCaseOffset && secondPosX > firstPosX - xCaseOffset) && (firstPosY < secondPosY + yCaseOffset && firstPosY > secondPosY - yCaseOffset))
		{
			return true;
		} else
		{
			return false;
		}
	}

	public List<CaseData> GetCaseByAoEFromCase(CaseData caseCible, int Range, AoEType aoeType, Direction direction = Direction.None)
	{
		foreach (CaseData obj in CaseManager.Instance.GetAllCaseWithStatut(Statut.atAoE))
		{
			obj.ChangeStatut(Statut.None, Statut.atAoE);
		}
		List<CaseData> caseList = new List<CaseData>();
		switch (aoeType)
		{
			case AoEType.Circle:
				caseList.Add(caseCible);
				for (int i = 0; i < Range; i++)
				{
					int listCount = caseList.Count;
					for (int y = 0; y < listCount; y++)
					{
						if (direction == Direction.SudEst)
						{
							if (caseList[y].GetCaseRelativeCoordinate(1, 0) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(1, 0)))
							{
								caseList.Add(caseList[y].GetCaseRelativeCoordinate(1, 0));
							}
						}

						if (direction == Direction.NordOuest)
						{
							if (caseList[y].GetCaseRelativeCoordinate(-1, 0) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(-1, 0)))
							{
								caseList.Add(caseList[y].GetCaseRelativeCoordinate(-1, 0));
							}
						}

						if (direction == Direction.NordEst)
						{
							if (caseList[y].GetCaseRelativeCoordinate(0, 1) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(0, 1)))
							{
								caseList.Add(caseList[y].GetCaseRelativeCoordinate(0, 1));
							}
						}

						if (direction == Direction.SudOuest)
						{
							if (caseList[y].GetCaseRelativeCoordinate(0, -1) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(0, -1)))
							{
								caseList.Add(caseList[y].GetCaseRelativeCoordinate(0, -1));
							}
						}
					}
				}
				return caseList;

			case AoEType.Croix:
				caseList.Add(caseCible);
				for (int i = 0; i < Range; i++)
				{
					if (caseCible.GetCaseRelativeCoordinate(i, 0) != null && caseCible.GetCaseRelativeCoordinate(i, 0))
						caseList.Add(caseCible.GetCaseRelativeCoordinate(i, 0));

					if (caseCible.GetCaseRelativeCoordinate(-i, 0) != null && caseCible.GetCaseRelativeCoordinate(-i, 0))
						caseList.Add(caseCible.GetCaseRelativeCoordinate(-i, 0));

					if (caseCible.GetCaseRelativeCoordinate(0, i) != null && caseCible.GetCaseRelativeCoordinate(0, i))
						caseList.Add(caseCible.GetCaseRelativeCoordinate(0, i));

					if (caseCible.GetCaseRelativeCoordinate(0, -i) != null && caseCible.GetCaseRelativeCoordinate(0, -i))
						caseList.Add(caseCible.GetCaseRelativeCoordinate(0, -i));

				}
				return caseList;

			case AoEType.Carre:
				caseList.Add(caseCible);
				for (int i = 0; i < Range; i++)
				{
					int listCount = caseList.Count;
					for (int y = 0; y < listCount; y++)
					{
						if (caseList[y].GetCaseRelativeCoordinate(1, 0) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(1, 0)))
							caseList.Add(caseList[y].GetCaseRelativeCoordinate(1, 0));

						if (caseList[y].GetCaseRelativeCoordinate(-1, 0) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(-1, 0)))
							caseList.Add(caseList[y].GetCaseRelativeCoordinate(-1, 0));

						if (caseList[y].GetCaseRelativeCoordinate(0, 1) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(0, 1)))
							caseList.Add(caseList[y].GetCaseRelativeCoordinate(0, 1));

						if (caseList[y].GetCaseRelativeCoordinate(0, -1) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(0, -1)))
							caseList.Add(caseList[y].GetCaseRelativeCoordinate(0, -1));

						if (caseList[y].GetCaseRelativeCoordinate(1, 1) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(1, 1)))
							caseList.Add(caseList[y].GetCaseRelativeCoordinate(1, 1));

						if (caseList[y].GetCaseRelativeCoordinate(-1, -1) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(-1, -1)))
							caseList.Add(caseList[y].GetCaseRelativeCoordinate(-1, -1));

						if (caseList[y].GetCaseRelativeCoordinate(1, -1) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(1, -1)))
							caseList.Add(caseList[y].GetCaseRelativeCoordinate(1, -1));

						if (caseList[y].GetCaseRelativeCoordinate(-1, 1) != null && !caseList.Contains(caseList[y].GetCaseRelativeCoordinate(-1, 1)))
							caseList.Add(caseList[y].GetCaseRelativeCoordinate(-1, 1));
					}
				}
				return caseList;
		}

		return null;

	}
  

}
