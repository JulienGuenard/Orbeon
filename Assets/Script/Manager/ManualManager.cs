using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class ManualManager : MonoBehaviour {

    public static ManualManager Instance;

    public AnimationCurve showingCurve;
    public float showingSpeed;

    Image slideImage;
    TextMeshProUGUI slideTitle;
    TextMeshProUGUI slideDescription;
    Scrollbar slideScroll;
    Animator slideTransition;

    Image buttonClose;
    Image buttonRight;
    Image buttonLeft;
    Image buttonSumary;
    Image buttonQuitManual;
    Image buttonCloseCross;
    Image buttonShowManual;

    Vector3 cameraPos;

    GameObject manual; // Mettre le parent de tous les éléments du manuel se trouvant dans le canvas
    Image manualImage;

    [System.Serializable]
    public struct Slide
    {
        public Sprite spriteImage;
        public string textTitle;
        [TextAreaAttribute(15,20)]
        public string textDescription;
        public float valueScroll; // 1 ou moins = pas de scroll
    }

    public List<Slide> slideList;
    int slideActual;

	void Awake ()
    {
        Instance = this;

        // GameObject racine
        manual = GameObject.Find("Manual");

        // Slide Content
        slideImage = GameObject.Find("ManualImage").GetComponent<Image>();
        slideTitle = GameObject.Find("ManualText_Title").GetComponent<TextMeshProUGUI>();
        slideDescription = GameObject.Find("ManualText_DescriptionText").GetComponent<TextMeshProUGUI>();
        slideScroll = GameObject.Find("Scrollbar Vertical").GetComponent<Scrollbar>();
        slideTransition = GameObject.Find("ManualTransition").GetComponent<Animator>();


        // Buttons
        buttonQuitManual = GameObject.Find("ManualButton_QuitManual").GetComponent<Image>();
        buttonRight = GameObject.Find("ManualButton_Right").GetComponent<Image>();
        buttonLeft = GameObject.Find("ManualButton_Left").GetComponent<Image>();
        buttonSumary = GameObject.Find("ManualButton_Sumary").GetComponent<Image>();
        buttonCloseCross = GameObject.Find("ManualButton_CloseCross").GetComponent<Image>();
        buttonShowManual = GameObject.Find("ManualButton_ShowManual").GetComponent<Image>();

        slideActual = 0;
	}

	public void Button_ShowManual ()
    {
        StartCoroutine(ShowManual());
	}

    public void Button_HideManual()
    {
        StartCoroutine(HideManual());
    }
    
    public void Button_NextSlide(int increment)
    {
        NextSlide(increment);
    }

    void NextSlide(int increment)
    {
        slideActual += increment;

        if (slideActual > slideList.Count - 1)
            slideActual = 0;

        if (slideActual < 0)
            slideActual = slideList.Count - 1;

        slideTransition.SetTrigger("MakeTransition");
    }

    public void Event_NextSlideImage()
    {
        slideImage.sprite = slideList[slideActual].spriteImage;
    }

    public void Event_NextSlideText()
    {
        slideTitle.text = slideList[slideActual].textTitle;
        slideDescription.text = slideList[slideActual].textDescription;
        slideDescription.GetComponent<RectTransform>().anchorMax = new Vector2(1, slideList[slideActual].valueScroll);
        slideScroll.value = 1;
    }

    IEnumerator ShowManual()
    {
        for (float i = 0; i < 1f; i += showingSpeed)
        {
            yield return new WaitForSeconds(0.01f);
            
            manual.GetComponent<RectTransform>().anchorMin = new Vector2(0, -1.1f + (showingCurve.Evaluate(i) * 1.1f));
            manual.GetComponent<RectTransform>().anchorMax = new Vector2(1, -0.1f + (showingCurve.Evaluate(i) * 1.1f));
        }

    }

    IEnumerator HideManual()
    {
        for (float i = 0; i < 1f; i += showingSpeed)
        {
            yield return new WaitForSeconds(0.01f);

            manual.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0 - (showingCurve.Evaluate(i) * 1.1f));
            manual.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - (showingCurve.Evaluate(i) * 1.1f));
        }
    }
}
