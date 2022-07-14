using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SummonBehaviour : NetworkBehaviour
{
  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  public static SummonBehaviour Instance;

  // ******************** //
  // ** Initialisation ** // Fonctions de départ, non réutilisable
  // ******************** //

  public override void OnStartClient()
  {
    if (Instance == null)
      Instance = this;
  }
}
