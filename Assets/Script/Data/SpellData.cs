using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Tout ce qu'il est possible de faire avec un sort, ainsi que toutes ses données.</summary>
public class SpellData : NetworkBehaviour
{

  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  public int costPA;

  public int range;
  public bool isLinear;

  public int areaOfEffect;

  public int damagePR;
  public int damagePA;
    public int manaIncrementPerSummon;
  public int damagePM;
  public bool reverseDamageOnAlly;
  public Element elementCreated;

  public int pushValue;
  public Direction pushDirection;
  public PushType pushType;
  public bool hurtWhenStopped;

  public bool absorbInvoc;
  public bool explosion;

  public SummonData summonedObj;
  public bool rotateSummon;
  /// <summary>
  /// Specifique au sort direct de feu
  /// Fait une invocation dans les 4 directions autour de la cible
  /// </summary>
  public bool summonOnCross;
  SummonData summonedObjInstancied;
  public bool isDirect;

  public SpellData nextSpell;

  public Sprite buttonSprite;

  List<CaseData> AoEList = new List<CaseData>();
  List<CaseData> pushList = new List<CaseData>();
 

  [TextArea] public string tooltipTitle;
  [TextArea] public string tooltipRange;
  [TextArea] public string tooltipCost;
  [TextArea] public string tooltipEffect;

  public int maxUsePerTurn;

  public RuntimeAnimatorController animatorSpell;
    public RuntimeAnimatorController animatorSpell2;
    public RuntimeAnimatorController animatorSpell3;
    public RuntimeAnimatorController animatorSpell4;
    public Vector3 fxPositionModifier;
    public Vector3 fxPositionModifier2;

    [EnumFlagAttribute]
  [Space(40)]
  public ObjectType affectedTarget;
  [EnumFlagAttribute]
  [Space(40)]
  public ObjectType allowedTarget;

  List<CaseData> rangeList = new List<CaseData>();
  List<CaseData> targetList = new List<CaseData>();

  // ******************** //
  // ** Initialisation ** // Fonctions de départ, non réutilisable
  // ******************** //

  void Start()
  {
    StartCoroutine(waitForInit());
  }

  IEnumerator waitForInit()
  {
    yield return new WaitForEndOfFrame();
    while (TurnManager.Instance == null)
    {
      yield return null;
    }
    Init();
  }

  public void Init()
  {

  }

  // *************** //
  // ** Fonctions ** // Fonctions réutilisables ailleurs
  // *************** //

  public void ShowAllFeedbacks()
  {
    foreach (CaseData obj in CaseManager.listAllCase)
    {
      obj.ChangeStatut(Statut.None, Statut.atAoE);
      obj.ChangeStatut(Statut.None, Statut.atPush);
    }

    if (rangeList.Contains(HoverManager.Instance.hoveredCase))
    {
      ShowAreaOfEffect();
    }

    if (!targetList.Contains(HoverManager.Instance.hoveredCase))
    {
      ManaManager.Instance.SpellButtonFeedbackOFF();
      BeforeFeedbackManager.Instance.HidePrediction();
      return;
    }

    ManaManager.Instance.SpellButtonFeedbackON(SpellManager.Instance.selectedSpell.costPA);
    ShowPushEffect();
    ShowSummon();
  }

