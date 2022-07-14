using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class infoPersoStats : NetworkBehaviour
{
  public GameObject PrRed;
  public GameObject PmRed;
  public GameObject PoRed;
 
  public GameObject PrBlue;
  public GameObject PmBlue;
  public GameObject PoBlue;

  // Use this for initialization
  public void updatePr(int pr)
  {
    if (!SelectionManager.Instance.selectedPersonnage)
      return;

    if (SelectionManager.Instance.selectedPersonnage.owner == Player.Red)
      PrRed.GetComponent<Text>().text = pr + " ";
    if (SelectionManager.Instance.selectedPersonnage.owner == Player.Blue)
      PrBlue.GetComponent<Text>().text = pr + " ";
  }
  // Use this for initialization
  public void updatePm(int pm)
  {
    if (!SelectionManager.Instance.selectedPersonnage)
      return;
    if (SelectionManager.Instance.selectedPersonnage.owner == Player.Red)
      PmRed.GetComponent<Text>().text = pm + " ";
    if (SelectionManager.Instance.selectedPersonnage.owner == Player.Blue)
      PmBlue.GetComponent<Text>().text = pm + " ";
  }
  // Use this for initialization
  public void updatePo(int po)
  {
    if (!SelectionManager.Instance.selectedPersonnage)
      return;

    if (SelectionManager.Instance.selectedPersonnage.owner == Player.Red)
      PoRed.GetComponent<Text>().text = po + " ";
    if (SelectionManager.Instance.selectedPersonnage.owner == Player.Blue)
      PoBlue.GetComponent<Text>().text = po + " ";
  }
}
