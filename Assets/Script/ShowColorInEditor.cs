/*
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShowColorInEditor : MonoBehaviour
{


  void Update()
  {
    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("case"))
      {
        CaseData objC = obj.GetComponent<CaseData>();
        objC.spriteR = objC.GetComponent<SpriteRenderer>();
        objC.GetComponent<SpriteRenderer>();
        objC.ChangeColorByStatut();
      }
  }
}
#endif
*/