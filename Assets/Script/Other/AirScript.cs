using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirScript : MonoBehaviour {

    static public AirScript Instance;
    public bool selected;
    public GameObject airEffect;
    public GameObject fireAirEffect;

    void Awake()
    {
        Instance = this;
    }

  void OnEnable()
    {
      EventManager.newClickEvent += OnNewClick;
    }

  void OnDisable()
    {
      EventManager.newClickEvent -= OnNewClick;
    }
      
  public void OnNewClick ()
    { // Lors d'un click sur une case
      if (GameManager.Instance.actualAction == PersoAction.isSelected)
        {
          if (selected == true)
            {
              airEffect.transform.position = HoverManager.Instance.hoveredCase.GetComponent<CaseData>().transform.position;
              airEffect.SetActive(true);
              selected = false;
              return;
            }
        }
    }

  public void Select()
    {
      selected = true;
    }
}
