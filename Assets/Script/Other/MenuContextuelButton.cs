using System.Collections;
using UnityEngine;

public class MenuContextuelButton : MonoBehaviour
{

	SpriteRenderer spriteR;

	public Color colorEnter;
	public Color colorExit;
	public Color colorDisable;

	public bool collision = false;

	GameObject menuContextuelReplacerTooltip;
	GameObject menuContextuelTirerTooltip;

	void Start()
	{
		menuContextuelReplacerTooltip = GameObject.Find("MenuContextuelReplacerTooltip");
		menuContextuelTirerTooltip = GameObject.Find("MenuContextuelTirerTooltip");
		spriteR = GetComponent<SpriteRenderer>();

		switch (name)
		{
			case "MenuContextuelReplacer":
				menuContextuelReplacerTooltip.SetActive(false);
				break;
			case "MenuContextuelTirer":
				menuContextuelTirerTooltip.SetActive(false);
				break;
		}
	}

	public bool Collision()
	{
		if (spriteR.color == colorDisable)
		{
			return false;
		}
		if (Camera.current == null)
			return false;
		if (Input.mousePosition.x <= Camera.current.WorldToScreenPoint(new Vector3(transform.position.x - spriteR.bounds.size.x / 2, transform.position.y, transform.position.z)).x
		    || Input.mousePosition.x >= Camera.current.WorldToScreenPoint(new Vector3(transform.position.x + spriteR.bounds.size.x / 2, transform.position.y, transform.position.z)).x)
			return false;
		if (Input.mousePosition.y <= Camera.current.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y - spriteR.bounds.size.y / 2, transform.position.z)).y
		    || Input.mousePosition.y <= Camera.current.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y - spriteR.bounds.size.y / 2, transform.position.z)).y)
			return false;
		return true;
	}

	public void MouseExit()
	{
		ChangeColor(colorExit);
		switch (name)
		{
			case "MenuContextuelReplacer":
				menuContextuelReplacerTooltip.SetActive(false);
				break;
			case "MenuContextuelTirer":
				ManaManager.Instance.Desactived();
				ManaManager.Instance.SpellButtonFeedbackOFF();
				if (GameObject.Find("Ballon") != null)
					GameObject.Find("Ballon").GetComponent<BallonData>().ShotDeprevisualisation();
				
				menuContextuelTirerTooltip.SetActive(false);
				break;
		}
	}

    public void MouseOver()
	{
		ChangeColor(colorEnter);
		switch (name)
		{
			case "MenuContextuelReplacer":
				menuContextuelReplacerTooltip.SetActive(true);
				break;
			case "MenuContextuelTirer":
				ManaManager.Instance.Actived();
				ManaManager.Instance.SpellButtonFeedbackON(2);
				GameObject.Find("Ballon").GetComponent<BallonData>().ShotPrevisualisation();
				menuContextuelTirerTooltip.SetActive(true);
				break;
		}
	}

	public void Disable()
	{
		ChangeColor(colorDisable);
	}

  private void Update()
  {
    if (Collision())
    {
      if (!collision)
      {
        collision = true;
        MouseOver();
      }
    }
    else
    {
      if (collision)
      {
        collision = false;
        MouseExit();
      }
    }
    if (MenuContextuel.Instance.activated)
    {
      if (name == "MenuContextuelReplacer" && SelectionManager.Instance.selectedPersonnage.actualPointMovement == 0)
      {
        Disable();
      }
    }

    if (MenuContextuel.Instance.activated)
    {
      if (name == "MenuContextuelTirer" && ManaManager.Instance.manaActuelRed < 2 && SelectionManager.Instance.selectedPersonnage.owner == Player.Red)
      {
        Disable();
      }
    }

    if (MenuContextuel.Instance.activated)
    {
      if (name == "MenuContextuelTirer" && ManaManager.Instance.manaActuelBlue < 2 && SelectionManager.Instance.selectedPersonnage.owner == Player.Blue)
      {
        Disable();
      }
    }
  }

    public void Enable()
	{
		ChangeColor(colorExit);
		StartCoroutine(DebugCollider());
	}

	IEnumerator DebugCollider()
	{
		yield return new WaitForSeconds(0.05f);
	}

	void ChangeColor(Color newColor)
	{
		if (spriteR != null)
			spriteR.color = newColor;
	}
}
