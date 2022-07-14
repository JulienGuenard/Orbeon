using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class DebugGetId : NetworkBehaviour
{
  public static DebugGetId Instance;
  public String text;

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
    //  GetComponent<Text>().text = RpcFunctions.Instance.localId.ToString();
  }


  private void Update()
  {
    Instance = this;
    // GetComponent<Text>().text = RpcFunctions.Instance.localId.ToString();
    GetComponent<Text>().text = text;
  }
}
