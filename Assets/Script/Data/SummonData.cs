using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Tout ce qu'il est possible de faire avec une invocation, ainsi que toutes ses données.</summary>
public class SummonData : NetworkBehaviour
{

	// *************** //
	// ** Variables ** // Toutes les variables sans distinctions
	// *************** //

	public bool isStatic;

	public bool isBlockingLineOfSight;

	public bool isTraversable;

	/// <summary>Push (positif) ou pull(negatif)</summary>
	public bool canPush;
	public int pushValue;
	public int pushRange;
	public PushType pushType;
	public Direction pushDirection;

	public Transform originPoint;

	public CaseData caseActual;

	public int actualPointResistance;
	public int maxPointResistance;
	public bool invulnerable;
	public bool makeBallExplosive;
	public bool stopMove;
	public int numberEffectDisapear;
	public Direction summonDirection = Direction.NordEst;

	public int damagePR;
	public int damagePA;
	public int damagePM;
	public bool reverseDamageOnAlly;

	public Sprite P1Sprite;
	public Sprite P2Sprite;

	public Player owner;

	public int limitInvoc;

	public Element element;

    bool absorbed = false;

    Animator animator;

	bool isDeath = false;

	// ******************** //
	// ** Initialisation ** // Fonctions de départ, non réutilisable
	// ******************** //

	void Awake()
	{
		animator = GetComponent<Animator>();
		StartCoroutine(waitForInit());
	}

	IEnumerator waitForInit()
	{
		yield return new WaitForEndOfFrame();
		while (TurnManager.Instance == null)
		{
			yield return null;
		}
		Init();
	}

	public void Init()
	{
        absorbed = false;
    }

	void Update()
	{
		CheckDeath();
	}

	// *************** //
	// ** Fonctions ** // Fonctions réutilisables ailleurs
	// *************** //

	public void SummonInvocEnded()
	{
		TransparencyManager.Instance.CheckCaseTransparency(caseActual);
        GetComponent<BoxCollider2D>().enabled = true;
	}

	/// <summary>Vérifie si l'invocation est censé être toujours vivant ou pas.</summary>
	public void CheckDeath()
	{
		if (((actualPointResistance <= 0 && !invulnerable) || numberEffectDisapear <= 0) && !isDeath)
		{
			isDeath = true;
			SummonManager.Instance.RemoveSummon(this);
			Death();
		}
	}

	public void Death()
	{
        animator.SetTrigger("Disparition");
        animator.enabled = true;
		GetComponent<BoxCollider2D>().enabled = false;
        
    }

	public void DestroyIt()
	{
		Destroy(this.gameObject);
	}

	/// <summary>Applique les effets selon les paramètres de l'invocation.</summary>
	public void ApplyEffect(GameObject objAfflicted)
	{
    PersoData persoAfflicted = objAfflicted.GetComponent<PersoData>();
		CaseData caseAfflicted = null;
		BallonData ballonAfflicted = objAfflicted.GetComponent<BallonData>();

		if (persoAfflicted != null)
		{
			caseAfflicted = persoAfflicted.persoCase;
			if (persoAfflicted.timeStunned > 0)
			{
				return;
			}
			if ((damagePR != 0 || damagePA != 0 || damagePM != 0))
			{
				persoAfflicted = objAfflicted.GetComponent<PersoData>();
				caseAfflicted = persoAfflicted.persoCase;
				if (reverseDamageOnAlly && persoAfflicted.owner == owner)
				{
					damagePA = -damagePA;
					damagePR = -damagePR;
					damagePM = -damagePM;
					AfterFeedbackManager.Instance.PRText(damagePR, objAfflicted, true);
				} else if(persoAfflicted.actualPointResistance > 0)
				{
					AfterFeedbackManager.Instance.PRText(damagePR, objAfflicted);
          EffectManager.Instance.ChangePr(persoAfflicted, -damagePR);
        }
        EffectManager.Instance.ChangePA(-damagePA);
        EffectManager.Instance.ChangePm(persoAfflicted, -damagePM);

			}
      if (stopMove)
      {
        persoAfflicted.moveInterrupted = true;
        persoAfflicted.GetComponent<PushData>().pushValue = 0;
      }

    }
    if (ballonAfflicted != null)
		{
			caseAfflicted = ballonAfflicted.ballonCase;
			if (makeBallExplosive)
			{
				ballonAfflicted.explode();
			}
      if (stopMove)
      {
        ballonAfflicted.isMoving = false;
        ballonAfflicted.GetComponent<PushData>().pushValue = 0;
      }
    }


    if (canPush)
    {
      EffectManager.Instance.MultiplePush(objAfflicted, caseAfflicted, pushValue, pushType, pushDirection);
    }

    numberEffectDisapear--;
	}

	public void ChangeSpriteByPlayer()
	{
		if (GetComponentInChildren<SpriteRenderer>() != null)
		{
			if (P2Sprite != null)
			{
				if (owner == Player.Red)
				{
					GetComponentInChildren<SpriteRenderer>().sprite = P1Sprite;
				}
				if (owner == Player.Blue)
				{
					GetComponentInChildren<SpriteRenderer>().sprite = P2Sprite;
				}
			} else
			{
				if (P1Sprite != null)
				{
					GetComponentInChildren<SpriteRenderer>().sprite = P1Sprite;
				}
			}
		}
	}
}
