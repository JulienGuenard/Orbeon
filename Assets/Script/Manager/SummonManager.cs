using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SummonManager : NetworkBehaviour
{

  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  public static SummonManager Instance;

  public SummonData lastSummonInstancied;
  public List<SummonData> crossSummonList = new List<SummonData>();

  public List<SummonData> summonAirRedList;
  public List<SummonData> summonAirBlueList;
  public List<SummonData> summonEarthRedList;
  public List<SummonData> summonEarthBlueList;
  public List<SummonData> summonFireRedList;
  public List<SummonData> summonFireBlueList;
  public List<SummonData> summonWaterRedList;
  public List<SummonData> summonWaterBlueList;

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
    TurnManager.Instance.changeTurnEvent += ResetSummonLimit;
    // SpellManager.Instance.changeTurnEvent += CheckSummonLimitReached();
  }
  /*
  void OnDisable()
  {
    if (LoadingManager.Instance != null && LoadingManager.Instance.isGameReady())
      {
        SpellManager.Instance.changeTurnEvent -= OnChangeTurn;
      }
  }*/

  void Update()
  {
    return; // pas utilisé
    CheckSummonLimitReached();
  }

  void ResetSummonLimit(object sender, PlayerArgs e)
  { // Lorsqu'un joueur termine son tour
    summonAirRedList.Clear();
    summonAirBlueList.Clear();
    summonEarthRedList.Clear();
    summonEarthBlueList.Clear();
    summonFireRedList.Clear();
    summonFireBlueList.Clear();
    summonWaterRedList.Clear();
    summonWaterBlueList.Clear();
  }

  void CheckSummonLimitReached()
  {
    if (summonAirRedList.Count != 0)
      {
        if (summonAirRedList.Count > summonAirRedList[0].limitInvoc)
          {
            if (summonAirRedList[0] == null)
              summonAirRedList.RemoveAt(0);

            summonAirRedList[0].GetComponent<SummonData>().Death();
            summonAirRedList.RemoveAt(0);
          }
      }

    if (summonAirBlueList.Count != 0)
      {
        if (summonAirBlueList.Count > summonAirBlueList[0].limitInvoc)
          {
            if (summonAirBlueList[0] == null)
              summonAirBlueList.RemoveAt(0);

            summonAirBlueList[0].GetComponent<SummonData>().Death();
            summonAirBlueList.RemoveAt(0);
          }
      }

    if (summonEarthRedList.Count != 0)
      {
        if (summonEarthRedList.Count > summonEarthRedList[0].limitInvoc)
          {
            if (summonEarthRedList[0] == null)
              summonEarthRedList.RemoveAt(0);

            summonEarthRedList[0].GetComponent<SummonData>().Death();
            summonEarthRedList.RemoveAt(0);
          }
      }

    if (summonEarthBlueList.Count != 0)
      {
        if (summonEarthBlueList.Count > summonEarthBlueList[0].limitInvoc)
          {
            if (summonEarthBlueList[0] == null)
              summonEarthBlueList.RemoveAt(0);

            summonEarthBlueList[0].GetComponent<SummonData>().Death();
            summonEarthBlueList.RemoveAt(0);
          }
      }

    if (summonFireRedList.Count != 0)
      {
        if (summonFireRedList.Count > summonFireRedList[0].limitInvoc)
          {
            if (summonFireRedList[0] == null)
              summonFireRedList.RemoveAt(0);

            summonFireRedList[0].GetComponent<SummonData>().Death();
            summonFireRedList.RemoveAt(0);
          }
      }

    if (summonFireBlueList.Count != 0)
      {
        if (summonFireBlueList.Count > summonFireBlueList[0].limitInvoc)
          {
            if (summonFireBlueList[0] == null)
              summonFireBlueList.RemoveAt(0);

            summonFireBlueList[0].GetComponent<SummonData>().Death();
            summonFireBlueList.RemoveAt(0);
          }
      }

    if (summonWaterRedList.Count != 0)
      {
        if (summonWaterRedList.Count > summonWaterRedList[0].limitInvoc)
          {
            if (summonWaterRedList[0] == null)
              summonWaterRedList.RemoveAt(0);

            summonWaterRedList[0].GetComponent<SummonData>().Death();
            summonWaterRedList.RemoveAt(0);
          }
      }

    if (summonWaterBlueList.Count != 0)
      {
        if (summonWaterBlueList.Count > summonWaterBlueList[0].limitInvoc)
          {
            if (summonWaterBlueList[0] == null)
              summonWaterBlueList.RemoveAt(0);

            summonWaterBlueList[0].GetComponent<SummonData>().Death();
            summonWaterBlueList.RemoveAt(0);

          }
      }
  }

  public void AddSummon(SummonData summon)
  {
    SpellData selectedSpell = SpellManager.Instance.selectedSpell;
    if (summon.owner == Player.Red)
      {
        if (summon.element == Element.Air)
          {
            SummonManager.Instance.summonAirRedList.Add(summon);
          }
        if (summon.element == Element.Eau)
          {
            SummonManager.Instance.summonWaterRedList.Add(summon);
          }
        if (summon.element == Element.Feu)
          {
            SummonManager.Instance.summonFireRedList.Add(summon);
          }
        if (summon.element == Element.Terre)
          {
            SummonManager.Instance.summonEarthRedList.Add(summon);
          }
      }
    if (summon.owner == Player.Blue)
      {
        if (summon.element == Element.Air)
          {
            SummonManager.Instance.summonAirBlueList.Add(summon);
          }
        if (summon.element == Element.Eau)
          {
            SummonManager.Instance.summonWaterBlueList.Add(summon);
          }
        if (summon.element == Element.Feu)
          {
            SummonManager.Instance.summonFireBlueList.Add(summon);
          }
        if (summon.element == Element.Terre)
          {
            SummonManager.Instance.summonEarthBlueList.Add(summon);
          }
      }
  }

  public void RemoveSummon(SummonData summon)
  {
    SpellData selectedSpell = SpellManager.Instance.selectedSpell;
    if (summon.owner == Player.Red)
      {
        if (summon.element == Element.Air)
          {
            SummonManager.Instance.summonAirRedList.Remove(summon);
          }
        if (summon.element == Element.Eau)
          {
            SummonManager.Instance.summonWaterRedList.Remove(summon);
          }
        if (summon.element == Element.Feu)
          {
            SummonManager.Instance.summonFireRedList.Remove(summon);
          }
        if (summon.element == Element.Terre)
          {
            SummonManager.Instance.summonEarthRedList.Remove(summon);
          }
      }
    if (summon.owner == Player.Blue)
      {
        if (summon.element == Element.Air)
          {
            SummonManager.Instance.summonAirBlueList.Remove(summon);
          }
        if (summon.element == Element.Eau)
          {
            SummonManager.Instance.summonWaterBlueList.Remove(summon);
          }
        if (summon.element == Element.Feu)
          {
            SummonManager.Instance.summonFireBlueList.Remove(summon);
          }
        if (summon.element == Element.Terre)
          {
            SummonManager.Instance.summonEarthBlueList.Remove(summon);
          }
      }
  }
}
