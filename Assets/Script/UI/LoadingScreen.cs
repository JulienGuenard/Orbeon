using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadingScreen : NetworkBehaviour
{

  public override void OnStartClient()
  {
    GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
    GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
    StartCoroutine(waitForInit());
  }

  IEnumerator waitForInit()
  {
    while (!LoadingManager.Instance.isGameReady())
      yield return new WaitForEndOfFrame();

    this.gameObject.SetActive(false);
  }
}