  public void newRangeList()
  {
    if (SelectionManager.Instance.selectedCase == null)
      return;

    List<CaseData> list = new List<CaseData>();
    List<CaseData> list2 = new List<CaseData>();
    CaseData selectedCase = SelectionManager.Instance.selectedCase;
    list.Add(selectedCase);
    rangeList.Clear();

    // get des cases
    if (isLinear)
    {
      for (int i = 0; i < range + 1; i++)
      {
        if (selectedCase.GetCaseRelativeCoordinate(i, 0) != null)
          list.Add(selectedCase.GetCaseRelativeCoordinate(i, 0));

        if (selectedCase.GetCaseRelativeCoordinate(-i, 0) != null)
          list.Add(selectedCase.GetCaseRelativeCoordinate(-i, 0));

        if (selectedCase.GetCaseRelativeCoordinate(0, i) != null)
          list.Add(selectedCase.GetCaseRelativeCoordinate(0, i));

        if (selectedCase.GetCaseRelativeCoordinate(0, -i) != null)
          list.Add(selectedCase.GetCaseRelativeCoordinate(0, -i));
      }
    }
    else
    {
      for (int i = 0; i < range; i++)
      {
        foreach (CaseData obj in list)
        {
          if (obj.GetBottomLeftCase() != null)
            list2.Add(obj.GetBottomLeftCase());

          if (obj.GetBottomRightCase() != null)
            list2.Add(obj.GetBottomRightCase());

          if (obj.GetTopLeftCase() != null)
            list2.Add(obj.GetTopLeftCase());

          if (obj.GetTopRightCase() != null)
            list2.Add(obj.GetTopRightCase());
        }
        list.AddRange(list2);
        list2.Clear();
      }
    }
    rangeList.AddRange(list);
    foreach (CaseData obj in rangeList)
    {
      if (obj != null)
      {
        obj.ChangeStatut(Statut.atRange);
      }
    }
  }

  bool CheckRange()
  {
    foreach (CaseData obj in rangeList)
    {
      if (obj != null)
        obj.ChangeStatut(Statut.atRange);
    }
    return true;
  }

  bool CheckTarget()
  {
    foreach (CaseData obj in targetList)
    {
      if (obj != null)
        obj.ChangeStatut(Statut.canTarget);
    }
    return true;
  }

  public void newTargetList()
  {
    CaseData hoveredCase = HoverManager.Instance.hoveredCase;
    targetList.Clear();
    foreach (CaseData obj in rangeList)
    {
      if ((ObjectType.Invoc & allowedTarget) == ObjectType.Invoc)
      {
        if (obj.summonData != null && !targetList.Contains(obj) && !obj.summonData.isTraversable)
        {
          if (pushValue == 0 || pushValue > 0 && !obj.summonData.isStatic)
            targetList.Add(obj);
        }
      }

      if ((ObjectType.AllyPerso & allowedTarget) == ObjectType.AllyPerso)
        if (obj.personnageData != null && obj.personnageData.owner == GameManager.Instance.currentPlayer && !targetList.Contains(obj))
          targetList.Add(obj);

      if ((ObjectType.EnemyPerso & allowedTarget) == ObjectType.EnemyPerso)
        if (obj.personnageData != null && obj.personnageData.owner != GameManager.Instance.currentPlayer && !targetList.Contains(obj))
          targetList.Add(obj);

      if ((ObjectType.Ballon & allowedTarget) == ObjectType.Ballon)
        if (obj.ballon != null && !targetList.Contains(obj))
          targetList.Add(obj);

      if ((ObjectType.EmptyCase & allowedTarget) == ObjectType.EmptyCase)
        if (obj.casePathfinding == PathfindingCase.Walkable && !targetList.Contains(obj) && obj.summonData == null)
          targetList.Add(obj);

      if (!((ObjectType.Self & allowedTarget) == ObjectType.Self))
        if (obj == SelectionManager.Instance.selectedCase && targetList.Contains(obj))
          targetList.Remove(obj);
    }
    foreach (CaseData obj in targetList)
    {
      if (obj != null)
        obj.ChangeStatut(Statut.canTarget);
    }
  }

