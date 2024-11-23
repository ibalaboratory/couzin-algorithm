using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable, HideInInspector]
public class AgentSetting {
    // 複製するエージェントの原本
    public Transform original;

    // 複製するエージェントの数
    public int nAgent = 1;

    // ランダムに配置する位置の下限と上限
    public Vector3 initialPositionLowerBound = Vector3.zero;
    public Vector3 initialPositionUpperBound = Vector3.one;

    // 回転（向き）をランダムに配置するかどうか
    public bool randomizeRotation = true;

    // randomizeRotationがfalseの時のエージェントの向き
    public Vector3 initialDirection = Vector3.forward;
}