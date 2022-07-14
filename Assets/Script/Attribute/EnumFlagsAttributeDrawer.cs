#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{

	public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
	{
		int buttonsIntValue = 0;
		int enumLength = _property.enumNames.Length;
		bool[] buttonPressed = new bool[enumLength];
		float buttonWidth = (_position.width - EditorGUIUtility.labelWidth) / enumLength;

		EditorGUI.LabelField(new Rect(_position.x, _position.y, EditorGUIUtility.labelWidth, _position.height), _label);

		EditorGUI.BeginChangeCheck ();
    int y = -1;
    int x = -1;
		for (int i = 0; i < enumLength; i++) {

			// Check if the button is/was pressed 
			if ( ( _property.intValue & (1 << i) ) == 1 << i ) {
				buttonPressed[i] = true;
			}
        x++;
        
          if (x >= 3)
          {
            x = 0;
            y++;
          }
          Rect buttonPos = new Rect (_position.x + EditorGUIUtility.labelWidth + buttonWidth+(88*x)-70, _position.y+15*y, buttonWidth+70, _position.height);
         
			buttonPressed[i] = GUI.Toggle(buttonPos, buttonPressed[i], _property.enumNames[i],  "Button");

			if (buttonPressed[i])
				buttonsIntValue += 1 << i;
		}

		if (EditorGUI.EndChangeCheck()) {
			_property.intValue = buttonsIntValue;
		}
	}
}
#endif