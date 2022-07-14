using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>Gère toutes les variables et fonctions lié au hover du curseur.</summary>
public class HoverManager : NetworkBehaviour
{

  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  [Header("  Hover Data")]
  [Tooltip("Case sous le curseur")]
  /// <summary>Change la couleur de la case lors d'un exit d'hover.</summary>
	[ReadOnly] public CaseData hoveredCase;
  [Tooltip("Personnage sous le curseur")]
  [ReadOnly] public PersoData hoveredPersonnage;
  [Tooltip("Ballon sous le curseur")]
  [ReadOnly] public BallonData hoveredBallon;
  [Tooltip("Case qui était précedemment sous le curseur")]
  [ReadOnly] public CaseData hoveredLastCase;

  [HideInInspector] public static HoverManager Instance;

  public EventHandler<HoverArgs> newHoverEvent;
  GameObject spriteHover;

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
    spriteHover = GameObject.Find("spriteHover");
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

  // *************** //
  // ** Events **    // Appel de fonctions au sein de ce script grâce à des events
  // *************** //

  void OnNewHover(object sender, HoverArgs e)
  { // Curseur se trouve sur une case ou quitte une case

    if (hoveredLastCase == null || hoveredLastCase != hoveredCase)
    {
      hoveredLastCase = hoveredCase;
    }

    if (hoveredCase != null)
    {
      changeColorExit(GameManager.Instance.currentPhase);
      changeSpriteExit();
    }

    if (e == null)
      return;

        hoveredCase = e.hoveredCase;
        hoveredPersonnage = e.hoveredPersonnage;
    hoveredBallon = e.hoveredBallon;

    if (hoveredCase != null)
    {
      changeColorEnter();
      changeSpriteEnter();
    }

    if (hoveredCase != null && hoveredLastCase != hoveredCase)
    {
      changeMovementPattern(GameManager.Instance.currentPhase);
    }
  }

  // *************** //
  // ** Fonctions ** // Fonctions réutilisables ailleurs
  // *************** //

  /// <summary>Change la couleur de la case lors d'un enter d'hover.</summary>
  private void changeColorEnter()
  { // Change la couleur de la case qui est sur le curseur

    Player currentPlayer = GameManager.Instance.currentPlayer;
    Phase currentPhase = GameManager.Instance.currentPhase;
    Color actionColor = ColorManager.Instance.actionColor;
    Color moveColor = ColorManager.Instance.moveColor;
    PersoData selectedPersonnage = SelectionManager.Instance.selectedPersonnage;
    Color hoverColor = ColorManager.Instance.hoverColor;
    PersoAction actualAction = GameManager.Instance.actualAction;

    hoveredCase.GetComponent<CaseData>().ChangeStatut(Statut.isHovered);

    if (hoveredCase.GetComponent<SpriteRenderer>().color == ColorManager.Instance.actionPreColor)
      hoveredCase.GetComponent<SpriteRenderer>().color = ColorManager.Instance.actionColor;
  }

  /// <summary>Change la couleur de la case lors d'un exit d'hover.</summary>
  public void changeColorExit(Phase currentPhase)
  { // Change la couleur de la case qui était sur le curseur
    switch (currentPhase)
    {
      case (Phase.Placement):
      hoveredCase.ChangeStatut(Statut.None, Statut.isHovered);
      break;
      case (Phase.Deplacement):
      hoveredCase.ChangeStatut(Statut.None, Statut.isHovered);
      break;
    }
  }

  private void changeMovementPattern(Phase currentPhase)
  {
    switch (currentPhase)
    {
      case (Phase.Placement):
      break;
      case (Phase.Deplacement):
      if (SelectionManager.Instance.selectedPersonnage != null 
        && GameManager.Instance.actualAction == PersoAction.isSelected
        && hoveredCase.casePathfinding == PathfindingCase.Walkable)
        {
          Pathfinding.Instance.StartPathfinding();
          MoveBehaviour.Instance.createPath();
      }
      else
      {
        CaseManager.Instance.RemovePath();
        BeforeFeedbackManager.Instance.HidePrediction();
      }
      break;
      //      

    }
  }

  private void changeSpriteEnter()
  { // Change la couleur de la case qui est sur le curseur
    if (GameManager.Instance.currentPhase == Phase.Deplacement)
      return;

    if (spriteHover == null)
      return;

    if (SelectionManager.Instance.selectedPersonnage == null)
      return;

    if ((GameManager.Instance.currentPlayer == Player.Red && hoveredCase.CheckStatut(Statut.placementRed))
        || (GameManager.Instance.currentPlayer == Player.Blue && hoveredCase.CheckStatut(Statut.placementBlue)))
    {
      spriteHover.transform.localScale = new Vector3(1.5f, 1.5f, 1);
      spriteHover.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
      spriteHover.GetComponent<SpriteRenderer>().sprite = SelectionManager.Instance.selectedPersonnage.GetComponentInChildren<SpriteRenderer>().sprite;
      spriteHover.transform.position = hoveredCase.transform.position - SelectionManager.Instance.selectedPersonnage.originPoint.transform.localPosition + SelectionManager.Instance.selectedPersonnage.GetComponentInChildren<SpriteRenderer>().transform.localPosition;
        }
  }

  public void changeSpriteExit()
  { // Change la couleur de la case qui est sur le curseur
    spriteHover.GetComponent<SpriteRenderer>().sprite = null;
    spriteHover.transform.position = transform.position = new Vector3(999, 999, 999);
  }

  public void UnHover()
  {
    changeColorExit(GameManager.Instance.currentPhase);
    changeSpriteExit();

    HoverArgs hoverArgs = null;
    OnNewHover(gameObject, hoverArgs);
  }
}