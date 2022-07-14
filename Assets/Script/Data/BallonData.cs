using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

/// <summary>Tout ce qu'il est possible de faire avec un ballon, ainsi que toutes ses données.</summary>
public class BallonData : NetworkBehaviour
{

  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  [Header("Data")]
  [SerializeField]
  [EnumFlagAttribute]
  public BallonStatut statut;
  [Space(100)]
  public bool isMoving = false;
  public bool isExplosive = false;
  public Player explosionOwner = Player.Neutral;

  [Header("  Temps")]
  public float travelTimeBallon;

  public Direction ballonDirection;
  public CaseData ballonCase;

  [Header("Tir")]
  public GameObject selectedBallon;
  public Vector3 offsetBallon;
  public float xCoordInc;
  public float yCoordInc;
  public float xCoord;
  public float yCoord;
  public bool canRebond;
  public int casesCrossed;
  public bool isPushed;

  [Header("Feedback")]
  Animator animator;
  public SpriteRenderer spriteR;
  public Image img;
  public Material shaderNormal;
  public Material shaderExplosive;
  bool ShineColorIsRunning = false;
  bool fadeRunning;

    public RuntimeAnimatorController fxShotBall;
    public RuntimeAnimatorController fxPersonnageHurt;
    public RuntimeAnimatorController fxBallImpact1;
    public RuntimeAnimatorController fxBallImpact2;
    public RuntimeAnimatorController fxBallImpact3;

    /// <summary>
    /// Tous les sorts qui ont touché le ballon ce tour-ci
    /// </summary>
    public List<SpellData> spellHit = new List<SpellData>();

  // ******************** //
  // ** Initialisation ** // Fonctions de départ, non réutilisable
  // ******************** //

  void Awake()
  {
    name = "Ballon";
  }

  void Start()
  {
    spriteR = GetComponent<SpriteRenderer>();
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
    animator = GetComponent<Animator>();
    TurnManager.Instance.changeTurnEvent += OnChangeTurn;
  }

  private void Update()
  {
    if (ballonCase.GetTopRightCase() != null && ballonCase.GetTopRightCase().personnageData != null)
    {
      if(!fadeRunning)
      StartCoroutine(FadeIn());
    }
    else if (ballonCase.GetTopLeftCase() != null && ballonCase.GetTopLeftCase().personnageData != null)
    {
      if (!fadeRunning)
        StartCoroutine(FadeIn());
    }
    else if (ballonCase.GetBottomRightCase() != null && ballonCase.GetBottomRightCase().personnageData != null)
    {
      if (!fadeRunning)
        StartCoroutine(FadeIn());
    }
    else if (ballonCase.GetBottomLeftCase() != null && ballonCase.GetBottomLeftCase().personnageData != null)
    {
      if (!fadeRunning)
        StartCoroutine(FadeIn());
    }
    else
    {
      img.material.SetFloat("_OutlineSize", 5);
      StopCoroutine(FadeIn());
      fadeRunning = false;
    }
  }

  // *************** //
  // ** Events **    // Appel de fonctions au sein de ce script grâce à des events
  // *************** //

  public void OnChangeTurn(object sender, PlayerArgs e)
  {
    spellHit.Clear();
  }

  // *************** //
  // ** Fonctions ** // Fonctions réutilisables ailleurs
  // *************** //

