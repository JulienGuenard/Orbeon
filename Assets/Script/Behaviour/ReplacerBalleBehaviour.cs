using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ReplacerBalleBehaviour : NetworkBehaviour
{

  // *************** //
  // ** Variables ** //
  // *************** //

  /// <summary>  Case où tu peux replacer ta balle </summary>
  public List<CaseData> caseAction;

  public static ReplacerBalleBehaviour Instance;

  // *************** //
  // ** Inittialisation ** //
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
    EventManager.newClickEvent += OnNewClick;
    EventManager.newHoverEvent += OnNewHover;
  }

  void OnDisable()
  {
    EventManager.newClickEvent -= OnNewClick;
    EventManager.newHoverEvent -= OnNewHover;
  }

  public void OnNewClick()
  { // Lors d'un click sur une case
    if (GameManager.Instance.actualAction == PersoAction.isReplacingBall && caseAction.Count != 0)
    {
      TirReplacerBalle();
    }
    else
      StartCoroutine(ReplacerBalleEnd());

  }
  void OnNewHover(object sender, HoverArgs e)
  { // Lors d'un click sur une case
    if (GameManager.Instance.actualAction == PersoAction.isReplacingBall && caseAction.Count != 0 && (HoverManager.Instance.hoveredCase.CheckStatut(Statut.canReplace)))
    {
      BeforeFeedbackManager.Instance.PredictDeplacement(SelectionManager.Instance.selectedBallon.gameObject, HoverManager.Instance.hoveredCase);
    }

  }

  public void TirReplacerBalle()
  {
    PersoData selectedPersonnage = SelectionManager.Instance.selectedPersonnage;
    CaseData hoveredCase = HoverManager.Instance.hoveredCase;
    BallonData selectedBallon = SelectionManager.Instance.selectedBallon;

    if (caseAction.Contains(hoveredCase))
      {
        selectedBallon.transform.position = hoveredCase.transform.position;
        hoveredCase.GetComponent<CaseData>().ChangeStatut(Statut.canReplace);
        StartCoroutine(ReplacerBalleEnd());
        return;
      }
    StartCoroutine(ReplacerBalleEnd());
    selectedPersonnage.GetComponent<PersoData>().actualPointMovement++;
  }

  public void ReplacerBalle()
  { // Action de replacer la balle après avoir cliquer sur le bouton approprié sur le menu contextuel
    CaseData selectedCase = SelectionManager.Instance.selectedCase;

    GameManager.Instance.actualAction = PersoAction.isReplacingBall;
    caseAction.Clear();

    int xCoord = selectedCase.GetComponent<CaseData>().xCoord;
    int yCoord = selectedCase.GetComponent<CaseData>().yCoord;

    ReplacerBalleAdd((xCoord + 1), (yCoord + 0));
    ReplacerBalleAdd((xCoord - 1), (yCoord + 0));
    ReplacerBalleAdd((xCoord + 0), (yCoord + 1));
    ReplacerBalleAdd((xCoord + 0), (yCoord - 1));
  }

  IEnumerator ReplacerBalleEnd()
  { // Action de replacer la balle qui se termine après avoir placer la balle 
    Color caseColor = ColorManager.Instance.caseColor;

    yield return new WaitForEndOfFrame();
    GameManager.Instance.actualAction = PersoAction.isSelected;

    BeforeFeedbackManager.Instance.HidePrediction();
    foreach (CaseData obj in caseAction)
      {
        //     obj.GetComponent<CaseData>().colorLock = false;
        obj.ChangeStatut(Statut.None, Statut.canReplace);
      }
    TurnManager.Instance.StartCoroutine("EnableFinishTurn");
    caseAction.Clear();
  }

  void ReplacerBalleAdd(int xCoord, int yCoord)
  {
    Color actionPreColor = ColorManager.Instance.actionPreColor;

    if (GameObject.Find(xCoord.ToString() + " " + yCoord.ToString()) != null
        && GameObject.Find(xCoord.ToString() + " " + yCoord.ToString()).GetComponent<CaseData>().casePathfinding != PathfindingCase.NonWalkable)
      {
        CaseData newCase = (GameObject.Find(xCoord.ToString() + " " + yCoord.ToString())).GetComponent<CaseData>();
        caseAction.Add(newCase);
        newCase.GetComponent<CaseData>().ChangeStatut(Statut.canReplace);
      }
  }
  public void replaceEnd()
  {
    StartCoroutine(ReplacerBalleEnd());
  }
}
