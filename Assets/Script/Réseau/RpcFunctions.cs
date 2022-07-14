using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Prototype.NetworkLobby;

public class RpcFunctions : NetworkBehaviour
{


  public static RpcFunctions Instance;

  public int localId;

  public override void OnStartLocalPlayer()
  {
    Instance = this;
    if (isServer)
      {
        LoadingManager.Instance.P1_GameReady = false;
        localId = 0;
      } else
      {
        LoadingManager.Instance.P2_GameReady = false;
        localId = 1;
      }
  }

  [Command]
  public void CmdSpellButtonClick(int IDSpell)
  {
    SpellManager.Instance.RpcSpellButtonClick(IDSpell);
  }

  [Command]
  public void CmdChangeTurn()
  {
    TurnManager.Instance.RpcChangeTurn();
  }

  [Command]
  public void CmdFirstTurn()
  {
    TurnManager.Instance.RpcFirstTurn();
  }

  [Command]
  public void CmdIsAllGameReady()
  {
    LoadingManager.Instance.RpcIsAllGameReady();
  }

  [Command]
  public void CmdSpawnPlayers()
  {
    //  RosterManager.Instance.RpcSpawnPlayers();
  }

  [Command]
  public void CmdSendHoverEvent(string hoveredCase, string hoveredPersonnage, string hoveredBallon)
  {
    SynchroManager.Instance.RpcReceiveHoverEvent(hoveredCase, hoveredPersonnage, hoveredBallon);
    // Au cas où il y a un nouveau hover, la coroutine se reset
    StopAllCoroutines();

    // On lance la coroutine a qui on a donne la fonction de validation
    StartCoroutine(SynchroManager.Instance.WaitForHoverEventValidation(hoveredCase, hoveredPersonnage, hoveredBallon));
  }


  [Command]
  public void CmdSendClickEvent()
  {
    SynchroManager.Instance.RpcReceiveClickEvent();
    // Au cas où il y a un nouveau hover, la coroutine se reset
    StopAllCoroutines();

    // On lance la coroutine a qui on a donne la fonction de validation
    StartCoroutine(SynchroManager.Instance.WaitForClickEventValidation());
  }

  [Command]
  public void CmdMenuContextuelClick(string buttonName)
  {
    EventManager.Instance.RpcMenuContextuelClick(buttonName);
  }

}
