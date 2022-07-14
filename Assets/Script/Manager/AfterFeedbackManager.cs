using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AfterFeedbackManager : NetworkBehaviour
{

	public static AfterFeedbackManager Instance;

	public List<GameObject> listTextFeedback;
	public List<GameObject> listTextFeedbackPredict;
	public List<GameObject> listPersoPredict;
	public Animator explodeEffect;

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
		listTextFeedback.AddRange(GameObject.FindGameObjectsWithTag("AfterFeedback"));
	}

	public void TackleText(int randomInt, int maxInt, GameObject obj)
	{
		StartCoroutine(TackleTextCoroutine(randomInt, maxInt, obj));
	}

	IEnumerator TackleTextCoroutine(int randomInt, int maxInt, GameObject obj)
	{
		GameObject takenText = listTextFeedback[0];
		listTextFeedback.Remove(takenText);
		takenText.transform.position = obj.transform.position;
        TextMesh takenTextMesh = takenText.GetComponentInChildren<TextMesh>();

        GameObject spriteObj = takenText.GetComponentInChildren<SpriteRenderer>().gameObject;
		if (randomInt < maxInt)
		{
            takenTextMesh.text = "Tackled!";
            takenTextMesh.color = new Color(0, .4f, 0, 1f);
			spriteObj.SetActive(false);
		} else
		{
            takenTextMesh.text = "Miss!";
            takenTextMesh.color = new Color(.8f, 0, 0, 1f);
			spriteObj.SetActive(false);
		}

		for (int i = 0; i < 300; i++)
		{
            takenTextMesh.color -= new Color(0, 0, 0, 0.0033f);
			takenText.transform.position += new Vector3(0, 0.01f, 0);
			yield return new WaitForEndOfFrame();
		}
		takenText.transform.position = new Vector3(999, 999, 999);
		spriteObj.SetActive(true);
		listTextFeedback.Add(takenText);
	}

	public void PRText(int PRchanged, GameObject obj, bool positiveValue = false)
	{
		if (obj.GetComponent<PersoData>() && obj.GetComponent<PersoData>().timeStunned > 0)
			return;

        if (PRchanged < 0)
            positiveValue = true;
        else
            positiveValue = false;

        StartCoroutine(PRTextCoroutine(PRchanged, obj, positiveValue));
	}

	IEnumerator PRTextCoroutine(int PRchanged, GameObject obj, bool positiveValue)
	{
		GameObject takenText = listTextFeedback[0];
		listTextFeedback.Remove(takenText);
		if (positiveValue)
		{
			takenText.GetComponentInChildren<TextMesh>().color = new Color(1, 1, 1, 1f);
			takenText.GetComponentInChildren<TextMesh>().text = "+" + Mathf.Abs(PRchanged).ToString();
			takenText.GetComponentInChildren<SpriteRenderer>().gameObject.SetActive(true);
			takenText.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
		} else
		{
			takenText.GetComponentInChildren<TextMesh>().color = new Color(1, 1, 1, 1f);
			takenText.GetComponentInChildren<TextMesh>().text = "-" + PRchanged.ToString();
			takenText.GetComponentInChildren<SpriteRenderer>().gameObject.SetActive(true);
			takenText.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
		}

		takenText.transform.position = obj.transform.position;

        WaitForSeconds delay = new WaitForSeconds(0.01f);
        for (int i = 0; i < 300; i++)
		{
			takenText.GetComponentInChildren<TextMesh>().color -= new Color(0, 0, 0, 0.0033f);
			takenText.GetComponentInChildren<SpriteRenderer>().color -= new Color(0, 0, 0, 0.0033f);
			takenText.transform.position += new Vector3(0, 0.01f, 0);
            yield return delay;
		}
		takenText.transform.position = new Vector3(999, 999, 999);
		listTextFeedback.Add(takenText);
	}

	public void ExplodeEffect(GameObject target)
	{
		explodeEffect.transform.position = target.transform.position + Vector3.up * 0.5f;
		explodeEffect.SetTrigger("BOOM");
	}
}
