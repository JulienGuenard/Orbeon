using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MenuRotateContexuel : NetworkBehaviour
{

  public static MenuRotateContexuel Instance;

  public List<MenuRotateButton> listButtons = new List<MenuRotateButton>();

  bool activated = false;

  void OnEnable()
  {
    if (Instance == null)
    {
      Instance = this;
      gameObject.SetActive(false);
      return;
    }
    transform.position = HoverManager.Instance.hoveredCase.transform.position - new Vector3(-0.1422f, 0.15f) * 3.5f;
    CaseManager.Instance.DisableAllColliders();
    StartCoroutine(waitUntilUp());
    BeforeFeedbackManager.Instance.HidePrediction();
  }

  void OnDisable()
  {
    if (LoadingManager.Instance != null && LoadingManager.Instance.isGameReady())
    {
      CaseManager.Instance.EnableAllColliders();
    }
    activated = false;
    transform.position = Vector3.one * 500;
    if (SpellManager.Instance != null)
      SpellManager.Instance.StartCoroutine(SpellManager.Instance.SpellEnd());
    if(BeforeFeedbackManager.Instance)
    BeforeFeedbackManager.Instance.HidePrediction();
  }

  private void Update()
  {
    if (activated == false)
    {
      return;
    }

/*    if (GameManager.Instance.actualAction != PersoAction.isCasting)
    {
      SpellManager.Instance.spellSuccess = false;
      gameObject.SetActive(false);
      return;
    }*/ 
    foreach (MenuRotateButton button in listButtons)
    {
      if (button.Collision())
      {
        button.MouseOver();
        if (Input.GetMouseButton(0))
        {
          button.MouseExit();

          if (button.name.Contains("NordEst"))
          {
            SummonManager.Instance.lastSummonInstancied.gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = button.GetComponent<SpriteRenderer>().sprite;
            EffectManager.Instance.Rotate(SummonManager.Instance.lastSummonInstancied.gameObject, Direction.NordEst);
          }
          if (button.name.Contains("NordOuest"))
          {
            SummonManager.Instance.lastSummonInstancied.gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = button.GetComponent<SpriteRenderer>().sprite;
            EffectManager.Instance.Rotate(SummonManager.Instance.lastSummonInstancied.gameObject, Direction.NordOuest);
          }
          if (button.name.Contains("SudEst"))
          {
            SummonManager.Instance.lastSummonInstancied.gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = button.GetComponent<SpriteRenderer>().sprite;
            EffectManager.Instance.Rotate(SummonManager.Instance.lastSummonInstancied.gameObject, Direction.SudEst);
          }
          if (button.name.Contains("SudOuest"))
          {
            SummonManager.Instance.lastSummonInstancied.gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = button.GetComponent<SpriteRenderer>().sprite;
            EffectManager.Instance.Rotate(SummonManager.Instance.lastSummonInstancied.gameObject, Direction.SudOuest);
          }
          SpellManager.Instance.SummonInvoc();
          SpellManager.Instance.SpellCaseClick();
          gameObject.SetActive(false);
        }

      }
      else
        button.MouseExit();
    }
  }
  IEnumerator waitUntilUp()
  {
    yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
    activated = true;
  }
}
