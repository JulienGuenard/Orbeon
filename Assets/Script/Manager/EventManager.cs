using UnityEngine;
using UnityEngine.Networking;
using System;

public class EventManager : NetworkBehaviour
{

  public static EventManager Instance;

  public static EventHandler<HoverArgs> newHoverEvent;
  public static System.Action newClickEvent;

  public override void OnStartClient()
  {
    if (Instance == null)
      Instance = this;
  }

  public void HoverEvent(string hoveredCaseString, string hoveredPersonnageString, string hoveredBallonString)
  {
    CaseData hoveredCase = null;
    PersoData hoveredPersonnage = null;
    BallonData hoveredBallon = null;

    if (hoveredCaseString != "null")
      hoveredCase = GameObject.Find(hoveredCaseString).GetComponent<CaseData>();
    if (hoveredPersonnageString != "null" && hoveredCase != null)
      hoveredPersonnage = hoveredCase.personnageData;
    if (hoveredBallonString != "null" && hoveredCase != null)
      hoveredBallon = hoveredCase.ballon;

    if (hoveredPersonnage == null && hoveredPersonnageString != "null")
      hoveredPersonnage = GameObject.Find(hoveredPersonnageString).GetComponent<PersoData>();
    if (hoveredBallon == null && hoveredBallonString != "null")
      hoveredBallon = GameObject.Find(hoveredBallonString).GetComponent<BallonData>();

    newHoverEvent(this, new HoverArgs(hoveredCase, hoveredPersonnage, hoveredBallon));
  }

  [ClientRpc]
  public void RpcMenuContextuelClick(string buttonName)
  {
    switch (buttonName)
      {
      case ("MenuContextuelReplacer"):
        if (SelectionManager.Instance.selectedPersonnage.GetComponent<PersoData>().actualPointMovement < 1)
          {
            MenuContextuel.Instance.HideMenu();
            return;
          }
      SelectionManager.Instance.selectedPersonnage.GetComponent<PersoData>().actualPointMovement--;
        ReplacerBalleBehaviour.Instance.ReplacerBalle();
        MenuContextuel.Instance.HideMenu();
        break;
      case ("MenuContextuelTirer"):
      ManaManager.Instance.ChangeActualMana(GameManager.Instance.currentPlayer, 2);
      SelectionManager.Instance.selectedBallon.GetComponent<BallonData>().StartCoroutine("Move");
        MenuContextuel.Instance.HideMenu();
        break;
      case ("Nothing"):
        MenuContextuel.Instance.HideMenu();
        break;
      }
  }

}
