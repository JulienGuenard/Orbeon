using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireScript : MonoBehaviour {

    static public FireScript Instance;
    public bool selected;
    public Sprite fireSprite;

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
        //      HoverManager.Instance.hoveredCase.GetComponent<CaseData>().changeElement(Element.Feu);
              selected = false;
              return;
            }
        }
    }
      
    public void Select () {
      selected = true;
    }

}