  /// <summary>Déplace le ballon devant le personnage ayant tiré.</summary>
  public IEnumerator Move()
  {
    casesCrossed = 0;
    CaseData hoveredCase = HoverManager.Instance.hoveredCase;
    PersoData selectedPersonnage = SelectionManager.Instance.selectedPersonnage;

    if (GameManager.Instance.manaGlobalActual < 2)
      yield return null;
    EffectManager.Instance.ChangePA(-2);

    GameManager.Instance.actualAction = PersoAction.isShoting;
    TurnManager.Instance.DisableFinishTurn();
    MenuContextuel.Instance.isShoting = true;
    isPushed = false;

    if (isExplosive)
    {
      explode();
      yield return null;
    }
    GameObject nextPosition;

    animator.SetTrigger("Roule");
    isMoving = true;
    offsetBallon = new Vector2(0, 0);
    float xCoordNext = hoveredCase.GetComponent<CaseData>().xCoord;
    float yCoordNext = hoveredCase.GetComponent<CaseData>().yCoord;

    xCoordInc = 0;
    yCoordInc = 0;
    transform.localRotation = Quaternion.Euler(0, 0, 0);

        Vector3 newPoint = transform.position + ((selectedPersonnage.transform.position - transform.position) / 2);
        FXManager.Instance.Show(fxShotBall, newPoint, Direction.NordEst);

        if (selectedPersonnage.transform.position.x > transform.position.x)
    {
      xCoordInc -= 0.5f;
      yCoordInc -= 0.5f;
    }
    else if (selectedPersonnage.transform.position.x < transform.position.x)
    {
      xCoordInc += 0.5f;
      yCoordInc += 0.5f;
    }

    if (selectedPersonnage.transform.position.y - 0.5f > transform.position.y)
    {
      xCoordInc += 0.5f;
      yCoordInc -= 0.5f;
    }
    else if (selectedPersonnage.transform.position.y - 0.5f < transform.position.y)
    {
      xCoordInc -= 0.5f;
      yCoordInc += 0.5f;
    }
    for (int i = 0; i < selectedPersonnage.shotStrenght; i++)
    {
      if (isMoving == false)
      {
        goto endMove;
      }

      if ((BallonStatut.isIntercepted & statut) == BallonStatut.isIntercepted)
      {
        ChangeStatut(BallonStatut.None, BallonStatut.isIntercepted);
        break;
      }
      xCoordNext += xCoordInc;
      yCoordNext += yCoordInc;
      if (GameObject.Find(xCoordNext.ToString() + " " + yCoordNext.ToString()) != null)
      {
        nextPosition = GameObject.Find(xCoordNext.ToString() + " " + yCoordNext.ToString());
        if (nextPosition.GetComponent<CaseData>().casePathfinding == PathfindingCase.NonWalkable)
        {
          AfterFeedbackManager.Instance.PRText(1, nextPosition);
          if (nextPosition.GetComponent<CaseData>().personnageData != null && nextPosition.GetComponent<CaseData>().personnageData.timeStunned == 0)
          {
            nextPosition.GetComponent<CaseData>().personnageData.actualPointResistance--;

                        newPoint = transform.position + ((nextPosition.GetComponent<CaseData>().personnageData.transform.position - transform.position) / 2);
                        FXManager.Instance.Show(fxPersonnageHurt, newPoint, Direction.NordEst);
                        
                        int random = UnityEngine.Random.Range(1, 4);

                        if (random == 1)
                            FXManager.Instance.Show(fxBallImpact1, newPoint, Direction.NordEst);

                        if (random == 2)
                            FXManager.Instance.Show(fxBallImpact2, newPoint, Direction.NordEst);

                        if (random == 3)
                            FXManager.Instance.Show(fxBallImpact3, newPoint, Direction.NordEst);
                    }
          if (nextPosition.GetComponent<CaseData>().summonData != null)
          {
            nextPosition.GetComponent<CaseData>().summonData.actualPointResistance--;

                        newPoint = transform.position + ((nextPosition.GetComponent<CaseData>().summonData.transform.position - transform.position) / 2);

                        int random = UnityEngine.Random.Range(1, 4);

                        if (random == 1)
                        FXManager.Instance.Show(fxBallImpact1, newPoint, Direction.NordEst);

                        if (random == 2)
                            FXManager.Instance.Show(fxBallImpact2, newPoint, Direction.NordEst);

                        if (random == 3)
                            FXManager.Instance.Show(fxBallImpact3, newPoint, Direction.NordEst);
                    }
          goto endMove;
        }
      }
      else
      {
                if (ballonDirection == Direction.NordEst)
                    newPoint = transform.position + new Vector3(0.3f,0.1f);

                if (ballonDirection == Direction.NordOuest)
                    newPoint = transform.position + new Vector3(-0.3f, 0.1f);

                if (ballonDirection == Direction.SudEst)
                    newPoint = transform.position + new Vector3(0.3f, -0.1f);

                if (ballonDirection == Direction.SudOuest)
                    newPoint = transform.position + new Vector3(-0.3f, -0.1f);

                int random = UnityEngine.Random.Range(1, 4);

                if (random == 1)
                    FXManager.Instance.Show(fxBallImpact1, newPoint, Direction.NordEst);

                if (random == 2)
                    FXManager.Instance.Show(fxBallImpact2, newPoint, Direction.NordEst);

                if (random == 3)
                    FXManager.Instance.Show(fxBallImpact3, newPoint, Direction.NordEst);
                goto endMove;
      }

      if (xCoordNext == ballonCase.GetComponent<CaseData>().xCoord)
      {
        if (yCoordNext < ballonCase.GetComponent<CaseData>().yCoord)
        {
          ballonDirection = Direction.SudOuest;
        }
        else
        {
          ballonDirection = Direction.NordEst;
        }
      }
      else if (yCoordNext == ballonCase.GetComponent<CaseData>().yCoord)
      {
        if (xCoordNext < ballonCase.GetComponent<CaseData>().xCoord)
        {
          ballonDirection = Direction.NordOuest;
        }
        else
        {
          ballonDirection = Direction.SudEst;
        }
      }
      ChangeRotation(ballonDirection);
      Vector3 startPos = transform.position;
      float fracturedTime = 0;
      float timeUnit = travelTimeBallon / 60;
      while (transform.position != nextPosition.transform.position)
      {
        fracturedTime += timeUnit + 0.01f;
        transform.position = Vector3.Lerp(startPos, nextPosition.transform.position, fracturedTime);
        yield return new WaitForEndOfFrame();
      }
      casesCrossed++;
      TackleBehaviour.Instance.CheckTackle(this.gameObject, selectedPersonnage);
    }
    endMove:
    StopMove();
  }

