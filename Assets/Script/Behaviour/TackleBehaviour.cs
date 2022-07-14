using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TackleBehaviour : NetworkBehaviour
{

	// *************** //
	// ** Variables ** //
	// *************** //

	[Header("Temps")]
	[Tooltip("Durée du va et vien entre le tacleur et le taclé")]
	public float tackleAnimTime;

	[HideInInspector] public static TackleBehaviour Instance;

	public SyncListInt randomIntList;
	int randomIntOrder = 0;

	public List<int> ballonTackleLourd = new List<int>();
	public List<int> ballonTackleLeger = new List<int>();


	Transform playerCase;
	Player currentPlayer;
	int randomInt;
	int maxInt = 0;

	// *************** //
	// ** Initialisation ** //
	// *************** //

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
		SetupRandomList();
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
			TurnManager.Instance.changeTurnEvent -= OnChangeTurn;
		}
	}

	// *************** //
	// ** Init les listes randoms ** //
	// *************** //

	public void SetupRandomList()
	{
		randomIntList.Clear();
		randomIntOrder = 0;
		if (!isServer)
			return;

		for (int i = 0; i < 1000; i++)
		{
			randomIntList.Add(UnityEngine.Random.Range(0, 100));
		}
	}

	void OnChangeTurn(object sender, PlayerArgs e)
	{ // Lorsqu'un joueur termine son tour
		SetupRandomList();
	}

	// *************** //
	// ** Checkers ** //
	// *************** //

	public void CheckTackle(GameObject movingObj, PersoData shotingPersonnage = null)
	{ // Vérifie si le personnage peut être taclé, et si c'est le cas, fait un test de chance pour savoir s'il est taclé
		playerCase = SelectionManager.Instance.selectedCase.transform;
		currentPlayer = TurnManager.Instance.currentPlayer;
		randomInt = randomIntList[randomIntOrder];

		foreach (PersoData perso in RosterManager.Instance.listHeroPlaced)
		{
			playerCase = SelectionManager.Instance.selectedCase.transform;
			currentPlayer = TurnManager.Instance.currentPlayer;

			if (movingObj != null && perso != shotingPersonnage)
			{
				if (perso != movingObj && perso.timeStunned == 0 && CaseManager.Instance.CheckAdjacent(perso.gameObject, movingObj.gameObject) == true)
				{
					randomIntOrder++;
					switch (movingObj.name)
					{
						case ("Ballon"):

							TackleBall(perso, movingObj);
							break;
						default:
							TacklePlayer(perso, movingObj);
							break;
					}
				}
			}
		}
	}
	// *************** //
	// ** Effet Tacle** //
	// *************** //

	void TackleBall(PersoData perso, GameObject movingObj)
	{

		if (perso.owner != currentPlayer)
		{
			if (SelectionManager.Instance.selectedPersonnage.weightType == WeightType.Heavy)
			{
				maxInt = ballonTackleLourd[movingObj.GetComponent<BallonData>().casesCrossed - 1];
			}
			if (SelectionManager.Instance.selectedPersonnage.weightType == WeightType.Light)
			{
				maxInt = ballonTackleLeger[movingObj.GetComponent<BallonData>().casesCrossed - 1];
			}

			playerCase = movingObj.GetComponent<BallonData>().ballonCase.transform;
			StartCoroutine(TackleEffect(perso, movingObj.transform));

			if (randomInt < maxInt)
			{
				AfterFeedbackManager.Instance.TackleText(randomInt, maxInt, playerCase.gameObject);
				GameManager.Instance.actualAction = PersoAction.isWaiting;
				movingObj.GetComponent<BallonData>().ChangeStatut(BallonStatut.isIntercepted);
			}
		}
	}

	void TacklePlayer(PersoData perso, GameObject movingObj)
	{
		if (perso.owner != currentPlayer)
		{
			StartCoroutine(TackleEffect(perso, playerCase));
			if (movingObj.GetComponent<PersoData>().weightType == perso.weightType)
			{
				maxInt = 50;
				if (randomInt < maxInt)
				{
					AfterFeedbackManager.Instance.TackleText(randomInt, maxInt, playerCase.gameObject);
					SelectionManager.Instance.selectedPersonnage.actualPointMovement = Mathf.CeilToInt(SelectionManager.Instance.selectedPersonnage.actualPointMovement / 2);
					SelectionManager.Instance.selectedPersonnage.isTackled = true;
					TurnManager.Instance.EnableFinishTurn();
				} else
				{
					AfterFeedbackManager.Instance.TackleText(randomInt, maxInt, playerCase.gameObject);
				}
			} else
			{
				maxInt = 25;
				if (randomInt < maxInt)
				{
					AfterFeedbackManager.Instance.TackleText(randomInt, maxInt, playerCase.gameObject);
					SelectionManager.Instance.selectedPersonnage.actualPointMovement = Mathf.CeilToInt(SelectionManager.Instance.selectedPersonnage.actualPointMovement / 4);
					SelectionManager.Instance.selectedPersonnage.isTackled = true;
					TurnManager.Instance.EnableFinishTurn();
				} else
				{
					AfterFeedbackManager.Instance.TackleText(randomInt, maxInt, playerCase.gameObject);
				}
			}
		}
	}

	// *************** //
	// ** Visuel Tacle ** //
	// *************** //

	IEnumerator TackleEffect(PersoData punchingPersonnage, Transform playerCase)
	{ // Effet visuel à chaque fois que le personnage se déplaçant se fait taclé
		if (punchingPersonnage.GetComponent<BoxCollider2D>().enabled != false)
		{

			punchingPersonnage.GetComponent<BoxCollider2D>().enabled = false;

			Vector3 startPos = punchingPersonnage.transform.position;

			float fracturedTime = 0;
			float timeUnit = tackleAnimTime / 60;

			while (fracturedTime < 2)
			{
				fracturedTime += timeUnit + 0.01f;
				punchingPersonnage.transform.position = Vector3.Lerp(startPos, playerCase.position - punchingPersonnage.originPoint.transform.localPosition, fracturedTime);
				yield return new WaitForEndOfFrame();
			}

			fracturedTime = 0;

			while (fracturedTime < 2)
			{
				fracturedTime += timeUnit + 0.01f;
				punchingPersonnage.transform.position = Vector3.Lerp(playerCase.position - punchingPersonnage.originPoint.transform.localPosition, startPos, fracturedTime);
				yield return new WaitForEndOfFrame();
			}
			punchingPersonnage.GetComponent<BoxCollider2D>().enabled = true;
		}
	}
}