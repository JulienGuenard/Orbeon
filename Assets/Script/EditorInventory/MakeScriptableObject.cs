#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public class MakeScriptableObject
{
  [MenuItem("Assets/Create/My Scriptable Object")]
  public static void CreateMyAsset()
  {
    MyScriptableObjectClass asset = ScriptableObject.CreateInstance<MyScriptableObjectClass>();


    AssetDatabase.CreateAsset(asset, "Assets/NewScripableObject.asset");
    AssetDatabase.SaveAssets();
    EditorUtility.FocusProjectWindow();

    Selection.activeObject = asset;

  }

  void OnGUI()
  {
    GUILayout.BeginHorizontal();
    EditorUtility.FocusProjectWindow();
  }
}
#endif