using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Reflection;

public class infoPersosPortrait : NetworkBehaviour
{

  public PortraitInteractive MainPortrait;
  public PortraitInteractive SubPortrait3;
  public PortraitInteractive SubPortrait2;
  public PortraitInteractive SubPortrait1;

  public Sprite Portrait_terre_rouge;
  public Sprite Portrait_feu_rouge;
  public Sprite Portrait_air_rouge;
  public Sprite Portrait_eau_rouge;

  public Sprite Portrait_terre_bleu;
  public Sprite Portrait_feu_bleu;
  public Sprite Portrait_air_bleu;
  public Sprite Portrait_eau_bleu;

  GameObject StatsRed;
  GameObject StatsBlue;

  PersoData lastPerso = null;

  bool isShowingStats = false;

  PortraitInteractive perso;

  public void DeselectPerso()
  {
    SubPortrait3.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
    SubPortrait2.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
    SubPortrait1.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
    MainPortrait.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
  }

  public void SelectPerso(PersoData newPerso)
  {
    if (StatsRed == null)
      StatsRed = GameObject.Find("StatsRed");

    if (StatsBlue == null)
      StatsBlue = GameObject.Find("StatsBlue");

    if (lastPerso != null && newPerso != null)
    if (lastPerso == newPerso)
      return;

    lastPerso = newPerso;

    SubPortrait3.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
    SubPortrait2.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
    SubPortrait1.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
    MainPortrait.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

        if (newPerso == null)
            return;

            PortraitInteractive portrait = null;

    if (newPerso == MainPortrait.newHoveredPersonnage)
      {
        portrait = MainPortrait;
        portrait.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
      }
    if (newPerso == SubPortrait1.newHoveredPersonnage)
      {
        portrait = SubPortrait1;
        portrait.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
      }
    if (newPerso == SubPortrait2.newHoveredPersonnage)
      {
        portrait = SubPortrait2;
        portrait.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
      }

    if (newPerso == SubPortrait3.newHoveredPersonnage)
      {
        portrait = SubPortrait3;
        portrait.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
      }
    
    if (portrait != null)
      {
        isShowingStats = false;
        StartCoroutine(ShowStats(portrait));
      }
  }

  public void SetupChangePlayerIcons(Player owner, int turnNumber)
  {
    foreach (PersoData perso in RosterManager.Instance.listHero)
      {
        string persoName = perso.gameObject.name;

        if (owner == Player.Red)
          {
            if (persoName == "Terre_rouge")
              {
                MainPortrait.setPortraitData(Portrait_terre_rouge, Color.white, perso);
              }
            if (persoName == "Air_rouge")
              {
                SubPortrait1.setPortraitData(Portrait_air_rouge, Color.white, perso);
              }
            if (persoName == "Feu_rouge")
              {
                SubPortrait2.setPortraitData(Portrait_feu_rouge, Color.white, perso);
              }
            if (persoName == "Eau_rouge")
              {
                SubPortrait3.setPortraitData(Portrait_eau_rouge, Color.white, perso);
              }
          }
        if (owner == Player.Blue)
          {
            if (persoName == "Terre_bleu")
              {
                MainPortrait.setPortraitData(Portrait_terre_bleu, Color.white, perso);
              }
            if (persoName == "Air_bleu")
              {
                SubPortrait1.setPortraitData(Portrait_air_bleu, Color.white, perso);
              }
            if (persoName == "Feu_bleu")
              {
                SubPortrait2.setPortraitData(Portrait_feu_bleu, Color.white, perso);
              }
            if (persoName == "Eau_bleu")
              {
                SubPortrait3.setPortraitData(Portrait_eau_bleu, Color.white, perso);
              }
          }
      }
  }

  public void UnGrayAllPortraits()
  {
    MainPortrait.UnGrayPortrait();
    SubPortrait1.UnGrayPortrait();
    SubPortrait2.UnGrayPortrait();
    SubPortrait3.UnGrayPortrait();
  }

  public void GrayPortraitPerso(PersoData perso)
  {
    if (MainPortrait.newHoveredPersonnage == perso)
      {
        MainPortrait.GrayPortrait();
      }
    if (SubPortrait1.newHoveredPersonnage == perso)
      {
        SubPortrait1.GrayPortrait();
      }
    if (SubPortrait2.newHoveredPersonnage == perso)
      {
        SubPortrait2.GrayPortrait();
      }
    if (SubPortrait3.newHoveredPersonnage == perso)
      {
        SubPortrait3.GrayPortrait();
      }
  }

  public void UnGrayPortraitPerso(PersoData perso)
  {
    if (MainPortrait.newHoveredPersonnage == perso)
      {
        MainPortrait.UnGrayPortrait();
      }
    if (SubPortrait1.newHoveredPersonnage == perso)
      {
        SubPortrait1.UnGrayPortrait();
      }
    if (SubPortrait2.newHoveredPersonnage == perso)
      {
        SubPortrait2.UnGrayPortrait();
      }
    if (SubPortrait3.newHoveredPersonnage == perso)
      {
        SubPortrait3.UnGrayPortrait();
      }
  }

  new public IEnumerator ShowStats(PortraitInteractive perso)
  {
    isShowingStats = true;
    float xMin = 0.8f;
    float xMax = 2;
    yield return new WaitForSecondsRealtime(0.02f);
    if (TurnManager.Instance.currentPlayer == Player.Red)
      {
        if (StatsRed.transform.parent == perso.transform.parent)
          isShowingStats = false;

        StatsRed.transform.parent = perso.transform;
        StatsRed.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        StatsRed.GetComponent<RectTransform>().anchorMin = new Vector2(xMin - 0.8f, StatsRed.GetComponent<RectTransform>().anchorMin.y);
        StatsRed.GetComponent<RectTransform>().anchorMax = new Vector2(xMax - 0.8f, StatsRed.GetComponent<RectTransform>().anchorMax.y);
        StatsRed.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        StatsRed.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        float speed = 0.35f;
        for (int i = 0; StatsRed.GetComponent<RectTransform>().anchorMax.x < xMax; i++)
          {
            if (!isShowingStats)
              {
                break;
              }
            yield return new WaitForSeconds(0.01f);
            speed = 0.002f + speed / 1.5f;
            StatsRed.GetComponent<RectTransform>().anchorMin += new Vector2(speed, 0);
            StatsRed.GetComponent<RectTransform>().anchorMax += new Vector2(speed, 0);
          }
      }

    if (TurnManager.Instance.currentPlayer == Player.Blue)
      {
        if (StatsBlue.transform.parent == perso.transform.parent)
          isShowingStats = false;

        StatsBlue.transform.parent = perso.transform;
        StatsBlue.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
        StatsBlue.GetComponent<RectTransform>().anchorMin = new Vector2(xMin - 0.8f, StatsBlue.GetComponent<RectTransform>().anchorMin.y);
        StatsBlue.GetComponent<RectTransform>().anchorMax = new Vector2(xMax - 0.8f, StatsBlue.GetComponent<RectTransform>().anchorMax.y);
        StatsBlue.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        StatsBlue.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        float speed = 0.35f;
        for (int i = 0; StatsBlue.GetComponent<RectTransform>().anchorMax.x < xMax; i++)
          {
            if (!isShowingStats)
              {
                break;
              }
            yield return new WaitForSeconds(0.01f);
            speed = 0.002f + speed / 1.5f;
            StatsBlue.GetComponent<RectTransform>().anchorMin += new Vector2(speed, 0);
            StatsBlue.GetComponent<RectTransform>().anchorMax += new Vector2(speed, 0);
          }
      }
  }
}
