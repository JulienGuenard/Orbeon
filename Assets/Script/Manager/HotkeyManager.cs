using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HotkeyManager : NetworkBehaviour
{

  public static HotkeyManager Instance;

  // ******************** //
  // ** Initialisation ** // Fonctions de départ, non réutilisable
  // ******************** //

  public override void OnStartClient()
  {
    if (Instance == null)
      Instance = this;
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Alpha1))
      {
        StartCoroutine(SpellManager.Instance.SpellEnd());
        SpellManager.Instance.SpellButtonClick(0);
      }


    if (Input.GetKeyDown(KeyCode.Alpha2))
      {
        StartCoroutine(SpellManager.Instance.SpellEnd());
        SpellManager.Instance.SpellButtonClick(1);
      }

    if (Input.GetKeyDown(KeyCode.Mouse1))
    {
      SpellManager.Instance.spellSuccess = false;
      StartCoroutine(SpellManager.Instance.SpellEnd());
      ReplacerBalleBehaviour.Instance.replaceEnd();
      MenuRotateContexuel.Instance.gameObject.SetActive(false);
      MenuContextuel.Instance.HideMenu();
    }
  }
}
