using System;
using UnityEngine;

public class HoverArgs : EventArgs {

	public CaseData hoveredCase;
	public PersoData hoveredPersonnage;
	public BallonData hoveredBallon;

	public HoverArgs(CaseData hoveredCase, PersoData hoveredPersonnage, BallonData hoveredBallon)
	{
		this.hoveredCase = hoveredCase;
		this.hoveredPersonnage = hoveredPersonnage;
		this.hoveredBallon = hoveredBallon;
	}
}