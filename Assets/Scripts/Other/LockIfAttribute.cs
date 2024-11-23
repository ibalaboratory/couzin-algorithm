using UnityEngine;
using System;

// インスペクターに表示される変数の編集をコントロールするための属性
// 使用例：
// [LockIf("lockFlag")]をインスペクターに表示されるプロパティにつける．
// lockFlagがTrueの間はインスペクターでプロパティの値を編集できる．
// lockFlagがFalseの間はプロパティがグレーアウトして編集できなくなる．
// 注：単純なプロパティ（float, stringなど）では動作しますが，表示が複数行にわたるもの（配列など）などでは表示がバグると思います．
[AttributeUsage(AttributeTargets.Field, AllowMultiple=false, Inherited=true)]
public class LockIfAttribute : PropertyAttribute
{
    public string flagName {get; private set;}

    public LockIfAttribute(string fn)
    {
        flagName = fn;
    }
}