  /// <summary>Stop le mouvement du ballon.</summary>
  public void StopMove()
  {
    isMoving = false;
    GameManager.Instance.actualAction = PersoAction.isSelected;
    animator.ResetTrigger("Roule");
    animator.SetTrigger("Idle");
    TurnManager.Instance.EnableFinishTurn();
    casesCrossed = 0;
  }

  /// <summary>Change la rotation du sprite du ballon.</summary>
  public void ChangeRotation(Direction direction)
  {
    ballonDirection = direction;
    switch (direction)
    {
      case Direction.SudOuest:
      transform.localRotation = Quaternion.Euler(0, 0, 0);
      break;
      case Direction.NordOuest:
      transform.localRotation = Quaternion.Euler(0, 0, 0);
      break;
      case Direction.SudEst:
      transform.localRotation = Quaternion.Euler(0, 0, 0);
      break;
      case Direction.NordEst:
      transform.localRotation = Quaternion.Euler(0, 0, 0);
      break;
    }

    MenuContextuel.Instance.isShoting = false;
  }

  /// <summary>Change le statut du ballon.</summary>
  public void ChangeStatut(BallonStatut newStatut = BallonStatut.None, BallonStatut oldStatut = BallonStatut.None)
  {
    BallonStatut lastStatut = statut;
    if ((newStatut != BallonStatut.None) && !((newStatut & statut) == newStatut))
      statut += (int)newStatut;
    if ((oldStatut != BallonStatut.None) && ((oldStatut & statut) == oldStatut))
      statut -= (int)oldStatut;
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

  public void RotateTowardsReversed(GameObject targetCasePosGMB)
  {
    Vector3 targetCasePos = targetCasePosGMB.transform.position;
    Vector3 originCasePos = ballonCase.transform.position;

    if (originCasePos.x > targetCasePos.x && originCasePos.y > targetCasePos.y)
      ChangeRotation(Direction.NordEst);

    if (originCasePos.x > targetCasePos.x && originCasePos.y < targetCasePos.y)
      ChangeRotation(Direction.SudEst);

    if (originCasePos.x < targetCasePos.x && originCasePos.y > targetCasePos.y)
      ChangeRotation(Direction.NordOuest);

    if (originCasePos.x < targetCasePos.x && originCasePos.y < targetCasePos.y)
      ChangeRotation(Direction.SudOuest);
  }

  public void setExplosive(Player owner)
  {
    explosionOwner = owner;
    img.material = shaderExplosive;
    isExplosive = true;
  }

  public void explode()
  {
    AfterFeedbackManager.Instance.ExplodeEffect(ballonCase.gameObject);
    img.material = shaderNormal;
    StartCoroutine(explosion());
  }

  IEnumerator explosion()
  {
    CaseData caseExplosion = ballonCase;
    damageTarget(caseExplosion.GetBottomLeftCase(), Direction.SudOuest);
    yield return new WaitForEndOfFrame();
    damageTarget(caseExplosion.GetBottomRightCase(), Direction.SudEst);
    yield return new WaitForEndOfFrame();
    damageTarget(caseExplosion.GetTopRightCase(), Direction.NordEst);
    yield return new WaitForEndOfFrame();
    damageTarget(caseExplosion.GetTopLeftCase(), Direction.NordOuest);
    isExplosive = false;
  }

  private void damageTarget(CaseData tempCase, Direction direction)
  {
    if (tempCase == null)
      return;
    if (tempCase.personnageData != null && tempCase.personnageData.timeStunned == 0)
    {
      //EffectManager.Instance.MultiplePush(tempCase.personnageData.gameObject, tempCase, 1, PushType.FromTerrain, direction);
      EffectManager.Instance.ChangePr(tempCase.personnageData, 1);
      AfterFeedbackManager.Instance.PRText(1, tempCase.gameObject);
    }
    if (tempCase.summonData != null && !tempCase.summonData.invulnerable)
    {
      EffectManager.Instance.ChangePr(tempCase.summonData, -1);
      AfterFeedbackManager.Instance.PRText(1, tempCase.gameObject);
    }

  }

  public void ShotPrevisualisation()
  {
    if (ballonCase.GetCaseInFront(SelectionManager.Instance.selectedCase.GetDirectionBetween(ballonCase)) == null)
    {
      return;
    }
    CaseData newCase = ballonCase.GetCaseInFront(SelectionManager.Instance.selectedCase.GetDirectionBetween(ballonCase));
    if (newCase == null | newCase.casePathfinding == PathfindingCase.NonWalkable)
      return;
    newCase.ChangeStatut(Statut.shotPrevisu, Statut.None);
    for (int i = 0; i < SelectionManager.Instance.selectedPersonnage.shotStrenght - 1; i++)
    {
      if (newCase.GetCaseInFront(SelectionManager.Instance.selectedCase.GetDirectionBetween(newCase)) == null |
        newCase.GetCaseInFront(SelectionManager.Instance.selectedCase.GetDirectionBetween(newCase)).casePathfinding == PathfindingCase.NonWalkable)
        break;
      newCase = newCase.GetCaseInFront(SelectionManager.Instance.selectedCase.GetDirectionBetween(newCase));
      newCase.ChangeStatut(Statut.shotPrevisu, Statut.None);
    }
    BeforeFeedbackManager.Instance.PredictDeplacement(gameObject, newCase);
  }

  public void ShotDeprevisualisation()
  {
    foreach (CaseData newCase in CaseManager.Instance.GetAllCase())
    {
      newCase.ChangeStatut(Statut.None, Statut.shotPrevisu);
    }
    BeforeFeedbackManager.Instance.HidePrediction();
  }

  IEnumerator FadeIn()
  {
    fadeRunning = true;
    bool increment = true;
    float min = 5f;//time you want it to run
    float max = 15.0f;//time you want it to run
    float value = 5f;
    float interval = 0.1f;//interval time between iterations of while loop
    img.material.SetFloat("_OutlineSize", 5);


 
    while (true)
    {
      if (increment)
      {
        value += interval;
        img.material.SetFloat("_OutlineSize", value);
        if (value >= max)
        {
          value = max;
          increment = false;
        }
      }
      else
      {
        value -= interval;
        img.material.SetFloat("_OutlineSize", value);
        if (value <= min)
        {
          value = min;
          increment = true;
        }
      }
      yield return new WaitForEndOfFrame();
    }
  }
}
