using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{

  public static SpellData tooltipObj;
  public GameObject tooltipText;
  Text UIText;

  void Awake()
  {
    UIText = tooltipText.GetComponent<Text>();
  }

  void OnEnable()
  {
    if (tooltipObj == null)
      return;

    UIText.text = tooltipObj.tooltipTitle + "\n" + tooltipObj.tooltipRange + "\n" + tooltipObj.tooltipCost + "\n" + tooltipObj.tooltipEffect;
    GetComponent<RectTransform>().anchorMax = 
          new Vector2(GetComponent<RectTransform>().anchorMax.x, GetComponent<RectTransform>().anchorMin.y + UIText.text.Split(new char[] {
      '\n',
      '\r'
    }).GetLength(0) * 0.03f);
  }
}
