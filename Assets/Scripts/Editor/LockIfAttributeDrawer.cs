using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// インスペクターに表示される変数の編集をコントロールするための属性
// LockIfを実装するためのクラス
[CustomPropertyDrawer(typeof(LockIfAttribute), true)]
public class LockIfAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool flag = property.serializedObject.FindProperty(((LockIfAttribute)this.attribute).flagName).boolValue;
        if(flag)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label, true); 
        }
    }
}
