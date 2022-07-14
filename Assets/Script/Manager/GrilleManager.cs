using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif

/// <summary>Permet de générer une grille de cases selon plusieurs paramètres comme le nombres de cases en X et le nombres de cases en Y.</summary>
public class GrilleManager: MonoBehaviour
{

	// *************** //
	// ** Variables ** // Toutes les variables sans distinctions
	// *************** //

	[Header("NewMap")]
	public bool pressToGenerate;
	[Range(1, 100)]
	public int largeur;
	[Range(1, 100)]
	public int hauteur;
	public float hSpace = 2;
	public float lSpace = 4.25f;

	public float xEcartCase;
	public float yEcartCase;

	[Header("Ressources")]
	public GameObject cube;
	public List<List<GameObject>> cubeAll = new List<List<GameObject>>();
	GameObject cubeNew;
	public static GrilleManager Instance;
	public Pathfinding path;

	// ******************** //
	// ** Initialisation ** // Fonctions de départ, non réutilisable
	// ******************** //

	void Awake()
	{
		pressToGenerate = false;
		if (Instance == null)
			Instance = this;
	}

	/*	void Update()
	{
		#if UNITY_EDITOR
		if (!EditorApplication.isPlaying)
		{
			if (pressToGenerate)
			{ 
				NewMap();
				if (path != null)
				{
					path.BakePathfinding(cubeAll);
				}
				pressToGenerate = false;
			}
		}
		#endif
	}*/

	// *************** //
	// ** Fonctions ** // Fonctions réutilisables ailleurs
	// *************** //

	void ConfigureListCube()
	{ // Détruit la grille
		foreach (Transform obj in GetComponentsInChildren<Transform>())
		{
			if (obj != this.transform)
			{
				DestroyImmediate(obj.gameObject);
			}
		}
		cubeAll.Clear();
	}

	public void NewMap()
	{ // Génère une nouvelle grille
		ConfigureListCube(); 
		CaseManager.listAllCase.Clear();

		for (int h = 0; h < hauteur; h++)
		{
			cubeAll.Add(new List<GameObject>());
			for (int l = 0; l < largeur; l++)
			{
				cubeNew = (GameObject)Instantiate(cube, new Vector2(h / hSpace + l / hSpace + transform.position.x, -h / lSpace + l / lSpace + transform.position.y), Quaternion.identity);
				cubeNew.transform.parent = transform;
				cubeNew.GetComponent<SpriteRenderer>().sortingOrder = -100;
				cubeNew.name = h + " " + l;
				cubeNew.GetComponent<CaseData>().xCoord = h;
				cubeNew.GetComponent<CaseData>().yCoord = l;
				cubeNew.GetComponent<SpriteRenderer>().color = ColorManager.Instance.caseColor;
				cubeAll[(int)h].Add(cubeNew);

				CaseManager.listAllCase.Add(cubeNew.GetComponent<CaseData>());
			}
		}
	}

	public List<List<GameObject>> getMap()
	{
		List<List<GameObject>> map = new List<List<GameObject>>();

		for (int h = 0; h < hauteur; h++)
		{
			map.Add(new List<GameObject>());
			for (int l = 0; l < largeur; l++)
			{
				map[h].Add(transform.GetChild(h * largeur + l).gameObject);
			}
		}
		return map;
	}
}