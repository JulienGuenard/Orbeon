using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fxRescaler : MonoBehaviour
{
	
	void Update()
	{
		transform.localScale = new Vector3(Camera.main.orthographicSize / 12, Camera.main.orthographicSize / 12, Camera.main.orthographicSize / 12);
	}
}
