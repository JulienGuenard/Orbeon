using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.Experimental.UIElements;

public class HoverEvent : NetworkBehaviour
{
      string hoveredCase = "null";
      string hoveredPersonnage = "null";
      string hoveredBallon = "null";

    bool isHover = false;

    void Awake()
	{
		GetComponent<PolygonCollider2D>().enabled = false;
	}

	public override void OnStartClient()
	{
		GetComponent<PolygonCollider2D>().enabled = false;
		StartCoroutine(waitForInit());
	}

	IEnumerator waitForInit()
	{
		while (!LoadingManager.Instance.isGameReady())
			yield return new WaitForEndOfFrame();

		yield return new WaitForSeconds(0.01f);
		Init();
	}

	private void Init()
	{
		this.enabled = true;
		GetComponent<PolygonCollider2D>().enabled = true;
	}

	void OnMouseOver()
	{
        if (isHover)
            return;

        isHover =  true;

        if (!SynchroManager.Instance.canPlayTurn())
		{
			return;
		}

		if (!enabled || !LoadingManager.Instance.isGameReady())
			return;

		if (UIManager.Instance.UIIsHovered)
			return;

        if (hoveredCase != this.GetComponent<CaseData>().name)
        {
            hoveredCase = this.GetComponent<CaseData>().name;
        }
		

        if (GetComponent<CaseData>().personnageData != null && GetComponent<CaseData>().personnageData.timeStunned > 0)
			return;

        if (GetComponent<CaseData>().personnageData != null)
            hoveredPersonnage = GetComponent<CaseData>().personnageData.name;
        else
            hoveredPersonnage = "null";

        if (GetComponent<CaseData>().ballon != null)
			hoveredBallon = GetComponent<CaseData>().ballon.name;
        else
            hoveredBallon = "null";

        if (GameManager.Instance.isSoloGame)
		{
			EventManager.Instance.HoverEvent(hoveredCase, hoveredPersonnage, hoveredBallon);
		} else
			RpcFunctions.Instance.CmdSendHoverEvent(hoveredCase, hoveredPersonnage, hoveredBallon);
	}

    void OnMouseExit()
    {
        isHover = false;

        /*
if (enabled && LoadingManager.Instance.isGameReady())
{
RpcFunctions.Instance.CmdSendHoverEvent("null", "null", "null");
}
return;
*/
    }
}
