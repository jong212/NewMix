using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(EnumArrayAttribute))]
public class EnumArrayDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EnumArrayAttribute enumNames = attribute as EnumArrayAttribute;
		//propertyPath returns something like component_hp_max.Array.data[4]
		//so get the index from there
		int index = System.Convert.ToInt32(property.propertyPath.Substring(property.propertyPath.IndexOf("[")).Replace("[", "").Replace("]", ""));
		//change the label
		if (index < enumNames.names.Length)
		{
			label.text = enumNames.names[index];
		}
		//draw field
		EditorGUI.PropertyField(position, property, label, true);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}
}