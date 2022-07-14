using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.ComponentModel.Design;

public class RosterManager : NetworkBehaviour
{

  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  public List<PersoData> listHeroJ1;
  public List<PersoData> listHeroJ2;
  [ReadOnly] public List<PersoData> listHero;
  [ReadOnly] public List<PersoData> listHeroPlaced;

  [HideInInspector] public static RosterManager Instance;

  public PersoData persoCreated;

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

  public void Init()
  {
    if (isServer)
      {
        foreach (PersoData obj in listHeroJ1)
          {
            GameObject persoCreated = (GameObject)Instantiate(obj.gameObject, new Vector3(999, 999, 999), Quaternion.identity);
            persoCreated.GetComponent<PersoData>().owner = Player.Red;
            NetworkServer.Spawn(persoCreated);
          }
        foreach (PersoData obj in listHeroJ2)
          {
            GameObject persoCreated = (GameObject)Instantiate(obj.gameObject, new Vector3(999, 999, 999), Quaternion.identity);
            persoCreated.GetComponent<PersoData>().owner = Player.Blue;
            NetworkServer.Spawn(persoCreated);
          }
      }
    listHeroJ1.Clear();
    listHeroJ2.Clear();
  }
}