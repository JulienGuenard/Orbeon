using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;
using System.Security.Cryptography;

/// <summary>Tout ce qu'il est possible de faire avec un personnage, ainsi que toutes ses données.</summary>
public class PersoData : NetworkBehaviour
{

	// *************** //
	// ** Variables ** // Toutes les variables sans distinctions
	// *************** //

	public WeightType weightType;
	public Player owner;
	public int actualPointMovement;
	public int maxPointMovement;
	public int actualPointResistance;
	public int maxPointResistance;

  public int pmDebuff;
  public int prDebuff;
  public int poDebuff;
  /// <summary>Modifie la portée du tir.</summary>
  public int shotStrenght;
  public int baseShotStrenght;
  public Direction persoDirection;
	public CaseData persoCase;
	public GameObject originPoint;
	public Sprite faceSprite;
	public Sprite backSprite;

	public SpellData Spell1 = null;
	public SpellData Spell2 = null;

	SpriteRenderer spriteR;
    SpriteRenderer shadowSpriteR;

    bool ShineColorIsRunning = false;

	public bool isTackled = false;
  public bool moveInterrupted = false;
  public bool isPlaced = false;
	public int timeStunned = 0;

	public int pushedDebt;
	public bool isPushed;

	public Animator animator;

	/// <summary>
	/// Tous les sorts qui ont touché le personnage ce tour-ci
	/// </summary>
	public List<SpellData> spellHit = new List<SpellData>();

	// ******************** //
	// ** Initialisation ** // Fonctions de départ, non réutilisable
	// ******************** //

	void Start()
	{
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
		spriteR = GetComponentInChildren<SpriteRenderer>();
        shadowSpriteR = spriteR.GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
		animator.SetBool("Idle", true);

		gameObject.name = spriteR.sprite.name;
		isTackled = false;
		actualPointMovement = maxPointMovement;

		RosterManager.Instance.listHero.Add(this);
		TurnManager.Instance.changeTurnEvent += OnChangeTurn;

		actualPointResistance = maxPointResistance;
		actualPointMovement = maxPointMovement;

		if (owner == Player.Blue)
			ChangeRotation(Direction.NordOuest);
    baseShotStrenght = shotStrenght;
	}

	void OnDisable()
	{
		TurnManager.Instance.changeTurnEvent -= OnChangeTurn;
	}

	private void Update()
	{
if (CheckDeath())
    {
      return;
    }
    if (SelectionManager.Instance.selectedPersonnage != this && HoverManager.Instance.hoveredPersonnage == this)
    {
      if (spriteR == null)
        return;
      if (TurnManager.Instance.currentPlayer == owner)
      {
        spriteR.color = Color.yellow;
      }
      else if (TurnManager.Instance.currentPlayer != owner)
      {
        spriteR.color = Color.grey;
      }
    }
    else
    {
      if (spriteR == null)
        return;
      spriteR.color = Color.white;
    }
  }

	// *************** //
	// ** Events **    // Appel de fonctions au sein de ce script grâce à des events
	// *************** //

	public void OnChangeTurn(object sender, PlayerArgs e)
	{
		spellHit.Clear();
		if (e.currentPlayer == owner)
		{
			ResetPM();

      EffectManager.Instance.ChangePm(this, pmDebuff);
      pmDebuff = 0;
      EffectManager.Instance.ChangePr(this, prDebuff);
      prDebuff = 0;
		}
    else
    if (timeStunned > 0)
    {
      timeStunned--;
      if (timeStunned == 0)
      {
        actualPointResistance = maxPointResistance;
        timeStunned = 0;
        spriteR.color = Color.white;
        persoCase.ChangeStatut(Statut.None, persoCase.statut);
      }
    }

  }

  // *************** //
  // ** Fonctions ** // Fonctions réutilisables ailleurs
  // *************** //