  /// <summary>Montre l'AoE du sort avant de le lancer</summary>
  public void ShowAreaOfEffect()
  {

    CaseData hoveredCase = HoverManager.Instance.hoveredCase;

    foreach (CaseData obj in CaseManager.Instance.GetAllCaseWithStatut(Statut.atAoE))
    {
      obj.ChangeStatut(Statut.None, Statut.atAoE);
    }

    int AoE = areaOfEffect;
    List<CaseData> list = new List<CaseData>();
    List<CaseData> list2 = new List<CaseData>();
    list.Add(hoveredCase);
    for (int i = 0; i < AoE; i++)
    {
      foreach (CaseData obj in list)
      {
        if (obj.GetBottomLeftCase() != null)
          list2.Add(obj.GetBottomLeftCase());

        if (obj.GetBottomRightCase() != null)
          list2.Add(obj.GetBottomRightCase());

        if (obj.GetTopLeftCase() != null)
          list2.Add(obj.GetTopLeftCase());

        if (obj.GetTopRightCase() != null)
          list2.Add(obj.GetTopRightCase());
      }
      list.AddRange(list2);
      list2.Clear();
    }

    if (AoEList.Count != 0)
      AoEList.Clear();

    AoEList.AddRange(list);
    foreach (CaseData obj in AoEList)
    {
      obj.ChangeStatut(Statut.atAoE);
    }
  }

  /// <summary>Montre à quelle portée les personnages vont être projetés avant de le lancer</summary>
  public void ShowPushEffect()
  {
    if (HoverManager.Instance.hoveredPersonnage == null && HoverManager.Instance.hoveredBallon == null)
      return;

    if (pushDirection == Direction.None)
      return;
    if (transform.name.Contains("AirIndirect"))
      return;

    if (HoverManager.Instance.hoveredPersonnage)
    {
      PersoData persoAfflicted = HoverManager.Instance.hoveredPersonnage;
      PushBehaviour.Instance.PushCheck(persoAfflicted.gameObject, pushValue, persoAfflicted.persoCase, pushType, pushDirection);
      if (persoAfflicted.persoCase != PushBehaviour.Instance.caseFinalShow)
        BeforeFeedbackManager.Instance.PredictDeplacement(persoAfflicted.gameObject, PushBehaviour.Instance.caseFinalShow);
      else
        BeforeFeedbackManager.Instance.HidePrediction();
    }
    else if (HoverManager.Instance.hoveredBallon)
    {
      BallonData ballonAfflicted = HoverManager.Instance.hoveredBallon;
      PushBehaviour.Instance.PushCheck(ballonAfflicted.gameObject, pushValue, ballonAfflicted.ballonCase, pushType, pushDirection);
      if (ballonAfflicted.ballonCase != PushBehaviour.Instance.caseFinalShow)
        BeforeFeedbackManager.Instance.PredictDeplacement(ballonAfflicted.gameObject, PushBehaviour.Instance.caseFinalShow);
      else
        BeforeFeedbackManager.Instance.HidePrediction();
    }
  }


