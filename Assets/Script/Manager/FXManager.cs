using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>Gère tous les sorts.</summary>
public class FXManager : NetworkBehaviour
{

  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  /// <summary>Montre à quelle portée les personnages vont être projetés avant de le lancer</summary>

  public List<GameObject> listFX;

  public static FXManager Instance;

  Animator animator;
  AnimatorOverrideController animatorOverrideController;
  //    public AnimationClipOverrides(int capacity) : base(capacity) {}

  // ******************** //
  // ** Initialisation ** // Fonctions de départ, non réutilisable
  // ******************** //

  public override void OnStartClient()
  {
    if (Instance == null)
      Instance = this;
    StartCoroutine(waitForInit());
  }

  IEnumerator waitForInit()
  {
    while (!LoadingManager.Instance.isGameReady())
      yield return new WaitForEndOfFrame();
    Init();
  }

  void Init()
  {
    listFX.AddRange(GameObject.FindGameObjectsWithTag("fxFeedback"));
  }

  // *************** //
  // ** Fonctions ** // Fonctions réutilisables ailleurs
  // *************** //

  public void Show(RuntimeAnimatorController animatorPlayed, Vector3 newPos, Direction direction)
  {
    GameObject takenFX = listFX[0];
    listFX.Remove(takenFX);

    takenFX.transform.position = newPos+ new Vector3(0, 0.5f, 0);

        if (animatorPlayed.name == "BallHurtPersonnage")
        {
            StartCoroutine(fxLayerUp(takenFX));
        }

    if (direction == Direction.NordEst)
      takenFX.transform.localRotation = Quaternion.Euler(0, 0, 25); 

    if (direction == Direction.SudEst)
      takenFX.transform.localRotation = Quaternion.Euler(0, 0, -25); 

    if (direction == Direction.SudOuest)
      takenFX.transform.localRotation = Quaternion.Euler(0, 0, -155); 

    if (direction == Direction.NordOuest)
      takenFX.transform.localRotation = Quaternion.Euler(0, 0, -205); 

    takenFX.GetComponent<Animator>().runtimeAnimatorController = animatorPlayed;
    takenFX.GetComponent<Animator>().SetTrigger("spellStart");

    if (animatorPlayed.animationClips.Length != 0)
      {
        StartCoroutine(BackToList(animatorPlayed.animationClips[0].length, takenFX));
      }
  }

    IEnumerator fxLayerUp(GameObject fx)
    {
        fx.GetComponent<SpriteRenderer>().sortingOrder = 1;
        yield return new WaitForSeconds(1f);
        fx.GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

  IEnumerator BackToList(float lenght, GameObject takenFX)
  {
    yield return new WaitForSeconds(lenght);
   // takenFX.GetComponent<Animator>().ResetTrigger("spellStart");
    takenFX.transform.position = new Vector3(999, 999, 999);
    listFX.Add(takenFX);
  }
}