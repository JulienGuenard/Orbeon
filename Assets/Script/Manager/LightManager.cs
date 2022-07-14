using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LightManager : NetworkBehaviour {

    GameObject ambientLight1;
    int lightPhase = 0;

    public bool lightModifierON;

    public static LightManager Instance;

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

    private void Init()
    {
        TurnManager.Instance.changeTurnEvent += OnChangeTurn;
        ambientLight1 = GameObject.Find("AmbientLight1");
    }

    void OnDisable()
    {
        if (LoadingManager.Instance != null && LoadingManager.Instance.isGameReady())
        {
            TurnManager.Instance.changeTurnEvent -= OnChangeTurn;
        }
    }

    void OnChangeTurn(object sender, PlayerArgs e)
    { // Lorsqu'un joueur termine son tour
        if (!lightModifierON)
            return;

        if (TurnManager.Instance.currentPhase == Phase.Deplacement && lightPhase == 0)
        {
            lightPhase = 1;
            ambientLight1.GetComponent<Animator>().SetTrigger("DeplacementPhase");
        }
    }

    private void Update()
    {
        if (!lightModifierON)
            return;

       if (lightPhase == 1 && (UIManager.Instance.scoreBlue != 0 || UIManager.Instance.scoreRed != 0))
            {
            lightPhase = 2;
            ambientLight1.GetComponent<Animator>().SetTrigger("WinPhase");
        }
    }
}