  /// <summary>Montre l'invocation avant de le lancer</summary>
  public void ShowSummon()
  {
    CaseData hoveredCase = HoverManager.Instance.hoveredCase;

    if (summonedObj != null && !summonOnCross)
    {
      if (SummonManager.Instance.lastSummonInstancied == null)
      {
          SummonManager.Instance.lastSummonInstancied = (SummonData)Instantiate(summonedObj, hoveredCase.transform.position + summonedObj.transform.position - summonedObj.originPoint.position, Quaternion.identity);
          SummonManager.Instance.lastSummonInstancied.owner = GameManager.Instance.currentPlayer;
          SummonManager.Instance.lastSummonInstancied.element = elementCreated;
          SummonManager.Instance.lastSummonInstancied.ChangeSpriteByPlayer();
          SummonManager.Instance.lastSummonInstancied.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
      }
      SummonManager.Instance.lastSummonInstancied.transform.position = hoveredCase.transform.position + SummonManager.Instance.lastSummonInstancied.transform.position - SummonManager.Instance.lastSummonInstancied.originPoint.position;
    }
    if (SummonManager.Instance.crossSummonList != null && summonOnCross)
    {
      // ne sert uniquement si les données sont incomplètes
      if (SummonManager.Instance.crossSummonList.Count != 4) //Init ou reset
      {
        // reset les anciennes données
        foreach (SummonData item in SummonManager.Instance.crossSummonList)
        {
          Destroy(item.gameObject);
        }
        SummonManager.Instance.crossSummonList.Clear();

        // creation des nouvelles données
        for (int j = 0; j < 4; j++)
        {
          SummonData summon = new SummonData();
          summon = (SummonData)Instantiate(summonedObj);
          SummonManager.Instance.crossSummonList.Add(summon);
          summon.owner = GameManager.Instance.currentPlayer;
          summon.element = elementCreated;
          summon.ChangeSpriteByPlayer();
          summon.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        }
      }
      // on place les summon
      int i = 0;
      CaseData closeCase = hoveredCase.GetTopLeftCase();
      SummonData tempSummon = new SummonData();
      if (closeCase != null)
      {
        tempSummon = SummonManager.Instance.crossSummonList[i];
        tempSummon.transform.position = closeCase.transform.position + tempSummon.transform.position - tempSummon.originPoint.position;
        i++;
      }
      closeCase = hoveredCase.GetTopRightCase();
      if (closeCase != null)
      {
        tempSummon = SummonManager.Instance.crossSummonList[i];
        tempSummon.transform.position = closeCase.transform.position + tempSummon.transform.position - tempSummon.originPoint.position;
        i++;
      }
      closeCase = hoveredCase.GetBottomLeftCase();
      if (closeCase != null)
      {
        tempSummon = SummonManager.Instance.crossSummonList[i];
        tempSummon.transform.position = closeCase.transform.position + tempSummon.transform.position - tempSummon.originPoint.position;
        i++;
      }
      closeCase = hoveredCase.GetBottomRightCase();
      if (closeCase != null)
      {
        tempSummon = SummonManager.Instance.crossSummonList[i];
        tempSummon.transform.position = closeCase.transform.position + tempSummon.transform.position - tempSummon.originPoint.position;
        i++;
      }
      for (int f = i; f < SummonManager.Instance.crossSummonList.Count; f++)
      {
        SummonManager.Instance.crossSummonList[f].transform.position = Vector2.one * 1000;
      }
    }
  }

  public void ApplyEffect(GameObject objAfflicted)
  {
    BeforeFeedbackManager.Instance.HidePrediction();

    if (rotateSummon)
    {
      MenuRotateContexuel.Instance.transform.gameObject.SetActive(true);
      return;
    }

    if (pushValue != 0)
    {
      ApplyPushEffect(objAfflicted);
    }
    if ((damagePR != 0 || damagePA != 0 || damagePM != 0))
    {
      ApplyStatsEffect(objAfflicted);
    }

    if (absorbInvoc)
    {
      ApplyAbsorbInvocEffect(objAfflicted);
    }

    if (explosion)
    {
      ApplyExplosionEffect(objAfflicted);
    }
  }

  public void ApplyPushEffect(GameObject objAfflicted)
  {
    if (objAfflicted.GetComponent<PersoData>() != null)
    {
      PersoData persoAfflicted = objAfflicted.GetComponent<PersoData>();
      CaseData caseAfflicted = persoAfflicted.persoCase;
      if (animatorSpell != null)
        FXManager.Instance.Show(animatorSpell, caseAfflicted.transform.position, SelectionManager.Instance.selectedPersonnage.persoDirection);

      EffectManager.Instance.MultiplePush(objAfflicted, caseAfflicted, pushValue, pushType, pushDirection);
    }

    if (objAfflicted.GetComponent<BallonData>() != null)
    {
      BallonData ballonAfflicted = objAfflicted.GetComponent<BallonData>();
      CaseData caseAfflicted = ballonAfflicted.ballonCase;
      if (animatorSpell != null)
        FXManager.Instance.Show(animatorSpell, caseAfflicted.transform.position, SelectionManager.Instance.selectedPersonnage.persoDirection);

      EffectManager.Instance.MultiplePush(objAfflicted, caseAfflicted, pushValue, pushType, pushDirection);
    }

    if (objAfflicted.GetComponent<SummonData>() != null)
    {
      SummonData summonAfflicted = objAfflicted.GetComponent<SummonData>();
      CaseData caseAfflicted = summonAfflicted.caseActual;
      if (animatorSpell != null)
        FXManager.Instance.Show(animatorSpell, caseAfflicted.transform.position, SelectionManager.Instance.selectedPersonnage.persoDirection);

      EffectManager.Instance.MultiplePush(objAfflicted, caseAfflicted, pushValue, pushType, pushDirection);
    }
  }

