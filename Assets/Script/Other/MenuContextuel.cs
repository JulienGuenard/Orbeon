using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MenuContextuel : NetworkBehaviour
{


	// *************** //
	// ** Variables ** //
	// *************** //

	[Tooltip("J'utilise parfois cette variable pour être sûr que le ballon soit bien centré où je veux sans bouger son transform")]
	[ReadOnly]
	public Vector3 offsetBallon;
	[Tooltip("S'il est coché, c'est qu'un tir de ballon est en cours")]
	[ReadOnly]
	public bool isShoting;
	[HideInInspector] public GameObject nextPosition;
	[HideInInspector] public bool canBounce;

	public static MenuContextuel Instance;
	GameObject ballon;

	public MenuContextuelButton buttonShot;
	public MenuContextuelButton buttonReplace;

	public bool activated;

	GameObject menuContextuelReplacerTooltip;
	GameObject menuContextuelTirerTooltip;

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
		ballon = GameObject.Find("Ballon");
		EventManager.newClickEvent += OnNewClick;
		UIManager.Instance.menuContextuel = this.gameObject;
		HideMenu();
	}

	void OnEnable()
	{
		if (LoadingManager.Instance != null && LoadingManager.Instance.isGameReady())
		{
			CaseManager.Instance.DisableAllColliders();
		}
	}


	void OnDisable()
	{
		EventManager.newClickEvent -= OnNewClick;
		if (LoadingManager.Instance != null && LoadingManager.Instance.isGameReady())
		{
			CaseManager.Instance.EnableAllColliders();
		}
	}

	public void OnNewClick()
	{ // Lors d'un click sur une case
		Phase currentPhase = TurnManager.Instance.currentPhase;
		BallonData hoveredBallon = HoverManager.Instance.hoveredBallon;
		PersoData selectedPersonnage = SelectionManager.Instance.selectedPersonnage;
		PersoAction actualAction = GameManager.Instance.actualAction;

		if (currentPhase == Phase.Deplacement
		    && selectedPersonnage != null
		    && hoveredBallon != null
		    && actualAction == PersoAction.isSelected)
		{
			if (CaseManager.Instance.CheckAdjacent(hoveredBallon.gameObject, selectedPersonnage.gameObject) == true)
			{
				buttonShotMenu();
			}
		}
	}

	public void buttonShotMenu()
	{
		GameObject menuContextuel = UIManager.Instance.menuContextuel;
		BallonData hoveredBallon = HoverManager.Instance.hoveredBallon;

		GameManager.Instance.actualAction = PersoAction.isShoting;
		CaseManager.Instance.DisableAllColliders();
		MoveBehaviour.Instance.movePathes.Clear();
		SelectionManager.Instance.selectedBallon = hoveredBallon;
		menuContextuel.transform.position = hoveredBallon.transform.position;
		foreach (MenuContextuelButton obj in menuContextuel.GetComponentsInChildren<MenuContextuelButton>())
		{
			obj.Enable();
		}

		TurnManager.Instance.DisableFinishTurn();
		activated = true;
	}

	public void HideMenu()
	{
		if (Instance != null)
		{
			CaseManager.Instance.EnableAllColliders();
			TurnManager.Instance.StartCoroutine("EnableFinishTurn");
			MenuContextuel.Instance.gameObject.transform.position = new Vector3(999, 999, 999);
			activated = false;
		}
	}

	private void Update()
	{
		if (!activated)
			return;
		if (Input.GetKeyDown("mouse 0"))
		{
			if (buttonShot.collision)
			{
				Clicked(buttonShot.transform.name);
			} else if (buttonReplace.collision)
			{
				Clicked(buttonReplace.transform.name);
			} else
			{
				Clicked("Nothing");
			}
			activated = false;
		}
	}

	private void Clicked(string name)
	{
		if (!SynchroManager.Instance.canPlayTurn())
		{
			return;
		}
		RpcFunctions.Instance.CmdMenuContextuelClick(name);
	}

}
