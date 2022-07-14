using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TutoManager : NetworkBehaviour
{
    public enum TutoPassType
        {
        Normal,
        Selection,
        Place,
        PlaceX8
        }

    [System.Serializable]
    public struct TutoPopupType
    {
        public string title;
        [TextArea]
        public string description;
        public TutoPassType passType;
        public Vector2 popupAnchorMin;
        public Vector2 popupAnchorMax;
    }

    public List<TutoPopupType> listTutoPopup = new List<TutoPopupType>();
    TutoPopupType actualTutoPopup;

    GameObject tuto;
    GameObject tutoPopupPass;
    GameObject tutoPopupTitle;
    GameObject tutoPopupText;
    GameObject tutoBlackScreen;
    GameObject tutoPopup;

    bool isTuto = false;

    public static TutoManager Instance;

    void Awake ()
    {
        Instance = this;
    }

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
        actualTutoPopup = new TutoPopupType();
        tuto = GameObject.Find("Tuto");
        tutoPopupPass = GameObject.Find("TutoPopupPass");
        tutoPopupTitle = GameObject.Find("TutoPopupTitle");
        tutoPopupText = GameObject.Find("TutoPopupText");
        tutoBlackScreen = GameObject.Find("TutoBlackScreen");
        tutoPopup = GameObject.Find("TutoPopup");
    }

    private void Update()
    {
        if (tuto == null)
            return;

        if (!isTuto)
        {
            PopupApparitions();
            return;
        }

        if (TutoPassType.Normal == actualTutoPopup.passType)
            CaseManager.Instance.DisableAllColliders();

        if (TutoPassType.Selection == actualTutoPopup.passType && SelectionManager.Instance.selectedPersonnage != null)
            EventPopupEnd();

        if (TutoPassType.Place == actualTutoPopup.passType && RosterManager.Instance.listHeroPlaced.Count != 0)
            EventPopupEnd();

        if (TutoPassType.PlaceX8 == actualTutoPopup.passType && GameManager.Instance.currentPhase == Phase.Deplacement)
            EventPopupEnd();
    }

    public void EventPopupEnd()
    {
        isTuto = false;
        actualTutoPopup.passType = TutoPassType.Normal;
        CaseManager.Instance.EnableAllColliders();
        tuto.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
        tuto.GetComponent<RectTransform>().anchorMax = new Vector2(2, 1);

        tutoPopupPass.SetActive(true);
        tutoBlackScreen.SetActive(true);
    }

    void PopupStart()
    {
        isTuto = true;
        tuto.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        tuto.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

        tutoPopup.GetComponent<RectTransform>().anchorMin = actualTutoPopup.popupAnchorMin;
        tutoPopup.GetComponent<RectTransform>().anchorMax = actualTutoPopup.popupAnchorMax;

        tutoPopupTitle.GetComponent<Text>().text = actualTutoPopup.title;
        tutoPopupText.GetComponent<Text>().text = actualTutoPopup.description;

        if (TutoPassType.Normal == actualTutoPopup.passType)
            return;

        if (TutoPassType.Selection == actualTutoPopup.passType)
        {
            SelectionManager.Instance.Deselect();
            InfoPerso.Instance.portrait1.GetComponent<infoPersosPortrait>().SelectPerso(null);
            PassInteraction(false);
            return;
        }

        if (TutoPassType.Place == actualTutoPopup.passType)
        {
            PassInteraction(false);
            return;
        }

        if (TutoPassType.PlaceX8 == actualTutoPopup.passType)
        {
            PassInteraction(false);
            return;
        }
    }

    void PassInteraction(bool isEnabled)
    {
        tutoPopupPass.SetActive(isEnabled);
        tutoBlackScreen.SetActive(isEnabled);
    }

    void PopupApparitions()
    {
        if (tuto == null)
        return;

        // intro
        if (actualTutoPopup.title == null)
        {
            actualTutoPopup = listTutoPopup[0];
            PopupStart();
            return;
        }

        // selection
        if (actualTutoPopup.title == listTutoPopup[0].title)
        {
            actualTutoPopup = listTutoPopup[1];
            PopupStart();
            return;
        }

        // placer
        if (actualTutoPopup.title == listTutoPopup[1].title)
        {
            actualTutoPopup = listTutoPopup[2];
            PopupStart();
            return;
        }

        // stats
        if (actualTutoPopup.title == listTutoPopup[2].title)
        {
            actualTutoPopup = listTutoPopup[3];
            PopupStart();
            return;
        }

        // sorts
        if (actualTutoPopup.title == listTutoPopup[3].title)
        {
            actualTutoPopup = listTutoPopup[4];
            PopupStart();
            return;
        }

        // placer 4 persos
        if (actualTutoPopup.title == listTutoPopup[4].title)
        {
            actualTutoPopup = listTutoPopup[5];
            PopupStart();
            return;
        }

        // selection(déplacement)
        if (actualTutoPopup.title == listTutoPopup[5].title && GameManager.Instance.currentPhase == Phase.Deplacement)
        {
            actualTutoPopup = listTutoPopup[6];
            PopupStart();
            return;
        }

        // déplacement
        if (actualTutoPopup.title == listTutoPopup[6].title && GameManager.Instance.currentPhase == Phase.Deplacement && SelectionManager.Instance.selectedPersonnage != null)
        {
            actualTutoPopup = listTutoPopup[7];
            PopupStart();
            return;
        }

        // Tacle
        if (actualTutoPopup.title == listTutoPopup[7].title)
        {
            actualTutoPopup = listTutoPopup[8];
            PopupStart();
            return;
        }

        // Sort
        if (actualTutoPopup.title == listTutoPopup[8].title)
        {
            actualTutoPopup = listTutoPopup[9];
            PopupStart();
            return;
        }

        // Ball
        if (actualTutoPopup.title == listTutoPopup[9].title)
        {
            actualTutoPopup = listTutoPopup[10];
            PopupStart();
            return;
        }

        // A vous de jouer !
        if (actualTutoPopup.title == listTutoPopup[10].title)
        {
            actualTutoPopup = listTutoPopup[11];
            PopupStart();
            return;
        }
    }
}
