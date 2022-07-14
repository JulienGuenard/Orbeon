using UnityEngine;
using System.Collections;

public class MenuRotateButton : MonoBehaviour
{
  SpriteRenderer spriteR;

  public Color colorEnter;
  public Color colorExit;
  public Color colorDisable;

  public bool collision;

  void Start()
  {
    spriteR = GetComponent<SpriteRenderer>();
  }

  public bool Collision()
  {
    if (spriteR.color == colorDisable || Camera.current == null)
      return false;

    if (Input.mousePosition.x <= Camera.current.WorldToScreenPoint(new Vector3(transform.position.x - spriteR.bounds.size.x / 2, transform.position.y, transform.position.z)).x
        || Input.mousePosition.x >= Camera.current.WorldToScreenPoint(new Vector3(transform.position.x + spriteR.bounds.size.x / 2, transform.position.y, transform.position.z)).x)
      return false;
    if (Input.mousePosition.y <= Camera.current.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y - spriteR.bounds.size.y / 2, transform.position.z)).y
        || Input.mousePosition.y >= Camera.current.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + spriteR.bounds.size.y / 2, transform.position.z)).y)
      return false;

    return true;
  }

  public void MouseExit()
  {
    ChangeColor(colorExit);
  }

  public void MouseOver()
  {
    ChangeColor(colorEnter);
    GameObject Obj = null;
    if (HoverManager.Instance.hoveredBallon)
      Obj = HoverManager.Instance.hoveredBallon.gameObject; // check si l'objet poussé est un ballon ou un personage
    else if (HoverManager.Instance.hoveredPersonnage)
      Obj = HoverManager.Instance.hoveredPersonnage.gameObject; // check si l'objet poussé est un ballon ou un personage
    else
      return; // pas d'objet poussé donc pas de check

    CaseData caseAfflicted = HoverManager.Instance.hoveredCase;

    if (name.Contains("NordEst"))
    {
      PushBehaviour.Instance.PushCheck(Obj, 1, caseAfflicted, PushType.FromTerrain, Direction.NordEst);
    }
    if (name.Contains("NordOuest"))
    {
      PushBehaviour.Instance.PushCheck(Obj, 1, caseAfflicted, PushType.FromTerrain, Direction.NordOuest);
    }
    if (name.Contains("SudEst"))
    {
      PushBehaviour.Instance.PushCheck(Obj, 1, caseAfflicted, PushType.FromTerrain, Direction.SudEst);
    }
    if (name.Contains("SudOuest"))
    {
      PushBehaviour.Instance.PushCheck(Obj, 1, caseAfflicted, PushType.FromTerrain, Direction.SudOuest);
    }
    BeforeFeedbackManager.Instance.PredictDeplacement(Obj, PushBehaviour.Instance.caseFinalShow);
  }

  public void Disable()
  {
    ChangeColor(colorDisable);
  }

  void OnEnable()
  {
    ChangeColor(colorExit);
  }

  void ChangeColor(Color newColor)
  {
    if (spriteR != null)
      spriteR.color = newColor;
  }
}
