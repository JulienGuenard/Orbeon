using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTransition : MonoBehaviour {

public void ChangeImage()
    {
        ManualManager.Instance.Event_NextSlideImage();
    }

    public void ChangeText()
    {
        ManualManager.Instance.Event_NextSlideText();
    }
}
