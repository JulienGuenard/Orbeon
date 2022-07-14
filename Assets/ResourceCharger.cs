using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceCharger : NetworkBehaviour
{
  public Object[] allSprite;

  public override void OnStartClient()
  {
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
    SpriteRenderer rend = GetComponent<SpriteRenderer>();
    allSprite = Resources.LoadAll("Sprite", typeof(Sprite));
    foreach (Sprite sprite in allSprite)
      {
        rend.sprite = sprite;
      }
    StartCoroutine(GoDestroy());
  }

  IEnumerator GoDestroy()
  {
    yield return new WaitForSeconds(0.01f);
    Destroy(this.gameObject);
  }
}
