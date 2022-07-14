using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMethods : MonoBehaviour
{

  [HideInInspector] public static CameraMethods Instance;
  [SerializeField]
  float maxZoomIn = 0;
  [SerializeField]
  float maxZoomOut = 0;

  [SerializeField]
  Vector3 globalPosition;

  public float currentZoom;

  Vector3 globalBottomLeftRect;
  Vector3 globalTopRightRect;

  Vector3 localBottomLeftRect;
  Vector3 localTopRightRect;

  Camera thisCamera;

  // *************** //
  // ** Initialisation ** //
  // *************** //

  void Awake()
  {
    currentZoom = maxZoomOut;
    Instance = this;
    globalPosition = transform.position;

    thisCamera = GetComponent<Camera>();

    globalBottomLeftRect = thisCamera.ScreenToWorldPoint(new Vector3(0, 0, thisCamera.nearClipPlane));
    globalTopRightRect = thisCamera.ScreenToWorldPoint(new Vector3(thisCamera.pixelWidth, thisCamera.pixelHeight, thisCamera.nearClipPlane));
  }

  private void Update()
  {
    // prend les inputs

    if (Input.GetAxis("Mouse ScrollWheel") != 0)
      {
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * (thisCamera.orthographicSize * 10 / 3);
      }

    if (Input.GetAxis("Horizontal") != 0)
      {
        transform.Translate(Vector3.right * Input.GetAxis("Horizontal"));
      }

    if (Input.GetAxis("Vertical") != 0)
      {
        transform.Translate(Vector3.up * Input.GetAxis("Vertical"));
      }

    if (Input.GetKey(KeyCode.Mouse1))
      {
        transform.position += new Vector3(Camera.main.ScreenToViewportPoint(Input.mousePosition).x - 0.5f, Camera.main.ScreenToViewportPoint(Input.mousePosition).y - 0.5f, 0);
      }

    if (currentZoom <= maxZoomIn)
      currentZoom = maxZoomIn;
    else if (currentZoom > maxZoomOut)
      {
        currentZoom = maxZoomOut;
        transform.position = globalPosition;
      }
    thisCamera.orthographicSize = currentZoom;

    // ajuste selon les bounds

    localBottomLeftRect = thisCamera.ScreenToWorldPoint(new Vector3(0, 0, thisCamera.nearClipPlane));
    localTopRightRect = thisCamera.ScreenToWorldPoint(new Vector3(thisCamera.pixelWidth, thisCamera.pixelHeight, thisCamera.nearClipPlane));

    Vector3 offset = Vector3.zero;
    if (localBottomLeftRect.x < globalBottomLeftRect.x)
      {
        offset.Set(globalBottomLeftRect.x - localBottomLeftRect.x, offset.y, offset.z);
      }

    if (localBottomLeftRect.y < globalBottomLeftRect.y)
      {
        offset.Set(offset.x, globalBottomLeftRect.y - localBottomLeftRect.y, offset.z);
      }
    if (localTopRightRect.x > globalTopRightRect.x)
      {
        offset.Set(globalTopRightRect.x - localTopRightRect.x, offset.y, offset.z);
      }
    if (localTopRightRect.y > globalTopRightRect.y)
      {
        offset.Set(offset.x, globalTopRightRect.y - localTopRightRect.y, offset.z);
      }

    transform.position += offset;
  }
}
