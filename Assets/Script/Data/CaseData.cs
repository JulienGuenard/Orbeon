using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.Remoting.Messaging;

/// <summary>Tout ce qu'il est possible de faire avec une case, ainsi que tout ce qui rentre dedans.</summary>
public class CaseData : NetworkBehaviour
{
	// *************** //
	// ** Variables ** // Toutes les variables sans distinctions
	// *************** //

	[SerializeField]
	[EnumFlagAttribute]
	[Space(200)]
	public Statut statut;
	[Space(200)]
	public SummonData summonData;
	public PersoData personnageData;
	public BallonData ballon;
	public PathfindingCase casePathfinding;
	public int xCoord;
	public int yCoord;

	public SpriteRenderer spriteR;
	Statut defaultStatut;
	bool ShineColorIsRunning = false;

	// ******************** //
	// ** Initialisation ** // Fonctions de départ, non réutilisable
	// ******************** //

	public override void OnStartClient()
	{
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
		spriteR = GetComponent<SpriteRenderer>();
		casePathfinding = PathfindingCase.Walkable;
		defaultStatut = statut;

		TurnManager.Instance.changeTurnEvent += OnChangeTurn;
	}

	void OnDisable()
	{
		if (LoadingManager.Instance != null && LoadingManager.Instance.isGameReady())
			TurnManager.Instance.changeTurnEvent -= OnChangeTurn;
	}

	// *************** //
	// ** Events **    // Appel de fonctions au sein de ce script grâce à des events
	// *************** //

	void OnChangeTurn(object sender, PlayerArgs e)
	{

		switch (e.currentPhase)
		{
			case Phase.Placement:
				ChangeStatut();
				break;
			case Phase.Deplacement:
				ChangeStatut(Statut.None, Statut.placementRed);
				ChangeStatut(Statut.None, Statut.placementBlue);
				break;
		}
	}

	// ************* //
	// ** Trigger ** // Appel de fonctions ou d'instructions grâce à la détection de collider triggers
	// ************* //

