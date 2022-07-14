using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class MoveBehaviour : NetworkBehaviour
{
  // *************** //
  // ** Variables ** //
  // *************** //

  [Header("  Temps")]
  [Tooltip("La durée d'un déplacement entre deux cases.")]
  public float travelTime;

  [Header("  Chemin du pathfinding")]
  [ReadOnly]
  public List<GameObject> GoPathes;
  // déplacement
  public List<Transform> movePathes;

  // déplacement
  [ReadOnly] public List<Transform> pathesLast;

  [HideInInspector] public static MoveBehaviour Instance;

  CaseData hoveredCase;

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
    EventManager.newClickEvent += OnNewClick;
  }

  void OnDisable()
  {
    EventManager.newClickEvent -= OnNewClick;
  }

  void OnNewClick()
  { // Lors d'un click sur une case
    if (TurnManager.Instance.currentPhase == Phase.Deplacement)
      StartCoroutine(LateOnNewClick());
  }

  IEnumerator LateOnNewClick()
  {
    yield return new WaitForEndOfFrame();
    CaseData hoveredCase = HoverManager.Instance.hoveredCase;
    PersoAction actualAction = GameManager.Instance.actualAction;
    PathfindingCase casePathfinding = hoveredCase.GetComponent<CaseData>().casePathfinding;

    if (movePathes.Count != 0
        && actualAction == PersoAction.isSelected
        && movePathes.Contains(hoveredCase.transform))
    {
      SendDeplacement();
    }
  }

  // *************** //
  // ** Pathfinding ** //
  // *************** //

  public void createPath()
  { // Créé une route de déplacement pour un personnage

    if (GameManager.Instance.actualAction != PersoAction.isSelected)
      return;

    pathesLast = movePathes;
    CaseManager.Instance.RemovePath();

    this.movePathes.Clear();

    foreach (GameObject path in GoPathes)
    {
      if (path != SelectionManager.Instance.selectedCase.gameObject)
        this.movePathes.Add(path.transform);
    }

    // Si le personnage n'a pas assez de PM, alors la route n'est pas créé
    if (this.movePathes.Count > SelectionManager.Instance.selectedPersonnage.GetComponent<PersoData>().actualPointMovement)
    {
      this.movePathes.Clear();
      BeforeFeedbackManager.Instance.HidePrediction();
    }
    else
    {
      ShowPath();
    }
  }

  public void ShowPath()
  { // Montre la route de déplacement


    Color moveColor = ColorManager.Instance.moveColor;
    Player currentPlayer = GameManager.Instance.currentPlayer;
    Color enemyColor = ColorManager.Instance.enemyColor;
    foreach (Transform path in this.movePathes)
    {
      path.GetComponent<CaseData>().ChangeStatut(Statut.canMove);
      foreach (PersoData persoCompared in RosterManager.Instance.listHero)
      {
        if (persoCompared.owner != currentPlayer
            && persoCompared.persoCase != null
            && CaseManager.Instance.CheckAdjacent(path.gameObject, persoCompared.gameObject) == true
            && persoCompared.timeStunned == 0)
        {
          path.GetComponent<CaseData>().ChangeStatut(Statut.canBeTackled);
          //   FeedbackManager.Instance.PredictInit(50, path.gameObject);
        }
      }
    }
    BeforeFeedbackManager.Instance.PredictDeplacement(SelectionManager.Instance.selectedPersonnage.gameObject, this.movePathes[this.movePathes.Count - 1].GetComponent<CaseData>());
  }

  // *************** //
  // ** Déplacement ** //
  // *************** //

  public void SendDeplacement()
  { // On stock la route de déplacement et on la colore, puis on appelle la fonction de déplacement
    Color isMovingColor = ColorManager.Instance.isMovingColor;
    Color caseColor = ColorManager.Instance.caseColor;

    foreach (Transform path in movePathes)
    {
      path.GetComponent<CaseData>().ChangeStatut(Statut.isMoving);
    }
    StartCoroutine(Deplacement(SelectionManager.Instance.selectedPersonnage.originPoint.transform.localPosition, SelectionManager.Instance.selectedPersonnage, movePathes));
  }

  public IEnumerator Deplacement(Vector3 originPoint, PersoData selectedPersonnage, List<Transform> pathes)
  { // On déplace le personnage de case en case jusqu'au click du joueur propriétaire, et entre temps on check s'il est taclé ou non
    TurnManager.Instance.DisableFinishTurn();
        GameManager.Instance.CanPlayTurn(false);

        float delayBeforeMovingAgain = 0.6f;

    TackleBehaviour.Instance.CheckTackle(selectedPersonnage.gameObject);

    List<Transform> tempPath = pathes.GetRange(0, pathes.Count);

    PersoData persoSelected = SelectionManager.Instance.selectedPersonnage;
    foreach (Transform path in tempPath)
    {
            delayBeforeMovingAgain += 0.23f;
            if (persoSelected.moveInterrupted)
      {

                SelectionManager.Instance.Deselect();
                persoSelected.moveInterrupted = false;
            }
      if (SelectionManager.Instance.selectedPersonnage == null)
      {
        CaseData caseSelected = persoSelected.persoCase;

        caseSelected.GetComponent<CaseData>().ChangeStatut(Statut.None, Statut.isMoving);
        GameManager.Instance.actualAction = PersoAction.isSelected;
        CaseManager.Instance.RemovePath();
        caseSelected.GetComponent<CaseData>().casePathfinding = PathfindingCase.NonWalkable;
        tempPath.Clear();
        StartCoroutine(TurnManager.Instance.EnableFinishTurn());
                GameManager.Instance.CanPlayTurn(true);
                yield return new WaitForEndOfFrame();
        yield return null;
      }
      else
      {
        persoSelected = SelectionManager.Instance.selectedPersonnage;
      }
      if (selectedPersonnage.isTackled)
      {
        path.GetComponent<CaseData>().ChangeStatut(Statut.isTackled, Statut.isMoving);
      }
      else
      {
        Vector3 startPos = selectedPersonnage.transform.position;
        float fracturedTime = 0;
        float timeUnit = travelTime / 60;

                if (SelectionManager.Instance.selectedPersonnage == null)
                    yield break;

        selectedPersonnage.RotateTowards(path.gameObject);
        SelectionManager.Instance.selectedPersonnage.actualPointMovement -= 1;
        InfoPerso.Instance.stats.updatePm(SelectionManager.Instance.selectedPersonnage.actualPointMovement);

        while (selectedPersonnage.transform.position != path.transform.position - originPoint)
        {
          fracturedTime += timeUnit + 0.01f;
          selectedPersonnage.transform.position = Vector3.Lerp(startPos, path.transform.position - originPoint, fracturedTime);
          yield return new WaitForEndOfFrame();
        }
        SelectionManager.Instance.selectedCase = path.gameObject.GetComponent<CaseData>();
        path.GetComponent<CaseData>().ChangeStatut(Statut.None, Statut.isMoving);
        TackleBehaviour.Instance.CheckTackle(selectedPersonnage.gameObject);
      }
    }
    SelectionManager.Instance.selectedCase = tempPath[tempPath.Count - 1].gameObject.GetComponent<CaseData>();
    SelectionManager.Instance.selectedCase.GetComponent<CaseData>().ChangeStatut(Statut.None, Statut.isMoving);
    GameManager.Instance.actualAction = PersoAction.isSelected;
    CaseManager.Instance.RemovePath();
    if (!selectedPersonnage.isTackled)
    {
      SelectionManager.Instance.selectedCase.GetComponent<CaseData>().casePathfinding = PathfindingCase.NonWalkable;
      tempPath.Clear();
    }
    else
    {
      selectedPersonnage.isTackled = false;
      SelectionManager.Instance.selectedPersonnage.actualPointMovement = 0;
            HoverManager.Instance.hoveredCase = null;
      tempPath.Clear();
    }
        StartCoroutine(TurnManager.Instance.EnableFinishTurn());
        yield return new WaitForSeconds(delayBeforeMovingAgain);
        GameManager.Instance.CanPlayTurn(true);
       
    }
}