using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TransparencyManager : NetworkBehaviour
{
	// *************** //
	// ** Variables ** // Toutes les variables sans distinctions
	// *************** //

	[Range(0, 1)] [Tooltip("Niveau de transparence")] public float alpha;
	[HideInInspector] static public TransparencyManager Instance;

	// ******************** //
	// ** Initialisation ** // Fonctions de départ, non réutilisable
	// ******************** //

	public override void OnStartClient()
	{
		if (Instance == null)
			Instance = this;
	}

	// *************** //
	// ** Fonctions ** // Fonctions réutilisables ailleurs
	// *************** //

	public void CheckCaseTransparency(CaseData Case, bool doRecursive = true)
	{ // Check s'il y a un personnage ou un ballon au dessus ou en dessous de la case ciblée pour détecter s'il doit faire une transparence ou non.

		// GET CASE HAUT ET GET CASE BAS
		CaseData upperCase = null;
		CaseData lowerCase = null;
		if (Case != null)
		{
			upperCase = Case.GetTopCase();
			lowerCase = Case.GetBottomCase();
		}
      
		if (upperCase != null)
		{
			if (upperCase.personnageData != null || upperCase.summonData != null)
			{
				if (Case.personnageData != null)
					ApplyTransparency(Case.personnageData.gameObject);

				if (Case.summonData != null)
					ApplyTransparency(Case.summonData.gameObject);
			} else if (upperCase != null && upperCase.personnageData == null || upperCase.summonData == null)
			{
				if (Case.personnageData != null)
					ApplyOpacity(Case.personnageData.gameObject);

				if (Case.summonData != null)
					ApplyOpacity(Case.summonData.gameObject);
			}
		}

		if (doRecursive)
		{
			CheckCaseTransparency(upperCase, false);
			CheckCaseTransparency(lowerCase, false);
		}
	}

	public void ApplyTransparency(GameObject obj)
	{ // Applique la transparence du TransparencyBehaviour sur le personnage.
		SpriteRenderer CaseSpriteR = obj.GetComponentInChildren<SpriteRenderer>();
		if (obj.GetComponent<Animator>() != null)
			obj.GetComponent<Animator>().enabled = false;
		
		Color transparency = new Color(CaseSpriteR.color.r, CaseSpriteR.color.g, CaseSpriteR.color.b, alpha);
		obj.GetComponentInChildren<SpriteRenderer>().color = transparency;
	}

	public void ApplyOpacity(GameObject obj)
	{ // Annule la transparence du personnage.
		SpriteRenderer CaseSpriteR = obj.GetComponentInChildren<SpriteRenderer>();
		if (obj.GetComponent<Animator>() != null)
			obj.GetComponent<Animator>().enabled = false;
		
		Color transparency = new Color(CaseSpriteR.color.r, CaseSpriteR.color.g, CaseSpriteR.color.b, 1);
		obj.GetComponentInChildren<SpriteRenderer>().color = transparency;
	}
}
