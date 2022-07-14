using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Toutes les couleurs sont stockés ici, pour une case, un personnage ou tout autre chose.</summary>
[ExecuteInEditMode]
public class ColorManager : NetworkBehaviour
{

  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  [Header("  Couleur des cases")]
  public Color hoverColor;
  public Color hoverStrongColor;
  public Color caseColor;
  public Color enemyColor;
  public Color moveColor;
  public Color actionPreColor;
  public Color actionColor;
  public Color placementZoneRed;
  public Color placementZoneBlue;
  public Color isMovingColor;
  public Color selectedColor;
  public Color goalColor;
  public Color atRange;
  public Color canTarget;
  public Color atAoE;
  public Color atPush;
  public Color canMovePrevisu;

  [Header("  Couleur des personnages")]
  public Color punchedPersonnageColor;

  [HideInInspector] public static ColorManager Instance;

  // ******************** //
  // ** Initialisation ** // Fonctions de départ, non réutilisable
  // ******************** //

  void Update()
  {
    if (Instance == null)
      Instance = this;
  }

  public override void OnStartClient()
  {
    if (Instance == null)
      Instance = this;
  }
}
