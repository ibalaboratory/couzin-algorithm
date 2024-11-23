using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// エージェントの抽象クラス．
// Couzinのアルゴリズム以外も
// このクラスを継承して試すことができる．
public abstract class Agent : MonoBehaviour
{
    // エージェントの位置・速度．
    // 他のスクリプト（主にAgentManager）からの参照用．
    // 注：何を報告するかの実装は継承先．
    // つまり，実際の値と違うものが報告される可能性もある．
    // 厳密に知るには，this.transform.positionなど．
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    public abstract Vector3 Position { get; }
    [HideInInspector]
    public Vector3 NewRotation;
    [HideInInspector]
    public Vector3 NewPosition;
    [HideInInspector]
    public abstract Vector3 Velocity { get; }
    [HideInInspector]
    public AgentSetting initialSetting;
    [HideInInspector]
    public AgentParameters agentParameters;

    public abstract void SetParameters(AgentParameters agentParameters);
    
    public abstract void CalculateMove();

    public abstract void Move();

    public abstract void AgentInitialize(AgentSetting initialSetting, AgentParameters initialParameters);

    public void Reset() {
        isDead = false;
        gameObject.SetActive(true);

        Vector3 position = Vector3.zero;
        position.x = Random.Range(this.initialSetting.initialPositionLowerBound.x, this.initialSetting.initialPositionUpperBound.x);
        position.y = Random.Range(this.initialSetting.initialPositionLowerBound.y, this.initialSetting.initialPositionUpperBound.y);
        position.z = Random.Range(this.initialSetting.initialPositionLowerBound.z, this.initialSetting.initialPositionUpperBound.z);
        
        Quaternion rotation;
        if (this.initialSetting.randomizeRotation) {
            rotation = Random.rotation; 
        } else {
            rotation = Quaternion.LookRotation(this.initialSetting.initialDirection); 
        }
        
        this.transform.position = position;
        this.transform.rotation = rotation;
    }
}
