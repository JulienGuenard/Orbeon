using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BeforeFeedbackManager : NetworkBehaviour
{

  public static BeforeFeedbackManager Instance;

  public List<GameObject> listTextFeedback;
  public List<GameObject> listTextFeedbackPredict;
  public List<GameObject> listPersoPredict;
  public SpriteRenderer showObj;

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

    InitGame();
  }

  void InitGame()
  {
    listTextFeedback.AddRange(GameObject.FindGameObjectsWithTag("textBeforeFeedback"));
  }

  public void PredictInit(int maxInt, GameObject obj)
  {

    if (listPersoPredict.Count != 0)
      {
        foreach (GameObject similarObj in listPersoPredict)
          {
            if (similarObj == obj)
              return;

          }
      }
    if (listTextFeedback.Count == 0)
      return;

    /* GameObject takenText = listTextFeedback[0];
    takenText.name = obj.name + " " + "feedback";
    listTextFeedback.Remove(takenText);
    listTextFeedbackPredict.Add(takenText);
    listPersoPredict.Add(obj);
    takenText.GetComponent<TextMesh>().text = "!";
    takenText.transform.position = obj.transform.position + new Vector3(0, 0.25f, 0);
    takenText.GetComponent<TextMesh>().color = new Color(.9f, 0, 0, 1f);*/
  }

  public void PredictEnd(GameObject obj)
  {
    if (GameObject.Find(obj.name + " " + "feedback") == null)
      return;

    listPersoPredict.Remove(obj);
    GameObject objText = GameObject.Find(obj.name + " " + "feedback");
    objText.name = "textFeedback";
    listTextFeedback.Add(objText);
    objText.transform.position = new Vector3(999, 999, 999);
  }

  public void PredictDeplacement(GameObject obj, CaseData newCaseObj)
  {
    showObj.transform.gameObject.SetActive(false);
    if (newCaseObj == null)
    {
      return;
    }
    if (obj.GetComponent<PersoData>() != null)
      {
        PersoData persoData = obj.GetComponent<PersoData>();
        showObj.sprite = persoData.transform.GetComponentInChildren<SpriteRenderer>().sprite;
        showObj.transform.position = newCaseObj.transform.position - persoData.originPoint.transform.localPosition + Vector3.up * 0.1f;
            showObj.transform.localScale = persoData.GetComponentInChildren<SpriteRenderer>().transform.localScale;
      }
    if (obj.GetComponent<BallonData>() != null)
      {
        BallonData ballonData = obj.GetComponent<BallonData>();
        showObj.sprite = ballonData.transform.GetComponent<SpriteRenderer>().sprite;
        showObj.transform.position = newCaseObj.transform.position - ballonData.offsetBallon;
        showObj.transform.localScale = new Vector3(1.25f, 1.25f, 1);
      }
    showObj.transform.gameObject.SetActive(true);
  }

  public void HidePrediction()
  {
    showObj.transform.gameObject.SetActive(false);
  }
}
