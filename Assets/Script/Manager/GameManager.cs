using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>Gère des variables globales tel que le type de phase, le joueur en train de jouer, c'est censé être le script le plus parent de tous les scripts !</summary>
public class GameManager : NetworkBehaviour
{

  // *************** //
  // ** Variables ** // Toutes les variables sans distinctions
  // *************** //

  [Tooltip("Joueur qui joue (Red, Blue, Neutral)")]
  public Player currentPlayer;
  [Tooltip("Joueur qui joue (Red=0, Blue=1, Neutral=2)")]
  public int indexPlayer;
  [Tooltip("Nombre de joueurs humains")]
  public int numberPlayer = -1;
  [Tooltip("Phase actuel (Placement, Deplacement)")]
  public Phase currentPhase;
  [Tooltip("Quel est l'action qu'entreprend le joueur (isMoving, isReplacingBall, isShoting, isIdle)")]
  public PersoAction actualAction;
  public int manaGlobalMax = 20;
  public int manaGlobalActual = 20;
  [SyncVar] public bool isSoloGame;

    public bool canPlayTurn = true;

  public static GameManager Instance;

  public int paDebuff;

  // ******************** //
  // ** Initialisation ** // Fonctions de départ, non réutilisable
  // ******************** //

  public override void OnStartClient()
  {
    if (Instance == null)
      Instance = this;

    foreach (NetworkLobbyPlayer obj in GameObject.FindObjectsOfType<NetworkLobbyPlayer>())
      {
        numberPlayer++;
      }

    if (numberPlayer > 1)
      isSoloGame = false;

    StartCoroutine(waitForInit());
  }

  IEnumerator waitForInit()
  {
    while (!LoadingManager.Instance.isGameReady())
      yield return new WaitForEndOfFrame();
    Init();
  }

  private void Init()
  {
    actualAction = PersoAction.isIdle;
    StartCoroutine(LateOnEnable());
  }

  IEnumerator LateOnEnable()
  {
    yield return new WaitForEndOfFrame();
    TurnManager.Instance.changeTurnEvent += OnChangeTurn;
  }

  void OnDisable()
  {
    if (LoadingManager.Instance != null && LoadingManager.Instance.isGameReady())
      {
        TurnManager.Instance.changeTurnEvent -= OnChangeTurn;
      }
  }

  // *************** //
  // ** Events **    // Appel de fonctions au sein de ce script grâce à des events
  // *************** //

  void OnChangeTurn(object sender, PlayerArgs e)
  { // Un joueur a terminé son tour
    currentPhase = e.currentPhase;
    currentPlayer = e.currentPlayer;
    GameManager.Instance.actualAction = PersoAction.isIdle;
    ResetMana();

    if (TurnManager.Instance.currentPhase == Phase.Deplacement && TurnManager.Instance.TurnNumber > 3) // si on est en jeu
      {
        ManaManager.Instance.ChangeMaxMana(currentPlayer, 1, true);
        if (TurnManager.Instance.TurnNumber != 2// si on est pas au premier tour de jeu
            && TurnManager.Instance.TurnNumber % 2 == 0) // et qu'on vient d'arriver sur un nouveau tour rouge
          {
            DecrementeManaMax(1);
          }
        EffectManager.Instance.ChangePA(paDebuff);
        paDebuff = 0;
      }
  }

  // *************** //
  // ** Fonctions ** // Fonctions réutilisables ailleurs
  // *************** //

  /// <summary> La partie démarre une nouvelle manche, les joueurs doivent replacer leurs personnages. </summary>
  public IEnumerator NewManche()
  {
    TurnManager.Instance.enabled = true;
    GameManager.Instance.actualAction = PersoAction.isIdle;
    GameManager.Instance.ChangeCurrentPhase(Phase.Placement);
    foreach (CaseData obj in CaseManager.Instance.GetAllCase())
      {
        obj.ClearStatutToDefault();
      }

    foreach (PersoData obj in RosterManager.Instance.listHeroPlaced)
      {
        obj.gameObject.transform.position = new Vector3(999, 999, 999);
      }

    GameObject.Find("Ballon").transform.position = GameObject.Find("10 5").transform.position;

    SelectionManager.Instance.selectedCase = null;
    SelectionManager.Instance.selectedPersonnage = null;
    SelectionManager.Instance.selectedLastCase = null;
    SelectionManager.Instance.selectedLastPersonnage = null;
    yield return new WaitForEndOfFrame();
    TurnManager.Instance.TurnNumber = 0;
    TurnManager.Instance.ChangePhase(Phase.Placement);
  }

  /// <summary>Change le joueur en train de jouer par un autre.</summary>
  public void ChangeCurrentPlayer(Player newPlayer)
  {
    currentPlayer = newPlayer;
    UIManager.Instance.ChangeBanner(newPlayer);
    TurnManager.Instance.currentPlayer = newPlayer;
  }

  /// <summary>Change la phase actuelle par une autre.</summary>
  public void ChangeCurrentPhase(Phase newPhase)
  {
    currentPhase = newPhase;
  }

  /// <summary>Change l'action principale du joueur par une autre.</summary>
  public void ChangeActualAction(PersoAction newAction)
  {
    actualAction = newAction;
  }

  /// <summary>Redémarre la partie.</summary>
  public void ResetScene()
  {
    SceneManager.LoadScene("Game");
  }

  /// <summary>Retour au lobby.</summary>
  public void GoToLobby()
  {
    SceneManager.LoadScene("Lobby réseau");
  }

  /// <summary>Le mana global actuel du joueur est reset au mana max de la partie</summary>
  public void ResetMana()
  {
    manaGlobalActual = manaGlobalMax;
  }

  /// <summary>Le mana global diminue de tant de mana</summary>
  public void DecrementeManaMax(int mana)
  {
    manaGlobalMax -= mana;
  }

  /// <summary>Le mana actuel diminue de tant de mana</summary>
  public void DecrementeManaActual(int mana)
  {
    manaGlobalActual -= mana;
  }

    /// <summary>Le mana actuel diminue de tant de mana</summary>
    public void CanPlayTurn(bool etat)
    {
        canPlayTurn = etat;
    }
}
