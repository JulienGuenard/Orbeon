using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushData : MonoBehaviour
{
  public PersoData PersoData;
  public BallonData BallonData;
  public int pushValue;
  public Direction pushDirection;
  public CaseData selfCase;
  public CaseData targetCase;
  public Vector3 targetPos;
  public bool isPushing;
  public Vector3 startPos;
  float fracJourney = 0;

  // Use this for initialization
  IEnumerator Start()
  {
    yield return new WaitForSeconds(0.1f);
    if (GetComponent<PersoData>())
    {
      PersoData = GetComponent<PersoData>();
    }
    if (GetComponent<BallonData>())
    {
      BallonData = GetComponent<BallonData>();
    }
  }

  public void Push(Direction pushDirection, int pushValue)
  {
        this.pushDirection = pushDirection;
    this.pushValue += pushValue;

    TurnManager.Instance.DisableFinishTurn();
    GameManager.Instance.actualAction = PersoAction.isMoving;

    enabled = true;
  }


  // Update is called once per frame
  void FixedUpdate()
  {
    if (!isPushing && pushValue != 0)
    {
      setTarget();
      if (!verifyNextTarget())
      {
        pushValue = 0;
      }
      else
        isPushing = true;
    }
    if (isPushing)
    {
      Mathf.Clamp(fracJourney, 0, 1);
      if (PersoData != null)
      {
        transform.position = Vector3.Lerp(startPos - PersoData.originPoint.transform.localPosition, targetPos - PersoData.originPoint.transform.localPosition, fracJourney);
        fracJourney += 0.2f;
        //        transform.position += (targetPos - startPos) * Time.deltaTime * 2;
      }
      else if (BallonData != null)
      {
        transform.position = Vector3.Lerp(startPos, targetPos, fracJourney);
        fracJourney += 0.2f;
        //        transform.position += (targetPos - startPos) * Time.deltaTime * 2;
      }

      if (fracJourney > 1)
      {
        isPushing = false;
        fracJourney = 0;
        if (pushValue == 0)
        {
          GameManager.Instance.actualAction = PersoAction.isSelected;
          selfCase = null;
          targetCase = null;
          enabled = false;
          StartCoroutine(TurnManager.Instance.EnableFinishTurn());
                    GameManager.Instance.CanPlayTurn(true);
        }
      }
    }
  }

  public void setTarget()
  {
    targetCase = null;
    if (PersoData)
    {
      startPos = transform.position + PersoData.originPoint.transform.localPosition;
      selfCase = PersoData.persoCase;
    }
    if (BallonData)
    {
      startPos = transform.position;
      selfCase = BallonData.ballonCase;
    }

    if (pushValue > 0)
    {
      if (pushDirection == Direction.NordEst)
        targetCase = selfCase.GetTopRightCase();
      if (pushDirection == Direction.NordOuest)
        targetCase = selfCase.GetTopLeftCase();
      if (pushDirection == Direction.SudEst)
        targetCase = selfCase.GetBottomRightCase();
      if (pushDirection == Direction.SudOuest)
        targetCase = selfCase.GetBottomLeftCase();
      pushValue--;
    }
    if (pushValue < 0)
    {
      if (pushDirection == Direction.NordEst)
        targetCase = selfCase.GetBottomLeftCase();
      if (pushDirection == Direction.NordOuest)
        targetCase = selfCase.GetBottomRightCase();
      if (pushDirection == Direction.SudEst)
        targetCase = selfCase.GetTopLeftCase();
      if (pushDirection == Direction.SudOuest)
        targetCase = selfCase.GetTopRightCase();
      pushValue++;
    }
    if (targetCase)
      targetPos = targetCase.transform.position;
  }

  bool verifyNextTarget()
  {
    if (targetCase == null || targetCase.casePathfinding == PathfindingCase.NonWalkable)
    {
      // self
      if (PersoData != null)
      {
        if (PersoData.timeStunned == 0)
        {
          AfterFeedbackManager.Instance.PRText(1, PersoData.persoCase.gameObject);
          PersoData.actualPointResistance--;
        }
      }
      if (targetCase == null)
        return false;
      // target
      if (targetCase.summonData != null)
      {
        AfterFeedbackManager.Instance.PRText(1, targetCase.gameObject);
        targetCase.summonData.actualPointResistance--;
      }
      if (targetCase.personnageData != null)
      {
        if (targetCase.personnageData.timeStunned == 0)
        {
          AfterFeedbackManager.Instance.PRText(1, targetCase.gameObject);
          targetCase.personnageData.actualPointResistance--;
        }
      }
      return false;
    }
    return true;
  }
}