  public void ApplyStatsEffect(GameObject objAfflicted)
  {
    PersoData persoAfflicted = objAfflicted.GetComponent<PersoData>();
    SummonData summonAfflicted = objAfflicted.GetComponent<SummonData>();
    BallonData ballonAfflicted = objAfflicted.GetComponent<BallonData>();
    CaseData caseAfflicted = null;

    if (damagePR == 0)
    {
      return;
    }

    if (persoAfflicted)
    {
      if (persoAfflicted.timeStunned > 0)
      {
        return;
      }

      caseAfflicted = persoAfflicted.persoCase;
      if (reverseDamageOnAlly)
      {
        EffectManager.Instance.ChangePr(persoAfflicted, damagePR);
        EffectManager.Instance.ChangePA(damagePA);
        AfterFeedbackManager.Instance.PRText(damagePR, caseAfflicted.gameObject, true);
        GameManager.Instance.manaGlobalActual += damagePA;
        EffectManager.Instance.ChangePm(persoAfflicted, damagePM);
      }
      else
      {
                EffectManager.Instance.ChangePr(persoAfflicted, -damagePR);
        AfterFeedbackManager.Instance.PRText(damagePR, caseAfflicted.gameObject);
        EffectManager.Instance.ChangePADebuff(-damagePA);
                EffectManager.Instance.ChangePmDebuff(persoAfflicted, -damagePM);
      }
    }
    if (summonAfflicted != null)
    {
      if (absorbInvoc)
        return;
      caseAfflicted = summonAfflicted.caseActual;
      if (reverseDamageOnAlly)
      {
        EffectManager.Instance.ChangePr(summonAfflicted, damagePR);
        AfterFeedbackManager.Instance.PRText(damagePR, caseAfflicted.gameObject, true);
      }
      else
      {
        EffectManager.Instance.ChangePr(summonAfflicted, -damagePR);
        AfterFeedbackManager.Instance.PRText(damagePR, caseAfflicted.gameObject);
      }
    }

    if (ballonAfflicted != null)
    {
      caseAfflicted = ballonAfflicted.ballonCase;
    }

    FXManager.Instance.Show(animatorSpell, caseAfflicted.transform.position, SelectionManager.Instance.selectedPersonnage.persoDirection);

        if (animatorSpell3 != null)
            FXManager.Instance.Show(animatorSpell3, caseAfflicted.transform.position, SelectionManager.Instance.selectedPersonnage.persoDirection);

        if (animatorSpell4 != null)
            FXManager.Instance.Show(animatorSpell4, caseAfflicted.transform.position, SelectionManager.Instance.selectedPersonnage.persoDirection);
    }

  void ApplyExplosionEffect(GameObject objAfflicted)
  {
    EffectManager.Instance.doExplosion(HoverManager.Instance.hoveredCase);
  }

  void ApplyAbsorbInvocEffect(GameObject objAfflicted)
  {
        if (!SpellManager.Instance.absorbCasterFXDone)
        FXManager.Instance.Show(animatorSpell, SelectionManager.Instance.selectedCase.transform.position + fxPositionModifier, Direction.NordEst);

        FXManager.Instance.Show(animatorSpell2, objAfflicted.transform.position + fxPositionModifier2, Direction.NordEst);

        SummonData summonAfflicted = objAfflicted.GetComponent<SummonData>();
        
    if (summonAfflicted != null)
    {
            summonAfflicted.transform.position = new Vector3(999, 999, 999);
            summonAfflicted.numberEffectDisapear = 0;
    }
  }
}

