using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PushBehaviour : NetworkBehaviour
{

  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  public static PushBehaviour Instance;

  bool isPushing = false;

  public float travelTime;

  public List<Transform> pathList = new List<Transform>();
  Direction actualPushDirection;

  PersoData persoAfflicted = null;
  public CaseData caseFinalShow = null; // la case à montrer pour le pré-rendu
  GameObject objAfflicted;

  public int pushValue;
  public int retainedPushValue;

    public RuntimeAnimatorController fxPersonnageHurt;
    public RuntimeAnimatorController fxBallImpact1;
    public RuntimeAnimatorController fxBallImpact2;
    public RuntimeAnimatorController fxBallImpact3;

    /// <summary>
    /// Est-ce que la tornade fait des dégats alors que le perso ne bouge pas?
    /// </summary>
    bool stillTornadoDamage;

  // ******************** //
  // ** Initialisation ** // Fonctions de départ, non réutilisable
  // ******************** //

  public override void OnStartClient()
  {
    if (Instance == null)
      Instance = this;

   // MultiplePushStart();
  }

  // *************** //
  // ** Fonctions ** // Fonctions réutilisables ailleurs
  // *************** //

  /// <summary>Check et prépare l'effet de poussée.</summary>
  public void PushCheck(GameObject obj, int givenPushValue, CaseData caseAfflicted, PushType pushType, Direction pushDirection = Direction.Front)
  {
    Direction persoDirection = Direction.None;

    pushValue = givenPushValue;
    if (obj.GetComponent<BallonData>() != null)
    {
      if (obj.GetComponent<BallonData>().isMoving)
      {
        persoDirection = obj.GetComponent<BallonData>().ballonDirection;
        obj.GetComponent<BallonData>().StopMove();
        obj.GetComponent<BallonData>().StopAllCoroutines();
      }
    }

    objAfflicted = obj;
    if (objAfflicted.GetComponent<PersoData>() != null)
    {
            MoveBehaviour.Instance.StopAllCoroutines();
            foreach (Transform path in MoveBehaviour.Instance.movePathes)
      {
        path.GetComponent<CaseData>().ChangeStatut(Statut.None, Statut.isMoving);
      }
      objAfflicted.GetComponent<PersoData>().pushedDebt = 0;
      MoveBehaviour.Instance.movePathes.Clear();
      persoAfflicted = objAfflicted.GetComponent<PersoData>();
      persoDirection = persoAfflicted.persoDirection;
    }

    if (objAfflicted.GetComponent<SummonData>() != null)
    {
      persoDirection = objAfflicted.GetComponent<SummonData>().summonDirection;
    }

    CaseData tempCase = null;

    pathList.Clear();

    switch (pushType)
    {
      case PushType.FromCaster:
      actualPushDirection = SelectionManager.Instance.selectedPersonnage.persoDirection;
      break;
      case PushType.FromTerrain:
      actualPushDirection = pushDirection;
      break;
    }
    GetShownCase(obj, pushValue, caseAfflicted, actualPushDirection);

    foreach (CaseData caseObj in CaseManager.Instance.GetAllCaseWithStatut(Statut.atPush))
    {
      caseObj.GetComponent<CaseData>().ChangeStatut(Statut.None, Statut.atPush);
    }
  }

  public void PushStart()
  {
    MoveBehaviour.Instance.StopAllCoroutines();
    StopAllCoroutines();

    StartCoroutine(Deplacement(objAfflicted, pathList));
  }

  public void MultiplePushStart()
  {
    if (objAfflicted.GetComponent<PushData>())
      objAfflicted.GetComponent<PushData>().Push(actualPushDirection, pushValue);
  }

  public IEnumerator Deplacement(GameObject objAfflicted, List<Transform> pathes)
  { // On déplace le personnage de case en case jusqu'au click du joueur propriétaire, et entre temps on check s'il est taclé ou non
    if (objAfflicted.GetComponent<PushData>())
      objAfflicted.GetComponent<PushData>().Push(actualPushDirection, pushValue);
    yield return null;
    TurnManager.Instance.DisableFinishTurn();

    GameManager.Instance.actualAction = PersoAction.isMoving;
    Transform lastPath = null;

    List<Transform> tempPath = pathes.GetRange(0, pathes.Count);
    bool stopDeplacement = false;

    Vector3 originPoint = Vector3.zero;

    if (objAfflicted.GetComponent<PersoData>() != null)
    {
      originPoint = objAfflicted.GetComponent<PersoData>().originPoint.transform.localPosition;
      objAfflicted.GetComponent<PersoData>().isPushed = true;
    }

    if (objAfflicted.GetComponent<BallonData>() != null)
    {
      originPoint = objAfflicted.GetComponent<BallonData>().offsetBallon;
      objAfflicted.GetComponent<BallonData>().isPushed = true;
    }

    if (objAfflicted.GetComponent<SummonData>() != null)
    {
      originPoint = objAfflicted.GetComponent<SummonData>().originPoint.transform.localPosition;
    }

    bool objectCollision = false;

    PersoData persoSelected = SelectionManager.Instance.selectedPersonnage;

    foreach (Transform path in tempPath)
    {

      if (path.GetComponent<CaseData>().casePathfinding == PathfindingCase.NonWalkable)
      {
        if (path.GetComponent<CaseData>().summonData != null)
        {
          AfterFeedbackManager.Instance.PRText(1, path.gameObject);
          path.GetComponent<CaseData>().summonData.actualPointResistance--;

                   Vector3 newPoint = transform.position + ((path.GetComponent<CaseData>().summonData.transform.position - transform.position) / 2);
                    int random = UnityEngine.Random.Range(1, 4);

                    if (random == 1)
                        FXManager.Instance.Show(fxBallImpact1, newPoint, Direction.NordEst);

                    if (random == 2)
                        FXManager.Instance.Show(fxBallImpact2, newPoint, Direction.NordEst);

                    if (random == 3)
                        FXManager.Instance.Show(fxBallImpact3, newPoint, Direction.NordEst);
                }
        if (path.GetComponent<CaseData>().personnageData != null)
        {

                    Vector3 newPoint = transform.position + ((path.GetComponent<CaseData>().personnageData.transform.position - transform.position) / 2);
                    FXManager.Instance.Show(fxPersonnageHurt, newPoint, Direction.NordEst);
                    int random = UnityEngine.Random.Range(1, 4);

                    if (random == 1)
                        FXManager.Instance.Show(fxBallImpact1, newPoint, Direction.NordEst);

                    if (random == 2)
                        FXManager.Instance.Show(fxBallImpact2, newPoint, Direction.NordEst);

                    if (random == 3)
                        FXManager.Instance.Show(fxBallImpact3, newPoint, Direction.NordEst);

                    if (path.GetComponent<CaseData>().personnageData.timeStunned == 0)
          {
            AfterFeedbackManager.Instance.PRText(1, path.gameObject);
            path.GetComponent<CaseData>().personnageData.actualPointResistance--;


                    }
        }

        if (objAfflicted.GetComponent<PersoData>() != null)
        {
          if (objAfflicted.GetComponent<PersoData>().timeStunned == 0)
          {
            AfterFeedbackManager.Instance.PRText(1, objAfflicted);
            objAfflicted.GetComponent<PersoData>().actualPointResistance--;
          }
        }
        if (objAfflicted.GetComponent<SummonData>() != null && !objAfflicted.GetComponent<SummonData>().invulnerable)
        {
          AfterFeedbackManager.Instance.PRText(1, objAfflicted);
          objAfflicted.GetComponent<SummonData>().actualPointResistance--;
        }
        objectCollision = true;
        break;
      }

      lastPath = path;
      Vector3 startPos = objAfflicted.transform.position;
      float fracturedTime = 0;
      float timeUnit = travelTime / 60;

      if (objAfflicted.GetComponent<PersoData>() != null)
      {
        objAfflicted.GetComponent<PersoData>().RotateTowardsReversed(path.gameObject);
      }

      if (objAfflicted.GetComponent<BallonData>() != null)
      {
        objAfflicted.GetComponent<BallonData>().RotateTowardsReversed(path.gameObject);
      }

      if (path.GetComponent<CaseData>() != null && path.GetComponent<CaseData>().summonData != null && (path.GetComponent<CaseData>().summonData.name.Contains("Air")
        || path.GetComponent<CaseData>().summonData.stopMove))
      {
        stopDeplacement = true;
      }

      while (fracturedTime < 1)
      {
        fracturedTime += timeUnit + 0.01f;
        objAfflicted.transform.position = Vector3.Lerp(startPos, path.transform.position - originPoint, fracturedTime);
        yield return new WaitForEndOfFrame();
      }

      if (stopDeplacement)
      {
        break;
      }

      if (stillTornadoDamage && objAfflicted.GetComponent<SummonData>() == null)
      {
        CaseData afflictedCase = null;
        PersoData persoAfficted = objAfflicted.GetComponent<PersoData>();
        Direction rightDirection = Direction.None;
        if (persoAfficted != null)
        {
          if (persoAfficted.timeStunned == 0)
          {
            AfterFeedbackManager.Instance.PRText(1, objAfflicted);
            persoAfficted.actualPointResistance--;
            afflictedCase = persoAfficted.persoCase;
            rightDirection = persoAfficted.persoDirection;
          }
        }
        BallonData ballonAfficted = objAfflicted.GetComponent<BallonData>();
        if (ballonAfficted != null)
        {
          afflictedCase = ballonAfficted.ballonCase;
          rightDirection = ballonAfficted.ballonDirection;
        }

        CaseData rightCase = afflictedCase.GetCaseAtRight(rightDirection);
        if (rightCase != null)
        {
          if (rightCase.personnageData != null)
          {
            if (persoAfficted.timeStunned == 0)
            {
              AfterFeedbackManager.Instance.PRText(1, rightCase.gameObject);
              rightCase.GetComponent<CaseData>().personnageData.actualPointResistance--;
            }
          }
          if (rightCase.summonData != null)
          {
            AfterFeedbackManager.Instance.PRText(1, rightCase.gameObject);
            rightCase.GetComponent<CaseData>().summonData.actualPointResistance--;
          }
        }
      }
      pushValue--;

      if (objAfflicted.GetComponent<PersoData>())
      {
        objAfflicted.GetComponent<PersoData>().pushedDebt = pushValue;
      }
    }
    GameManager.Instance.actualAction = PersoAction.isSelected;
    SelectionManager.Instance.selectedCase = persoSelected.persoCase;
    CaseManager.Instance.RemovePath();

    if (objectCollision == false && objAfflicted.GetComponent<PersoData>() != null)
    {
      if (pushValue > tempPath.Count)
      {
        if (objAfflicted.GetComponent<PersoData>().timeStunned == 0)
        {
          AfterFeedbackManager.Instance.PRText(1, objAfflicted);
          objAfflicted.GetComponent<PersoData>().actualPointResistance--;
        }
      }
    }

    retainedPushValue = pushValue;
    tempPath.Clear();

    StartCoroutine(TurnManager.Instance.EnableFinishTurn());
    yield return new WaitForEndOfFrame();
  }

  public void GetShownCase(GameObject objPushed, int pushValue, CaseData startCase, Direction pushDirection)
  {
    caseFinalShow = null;

    if(pushValue < 0)
    {
      pushValue = -pushValue;
      if (pushDirection == Direction.NordEst)
        pushDirection = Direction.SudOuest;
      else if (pushDirection == Direction.NordOuest)
        pushDirection = Direction.SudEst;
      else if (pushDirection == Direction.SudEst)
        pushDirection = Direction.NordOuest;
      else if (pushDirection == Direction.SudOuest)
        pushDirection = Direction.NordEst;
    }

    CaseData targetCase = startCase;
    List<SummonData> destroyedSummon = new List<SummonData>();

    while (pushValue > 0)
    {
      if (pushDirection == Direction.NordEst)
        targetCase = targetCase.GetTopRightCase();
      if (pushDirection == Direction.NordOuest)
        targetCase = targetCase.GetTopLeftCase();
      if (pushDirection == Direction.SudEst)
        targetCase = targetCase.GetBottomRightCase();
      if (pushDirection == Direction.SudOuest)
        targetCase = targetCase.GetBottomLeftCase();

      pushValue--;

      if (targetCase == null || targetCase.casePathfinding == PathfindingCase.NonWalkable && 
        (targetCase.ballon && targetCase.ballon.gameObject != objPushed || targetCase.personnageData && targetCase.personnageData.gameObject != objPushed))
      {
        pushValue = 0;
        Debug.Log("collision !");
      }
      else if (targetCase.summonData != null && targetCase.summonData.name.Contains("Water") && pushValue >= 1 && !destroyedSummon.Contains(targetCase.summonData))
      {
        pushValue = 0;
        Debug.Log("arret !");
        caseFinalShow = targetCase;
      }
      else if (targetCase.summonData != null && targetCase.summonData.name.Contains("Air") && !destroyedSummon.Contains(targetCase.summonData))
      {
        pushValue += 1;
        pushDirection = targetCase.summonData.pushDirection;
        destroyedSummon.Add(targetCase.summonData);
      }
      else
        caseFinalShow = targetCase;
    }

    return;
    /*
    CaseData nextCase = caseAfflicted;
    int y = pushValue;
    caseFinalShow = null;
    if (pushType == PushType.FromCaster)
    { // tornade à implémenter
      while (y != 0)
      {
        if (y > 0)
        {
          y--;
          if (nextCase.GetCaseInFront(SelectionManager.Instance.selectedPersonnage.persoDirection) != null)
          {
            nextCase = nextCase.GetCaseInFront(SelectionManager.Instance.selectedPersonnage.persoDirection);
            if (nextCase.casePathfinding == PathfindingCase.NonWalkable)
            {
              caseFinalShow = nextCase.GetCaseAtBack(SelectionManager.Instance.selectedPersonnage.persoDirection);
            }
          }
        }
        if (y < 0)
        {
          y++;
          if (nextCase.GetCaseAtBack(SelectionManager.Instance.selectedPersonnage.persoDirection) != null)
          {
            nextCase = nextCase.GetCaseAtBack(SelectionManager.Instance.selectedPersonnage.persoDirection);
            if (nextCase.casePathfinding == PathfindingCase.NonWalkable)
            {
              if (y == 1)
                caseFinalShow = caseAfflicted;
              else
                caseFinalShow = nextCase.GetCaseInFront(SelectionManager.Instance.selectedPersonnage.persoDirection);
              break;
            }
          }
        }
      }
      if (caseFinalShow == null)
      {
        caseFinalShow = nextCase;
      }
    }
    else if (pushType == PushType.FromTarget)
    {
      if (pushDirection == Direction.Left)
        caseFinalShow = nextCase.GetCaseAtLeft(SelectionManager.Instance.selectedPersonnage.persoDirection);

      if (pushDirection == Direction.Right)
        caseFinalShow = nextCase.GetCaseAtRight(SelectionManager.Instance.selectedPersonnage.persoDirection);

      if (pushDirection == Direction.Back)
      {
        caseFinalShow = nextCase.GetCaseAtBack(SelectionManager.Instance.selectedPersonnage.persoDirection);
      }

      if (pushDirection == Direction.Front)
        caseFinalShow = nextCase.GetCaseInFront(SelectionManager.Instance.selectedPersonnage.persoDirection);

      pushDirection = SelectionManager.Instance.selectedPersonnage.persoDirection;

      for (int i = 0; i < pushValue; i++)
      {
        if (nextCase != null)
          if (nextCase.summonData != null)
            if (nextCase.summonData.name.Contains("Air"))
            {
              if (pushDirection == Direction.NordEst)
                pushDirection = Direction.NordOuest;
              if (pushDirection == Direction.NordOuest)
                pushDirection = Direction.SudOuest;
              if (pushDirection == Direction.SudOuest)
                pushDirection = Direction.SudEst;
              if (pushDirection == Direction.SudEst)
                pushDirection = Direction.NordEst;
            }
        /*
        if (pushDirection == Direction.Left)
          caseFinalShow = nextCase.GetCaseAtLeft(SelectionManager.Instance.selectedPersonnage.persoDirection);

        if (pushDirection == Direction.Right)
          caseFinalShow = nextCase.GetCaseAtRight(SelectionManager.Instance.selectedPersonnage.persoDirection);

        if (pushDirection == Direction.Back)
          caseFinalShow = nextCase.GetCaseAtBack(SelectionManager.Instance.selectedPersonnage.persoDirection);

        if (pushDirection == Direction.Front)
          caseFinalShow = nextCase.GetCaseInFront(SelectionManager.Instance.selectedPersonnage.persoDirection);

        if (tempCase == null || tempCase.casePathfinding == PathfindingCase.NonWalkable)
        {
          caseAfflicted.casePathfinding = PathfindingCase.Walkable;
          pathList.Add(caseAfflicted.transform);
          stillTornadoDamage = true;
          break;
        }
        else
        {
          pathList.Add(tempCase.transform);
          caseAfflicted = tempCase;
        }*/
      }

      //GetCaseAtRight(objAfflicted.GetComponent<PersoData>().persoDirection);
    }