  /// <summary>Vérifie si l'invocation est censé être toujours vivant ou pas.</summary>
  public bool CheckDeath()
	{
		if (actualPointResistance <= 0 && timeStunned == 0)
		{
      spriteR.color = Color.grey;
        timeStunned = 3;
      if (SelectionManager.Instance.selectedPersonnage == this)
			{
				SelectionManager.Instance.Deselect();
        HoverManager.Instance.UnHover();
        persoCase.ChangeStatut(Statut.None, persoCase.statut);
			}
            return true;
		}
		if(actualPointResistance <= 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>Fixe les PM actuel du personnage à ses PM max.</summary>
	public void ResetPM()
	{
		actualPointMovement = maxPointMovement;
	}

  /// <summary>Fixe les PM actuel du personnage à ses PR max.</summary>
  public void ResetPR()
	{
		actualPointResistance = maxPointResistance;
	}

	/// <summary>Change la rotation du sprite du personnage dans la direction donnée.</summary>
	public void ChangeRotation(Direction direction)
	{
		persoDirection = direction;
		switch (persoDirection)
		{
			case Direction.SudOuest:
                spriteR.flipX = true;
                shadowSpriteR.flipX = true;
            //    transform.localRotation = Quaternion.Euler(0, 180, 0);
				animator.SetBool("Back", false);
				animator.SetBool("Front", true);
				break;
			case Direction.NordOuest:
                spriteR.flipX = false;
                shadowSpriteR.flipX = false;
           //     transform.localRotation = Quaternion.Euler(0, 0, 0);
				animator.SetBool("Front", false);
				animator.SetBool("Back", true);
				break;
			case Direction.SudEst:
                spriteR.flipX = false;
                shadowSpriteR.flipX = false;
           //     transform.localRotation = Quaternion.Euler(0, 0, 0);
				animator.SetBool("Back", false);
				animator.SetBool("Front", true);
				break;
			case Direction.NordEst:
                spriteR.flipX = true;
                shadowSpriteR.flipX = true;
            //    transform.localRotation = Quaternion.Euler(0, 180, 0);
				animator.SetBool("Front", false);
				animator.SetBool("Back", true);
				break;
		}
	}

	/// <summary>Change la direction du personnage en direction de la case ciblée.</summary>
	public void RotateTowards(GameObject targetCasePosGMB)
	{
		if (persoCase == null)
			return;

		Vector3 targetCasePos = targetCasePosGMB.transform.position;
		Vector3 originCasePos = persoCase.transform.position;

		if (originCasePos.x > targetCasePos.x && originCasePos.y > targetCasePos.y)
			ChangeRotation(Direction.SudOuest);

		if (originCasePos.x > targetCasePos.x && originCasePos.y < targetCasePos.y)
			ChangeRotation(Direction.NordOuest);

		if (originCasePos.x < targetCasePos.x && originCasePos.y > targetCasePos.y)
			ChangeRotation(Direction.SudEst);

		if (originCasePos.x < targetCasePos.x && originCasePos.y < targetCasePos.y)
			ChangeRotation(Direction.NordEst);
	}

	public void RotateTowardsReversed(GameObject targetCasePosGMB)
	{
		Vector3 targetCasePos = targetCasePosGMB.transform.position;
		Vector3 originCasePos = persoCase.transform.position;

		if (originCasePos.x > targetCasePos.x && originCasePos.y > targetCasePos.y)
			ChangeRotation(Direction.NordEst);

		if (originCasePos.x > targetCasePos.x && originCasePos.y < targetCasePos.y)
			ChangeRotation(Direction.SudEst);

		if (originCasePos.x < targetCasePos.x && originCasePos.y > targetCasePos.y)
			ChangeRotation(Direction.NordOuest);

		if (originCasePos.x < targetCasePos.x && originCasePos.y < targetCasePos.y)
			ChangeRotation(Direction.SudOuest);
	}

	/// <summary>La couleur du sprite oscille entre deux couleurs.</summary>
	public IEnumerator StartShineColor(Color color1, Color color2, float time)
	{
		if (spriteR.color == color1 && spriteR.color == color2)
			StopCoroutine(StartShineColor(color1, color2, time));

		if (ShineColorIsRunning)
			StopCoroutine(StartShineColor(color1, color2, time));

		ShineColorIsRunning = true;

		while (ShineColorIsRunning)
		{
			Color colorx = color1;
			color1 = color2;
			color2 = colorx;
			for (int i = 0; i < 100; i++)
			{
				if (!ShineColorIsRunning)
					break;

				spriteR.color += (color1 - color2) / 100;
				yield return new WaitForSeconds(time + 0.01f);
			}

		}
	}

	/// <summary>Stop la fonction StartShineColor</summary>
	public void StopShineColor()
	{
		ShineColorIsRunning = false;
	}
}
