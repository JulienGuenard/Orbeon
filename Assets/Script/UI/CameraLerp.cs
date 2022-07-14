using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLerp : MonoBehaviour
{

  [SerializeField]
  public Camera lerpTarget;

  Vector3 lastPosition;
  float lastSize;

  // Use this for initialization
  void Start()
  {
    lastPosition = lerpTarget.transform.position;
    lastSize = lerpTarget.orthographicSize;
  }

  // Update is called once per frame
  void Update()
  {
        if (lastPosition != lerpTarget.transform.position)
      {
        lastPosition = lerpTarget.transform.position;
        StopCoroutine(lerpPosition());
        StartCoroutine(lerpPosition());
      }
    if (lastSize != lerpTarget.orthographicSize)
      {
        lastSize = lerpTarget.orthographicSize;
        StopCoroutine(lerpZoom());
        StartCoroutine(lerpZoom());
      }
  }

  IEnumerator lerpPosition()
  {
    float t = 1;
    while (Camera.main.transform.position != lerpTarget.transform.position)
      {
        t += Time.deltaTime;
        if (t > 1)
          t = 1;
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, lerpTarget.transform.position, t);
        yield return new WaitForSeconds(Time.deltaTime);
      }
    yield return null;
  }

  IEnumerator lerpZoom()
  {
    StopCoroutine(lerpPosition());

    float t = 0;
    while (Camera.main.orthographicSize != lerpTarget.orthographicSize)
      {
        t += Time.deltaTime;
        if (t > 1)
          t = 1;
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, lerpTarget.orthographicSize, t);
        yield return new WaitForSeconds(Time.deltaTime);
      }
    yield return null;
  }
}
