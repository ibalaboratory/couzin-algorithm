using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// GameObjectの軌跡を表示するTrailRendererの設定を
// InspectorでいじれるようにするTrailControllerのためのGUI.
// 注：Play中にしかいじれません．
[CustomEditor(typeof(TrailController))]
[CanEditMultipleObjects]
public class TrailControllerEditor : Editor
{
    // 軌跡の持続時間[s]
    SerializedProperty durationProp;
    // 軌跡の色
    SerializedProperty colorProp;

    // 非playモード時の表示テキスト
    private string nonPlayModeText = "Only available in play mode.";

    // trailオブジェクトが見つからない時の表示テキスト
    private string trailNotSetText = "No trail object found.";

    void OnEnable()
    {
        durationProp = serializedObject.FindProperty("duration");
        colorProp = serializedObject.FindProperty("color");
    }


    public override void OnInspectorGUI()
    {
        // 非playモード時
        if(!Application.isPlaying)
        {
            GUILayout.Label(nonPlayModeText);
            return;
        }

        // trailオブジェクトが見つからない時
        if(!CheckTrailRenderer())
        {
            GUILayout.Label(trailNotSetText);
            return;
        }

        serializedObject.Update();

        // durationの入力フィールド
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(durationProp);
        if(EditorGUI.EndChangeCheck()) ApplyDurationChange();

        // colorの入力フィールド
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(colorProp);
        if(EditorGUI.EndChangeCheck()) ApplyColorChange();

        // 色をランダムに変えるボタン
        if(GUILayout.Button("Randomize Color"))
        {
            ApplyRandomizeColor();
        }

        serializedObject.ApplyModifiedProperties();
        
        // trailを有効化するボタン
        if(GUILayout.Button("Enable Trail"))
        {
            ApplyEnableTrail();
        }

        // trailを無効化するボタン
        if(GUILayout.Button("Disable Trail"))
        {
            ApplyDisableTrail();
        }
    }

    // 選択されているオブジェクトでTrailRendererが認識できているか確認．
    bool CheckTrailRenderer()
    {
        foreach(Object obj in targets)
        {
            TrailController controller = obj as TrailController;
            if(controller.trailRenderer) return true;
        }
        return false;
    }

    // durationの変化を適用
    void ApplyDurationChange()
    {
        foreach(Object obj in targets)
        {
            TrailController controller = obj as TrailController;
            if(!controller.trailRenderer) continue;
            controller.trailRenderer.time = durationProp.floatValue;
        }
    }

    // colorの変化を適用
    void ApplyColorChange()
    {
        foreach(Object obj in targets)
        {
            TrailController controller = obj as TrailController;
            if(!controller.trailRenderer) continue;
            controller.trailRenderer.startColor = colorProp.colorValue;
        }
    }

    // 選択されているすべてのオブジェクトの色を一括でランダム化
    private void ApplyRandomizeColor()
    {
        foreach(Object obj in targets)
        {
            TrailController trailController = obj as TrailController;
            trailController.RandomizeColor();
        }
    }

    // 選択されているすべてのオブジェクトのtrailを一括で有効化
    private void ApplyEnableTrail()
    {
        foreach(Object obj in targets)
        {
            TrailController trailController = obj as TrailController;
            trailController.EnableTrail();
        }
    }

    
    // 選択されているすべてのオブジェクトのtrailを一括で無効化
    private void ApplyDisableTrail()
    {
        foreach(Object obj in targets)
        {
            TrailController trailController = obj as TrailController;
            trailController.DisableTrail();
        }
    }
}