        IEnumerator DisableForATime(CaseData obj)
    {
        obj.GetComponent<PolygonCollider2D>().enabled = false;
        obj.GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        obj.GetComponent<PolygonCollider2D>().enabled = true;
        obj.GetComponent<SpriteRenderer>().enabled = true;
    }

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.tag == "Personnage")
		{
            if (col.gameObject.GetComponent<PersoData>().persoCase != null && col.gameObject.GetComponent<PersoData>().persoCase != this.GetComponent<CaseData>())
            {
                CaseData lastCase = col.gameObject.GetComponent<PersoData>().persoCase;
                lastCase.personnageData = null;
                lastCase.casePathfinding = PathfindingCase.Walkable;
                lastCase.ChangeStatut(Statut.None, Statut.isSelected);
                lastCase.ChangeStatut(Statut.None, Statut.isControllable);
                lastCase.StartCoroutine(DisableForATime(lastCase));
            }

            personnageData = col.gameObject.GetComponent<PersoData>();
			casePathfinding = PathfindingCase.NonWalkable;
			col.gameObject.GetComponent<PersoData>().persoCase = this;
            if (SelectionManager.Instance.selectedPersonnage == personnageData)
                SelectionManager.Instance.selectedCase = this;

            if (summonData != null)
			{
        summonData.ApplyEffect(col.gameObject);
			}

			foreach (SpriteRenderer rend in col.gameObject.GetComponentsInChildren<SpriteRenderer>())
			{
				rend.sortingOrder = spriteR.sortingOrder + 1;
			}
		}

		if (col.tag == "Ballon")
		{
            if (col.gameObject.GetComponent<BallonData>().ballonCase != null && col.gameObject.GetComponent<BallonData>().ballonCase != this.GetComponent<CaseData>())
            {
                CaseData lastCase = col.gameObject.GetComponent<BallonData>().ballonCase;
                lastCase.ballon = null;
                lastCase.casePathfinding = PathfindingCase.Walkable;
                lastCase.ChangeStatut(Statut.None, Statut.canShot);
                lastCase.StartCoroutine(DisableForATime(lastCase));
            }

            ballon = col.gameObject.GetComponent<BallonData>();
			casePathfinding = PathfindingCase.NonWalkable;
			col.gameObject.GetComponent<BallonData>().ballonCase = this;
			col.gameObject.GetComponent<BallonData>().xCoord = xCoord;
			col.gameObject.GetComponent<BallonData>().yCoord = yCoord;
			if (summonData != null)
			{
        summonData.ApplyEffect(col.gameObject);
			}
			if (CheckStatut(Statut.goalRed))
				StartCoroutine(UIManager.Instance.ScoreChange(Player.Blue));

			if (CheckStatut(Statut.goalBlue))
				StartCoroutine(UIManager.Instance.ScoreChange(Player.Red));

			foreach (SpriteRenderer rend in col.gameObject.GetComponentsInChildren<SpriteRenderer>())
			{
				rend.sortingOrder = spriteR.sortingOrder + 1;
			}
		}

		if (col.tag == "Summon")
		{
			if (summonData != null && summonData != col.gameObject.GetComponent<SummonData>())
				Destroy(summonData.gameObject);
			summonData = col.gameObject.GetComponent<SummonData>();
			if (col.gameObject.GetComponent<SummonData>().caseActual != this)
			{
				if (!col.gameObject.GetComponent<SummonData>().isTraversable)
				{
					casePathfinding = PathfindingCase.NonWalkable;
				}

				if (personnageData != null)
				{
					summonData.ApplyEffect(personnageData.gameObject);
				}
				if (ballon != null)
				{
					summonData.ApplyEffect(ballon.gameObject);
				}

				col.gameObject.GetComponent<SummonData>().caseActual = this;
			}
			foreach (SpriteRenderer rend in col.gameObject.GetComponentsInChildren<SpriteRenderer>())
			{
				rend.sortingOrder = spriteR.sortingOrder + 1;
			}
		}
		TransparencyManager.Instance.CheckCaseTransparency(this);

	}

	void OnTriggerExit2D(Collider2D col)
	{
		if (col.tag == "Personnage"
		    && col.gameObject.GetComponent<BoxCollider2D>().enabled == true
		    && GetComponent<PolygonCollider2D>().enabled == true)
		{
			/*personnageData = null;
			casePathfinding = PathfindingCase.Walkable;
			ChangeStatut(Statut.None, Statut.isSelected);
			ChangeStatut(Statut.None, Statut.isControllable);*/
		}

		if (col.tag == "Ballon"
		    && col.gameObject.GetComponent<BoxCollider2D>().enabled == true
		    && GetComponent<PolygonCollider2D>().enabled == true)
		{
		/*	ballon = null;
			casePathfinding = PathfindingCase.Walkable;
			ChangeStatut(Statut.None, Statut.canShot);*/
		}

		if (col.tag == "Summon"
		    && GetComponent<PolygonCollider2D>().enabled == true)
		{
			if (!summonData.isTraversable)
				casePathfinding = PathfindingCase.Walkable;
			summonData = null;
		}
		TransparencyManager.Instance.CheckCaseTransparency(this);

	}

	// *************** //
	// ** Fonctions ** // Fonctions réutilisables ailleurs
	// *************** //

	/// <summary>Change le statut, et met à jour la couleur et le feedback lié au statut</summary>
	public void ChangeStatut(Statut newStatut = Statut.None, Statut oldStatut = Statut.None)
	{
		Statut lastStatut = statut;

		if ((newStatut != Statut.None) && !((newStatut & statut) == newStatut))
			statut += (int)newStatut;

		if ((oldStatut != Statut.None) && ((oldStatut & statut) == oldStatut))
			statut -= (int)oldStatut;

		ChangeColorByStatut();
		ChangeFeedbackByStatut(statut, oldStatut);
	}

	/// <summary>Change la couleur de la case selon son statut.</summary>
	public void ChangeColorByStatut()
	{
		if (ColorManager.Instance == null)
			return;

		spriteR.color = ColorManager.Instance.caseColor;

		if ((Statut.goalRed & statut) == Statut.goalRed)
			spriteR.color = ColorManager.Instance.goalColor;

		if ((Statut.goalBlue & statut) == Statut.goalBlue)
			spriteR.color = ColorManager.Instance.goalColor;

		if (((Statut.placementRed & statut) == Statut.placementRed) && GameManager.Instance.currentPlayer == Player.Red)
			spriteR.color = ColorManager.Instance.placementZoneRed;

		if (((Statut.placementBlue & statut) == Statut.placementBlue) && GameManager.Instance.currentPlayer == Player.Blue)
			spriteR.color = ColorManager.Instance.placementZoneBlue;

		if ((Statut.isSelected & statut) == Statut.isSelected)
			spriteR.color = ColorManager.Instance.selectedColor;

		if ((Statut.canReplace & statut) == Statut.canReplace)
			spriteR.color = ColorManager.Instance.actionPreColor;

		if ((Statut.canPunch & statut) == Statut.canPunch)
			spriteR.color = ColorManager.Instance.actionPreColor;

		if ((Statut.atRange & statut) == Statut.atRange)
			spriteR.color = ColorManager.Instance.atRange;

		if ((Statut.atAoE & statut) == Statut.atAoE)
			spriteR.color = ColorManager.Instance.atAoE;

		if ((Statut.atPush & statut) == Statut.atPush)
			spriteR.color = ColorManager.Instance.atPush;

		if ((Statut.canMovePrevisu & statut) == Statut.canMovePrevisu)
			spriteR.color = ColorManager.Instance.canMovePrevisu;

		if ((Statut.canShot & statut) == Statut.canShot)
			spriteR.color = ColorManager.Instance.actionPreColor;
		if ((Statut.isControllable & statut) == Statut.isControllable)
			spriteR.color = ColorManager.Instance.actionPreColor;



		if ((Statut.isHovered & statut) == Statut.isHovered)
		{
			spriteR.color = ColorManager.Instance.hoverColor;
			if ((Statut.canReplace & statut) == Statut.canReplace)
				spriteR.color = ColorManager.Instance.actionColor;
			if ((Statut.canPunch & statut) == Statut.canPunch)
				spriteR.color = ColorManager.Instance.actionColor;
			if ((Statut.canShot & statut) == Statut.canShot)
				spriteR.color = ColorManager.Instance.actionColor;
			if ((Statut.isControllable & statut) == Statut.isControllable)
				spriteR.color = ColorManager.Instance.actionColor;
		}

		if ((Statut.canMove & statut) == Statut.canMove)
			spriteR.color = ColorManager.Instance.moveColor;

		if ((Statut.canBeTackled & statut) == Statut.canBeTackled)
			spriteR.color = ColorManager.Instance.enemyColor;

		if ((Statut.shotPrevisu & statut) == Statut.shotPrevisu)
			spriteR.color = ColorManager.Instance.canTarget;
	}

	/// <summary>Change le feedback visuel de la case selon son statut.</summary>
	private void ChangeFeedbackByStatut(Statut statut, Statut oldStatut)
	{
		if ((Statut.canBeTackled & statut) == Statut.canBeTackled)
			BeforeFeedbackManager.Instance.PredictInit(50, gameObject);
		if (oldStatut == (Statut.canBeTackled))
			BeforeFeedbackManager.Instance.PredictEnd(gameObject);
	}

	/// <summary>Change la couleur de la case.</summary>
	public void ChangeColor(Color color)
	{
		spriteR.color = color;
	}

	/// <summary>Change le sprite.</summary>
	public void ChangeSprite(Sprite sprite)
	{
		spriteR.sprite = sprite;
	}

	/// <summary>La couleur du sprite oscille entre deux couleurs.</summary>
	public IEnumerator StartShineColor(Color color1, Color color2, float time)
	{
		if (spriteR.color == color1 && spriteR.color == color2)
			StopCoroutine(StartShineColor(color1, color2, time));

		if (ShineColorIsRunning)
			StopCoroutine(StartShineColor(color1, color2, time));

		ShineColorIsRunning = true;

		while (ShineColorIsRunning)
		{
			Color colorx = color1;
			color1 = color2;
			color2 = colorx;
			for (int i = 0; i < 100; i++)
			{
				if (!ShineColorIsRunning)
					break;

				spriteR.color += (color1 - color2) / 100;
				yield return new WaitForSeconds(time + 0.01f);
			}

		}
	}

	/// <summary>Stop la fonction StartShineColor</summary>
	public void StopShineColor()
	{
		ShineColorIsRunning = false;
	}

	/// <summary>Check si la case a ce statut.</summary>
	public bool CheckStatut(Statut statutChecked)
	{
		if ((statutChecked & statut) == statutChecked)
			return true;
		else
			return false;
	}

	/// <summary>Change le passage de la case en "Walkable" ou "Non walkable".</summary>
	public void ChangePathfinding(PathfindingCase pathfinding)
	{
		casePathfinding = pathfinding;
	}

	/// <summary>Récupère la case en haut de cette case.</summary>
	public CaseData GetTopCase()
	{
		if (GameObject.Find(xCoord - 1 + " " + (yCoord + 1)) == null)
			return null;

		GameObject newCase = (GameObject.Find(xCoord - 1 + " " + (yCoord + 1)) != null) ? GameObject.Find(xCoord - 1 + " " + (yCoord + 1)) : null;
		if (newCase == null)
			return null;

		return newCase.GetComponent<CaseData>();
	}

	/// <summary>Récupère la case en bas de cette case.</summary>
	public CaseData GetBottomCase()
	{
		if (GameObject.Find(xCoord + 1 + " " + (yCoord - 1)) == null)
			return null;

		GameObject newCase = (GameObject.Find(xCoord + 1 + " " + (yCoord - 1)) != null) ? GameObject.Find(xCoord + 1 + " " + (yCoord - 1)) : null;
		if (newCase == null)
			return null;

		return newCase.GetComponent<CaseData>();
	}

	/// <summary>Récupère la case à gauche de cette case.</summary>
	public CaseData GetLeftCase()
	{
		GameObject newCase = (GameObject.Find(xCoord - 1 + " " + (yCoord - 1)) != null) ? GameObject.Find(xCoord - 1 + " " + (yCoord - 1)) : null;
		if (newCase == null)
			return null;

		return newCase.GetComponent<CaseData>();
	}

	/// <summary>Récupère la case à droite de cette case.</summary>
	public CaseData GetRightCase()
	{
		GameObject newCase = (GameObject.Find(xCoord + 1 + " " + (yCoord + 1)) != null) ? GameObject.Find(xCoord + 1 + " " + (yCoord + 1)) : null;
		if (newCase == null)
			return null;

		return newCase.GetComponent<CaseData>();
	}

	/// <summary>Récupère la case en haut à gauche de cette case.</summary>
	public CaseData GetTopLeftCase()
	{
		GameObject newCase = (GameObject.Find(xCoord - 1 + " " + yCoord) != null) ? GameObject.Find(xCoord - 1 + " " + yCoord) : null;
		if (newCase == null)
			return null;

		return newCase.GetComponent<CaseData>();
	}

	/// <summary>Récupère la case en bas à droite de cette case.</summary>
	public CaseData GetBottomRightCase()
	{
		GameObject newCase = (GameObject.Find(xCoord + 1 + " " + yCoord) != null) ? GameObject.Find(xCoord + 1 + " " + yCoord) : null;
		if (newCase == null)
			return null;

		return newCase.GetComponent<CaseData>();
	}

	/// <summary>Récupère la case en haut à droite de cette case.</summary>
	public CaseData GetTopRightCase()
	{
		GameObject newCase = (GameObject.Find(xCoord + " " + (yCoord + 1)) != null) ? GameObject.Find(xCoord + " " + (yCoord + 1)) : null;
		if (newCase == null)
			return null;

		return newCase.GetComponent<CaseData>();
	}

	/// <summary>Récupère la case en bas à gauche de cette case.</summary>
	public CaseData GetBottomLeftCase()
	{
		GameObject newCase = (GameObject.Find(xCoord + " " + (yCoord - 1)) != null) ? GameObject.Find(xCoord + " " + (yCoord - 1)) : null;
		if (newCase == null)
			return null;

		return newCase.GetComponent<CaseData>();
	}

	/// <summary>Récupère la case par rapport à X et Y coordonnées de cette case./n
	/// x + 1 = GetBottomRightCase/n
	/// x - 1 = GetTopLeftCase/n
	/// y + 1 = GetTopRightCase/n
	/// y - 1 = GetBottomLeftCase 
	public CaseData GetCaseRelativeCoordinate(int x, int y)
	{
		GameObject newCase = (GameObject.Find((xCoord + x) + " " + (yCoord + y)) != null) ? GameObject.Find((xCoord + x) + " " + (yCoord + y)) : null;
		if (newCase == null)
			return null;

		return newCase.GetComponent<CaseData>();
	}

	/// <summary>Desactive all statuts for this case.</summary>
	public void ClearAllStatut()
	{
		statut = 0;
		ChangeColorByStatut();
	}

	/// <summary>Desactive all statuts for this case except what was already checked in inspector.</summary>
	public void ClearStatutToDefault()
	{
		statut = defaultStatut;
		ChangeColorByStatut();
	}

	/// <summary>Get la case en face du personnage ou du ballon, on peut choisir celle à droite aussi</summary>
	public CaseData GetCaseInFront(Direction direction)
	{
		switch (direction)
		{
			case Direction.NordEst:
				return GetCaseRelativeCoordinate(0, 1);

			case Direction.NordOuest:
				return GetCaseRelativeCoordinate(-1, 0);

			case Direction.SudEst:
				return GetCaseRelativeCoordinate(1, 0);

			case Direction.SudOuest:
				return GetCaseRelativeCoordinate(0, -1);
		}
		return null;
	}

	/// <summary>Get la case en face du personnage ou du ballon, on peut choisir celle à droite aussi</summary>
	public CaseData GetCaseAtRight(Direction direction)
	{
		switch (direction)
		{
			case Direction.NordEst:
				return GetCaseRelativeCoordinate(-1, 0);

			case Direction.NordOuest:
				return GetCaseRelativeCoordinate(0, -1);

			case Direction.SudEst:
				return GetCaseRelativeCoordinate(0, 1);

			case Direction.SudOuest:
				return GetCaseRelativeCoordinate(1, 0);
		}
		return null;
	}

	/// <summary>Get la case en face du personnage ou du ballon, on peut choisir celle à droite aussi</summary>
	public CaseData GetCaseAtLeft(Direction direction)
	{
		switch (direction)
		{
			case Direction.NordEst:
				return GetCaseRelativeCoordinate(1, 0);

			case Direction.NordOuest:
				return GetCaseRelativeCoordinate(0, 1);

			case Direction.SudEst:
				return GetCaseRelativeCoordinate(0, -1);

			case Direction.SudOuest:
				return GetCaseRelativeCoordinate(-1, 0);
		}
		return null;
	}

	/// <summary>Get la case en face du personnage ou du ballon, on peut choisir celle à droite aussi</summary>
	public CaseData GetCaseAtBack(Direction direction)
	{
		switch (direction)
		{
			case Direction.NordEst:
				return GetCaseRelativeCoordinate(0, -1);

			case Direction.NordOuest:
				return GetCaseRelativeCoordinate(1, 0);

			case Direction.SudEst:
				return GetCaseRelativeCoordinate(-1, 0);

			case Direction.SudOuest:
				return GetCaseRelativeCoordinate(0, 1);
		}
		return null;
	}

	public Direction GetDirectionBetween(CaseData comparedCase)
	{
		Vector3 targetCasePos = comparedCase.transform.position;
		Vector3 originCasePos = transform.position;

		if (originCasePos.x > targetCasePos.x && originCasePos.y > targetCasePos.y)
			return Direction.SudOuest;

		if (originCasePos.x > targetCasePos.x && originCasePos.y < targetCasePos.y)
			return Direction.NordOuest;

		if (originCasePos.x < targetCasePos.x && originCasePos.y > targetCasePos.y)
			return Direction.SudEst;

		if (originCasePos.x < targetCasePos.x && originCasePos.y < targetCasePos.y)
			return Direction.NordEst;

		return Direction.None;
	}
}
