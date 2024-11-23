using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class CouzinAgentParameters : AgentParameters {
    ///////////////////////
    // エージェントの設定 //
    /////////////////////

    // スケール　（エージェントの大きさ，Unitsの基準）
    public float scale { get; set; }

    // 斥力【単位：Units】
    public float zor { get; set; }

    // 定位【単位：Units】
    public float zoo { get; set; }

    // 吸引【単位：Units】
    public float zoa { get; set; }

    // 視野【単位：度】
    public float fov { get; set; }

    // 回転率【単位：度/秒】
    public float theta { get; set; }

    // 速度【単位：Units/秒】
    public float speed { get; set; }

    // 誤差（S.D.）【単位：度】
    public float sd { get; set; }


    // 異種個体がいる場合のパラメタ
    // 種はGameObject.tagで識別
    // 捕食関係はGameObject.layerで識別（レイヤ番号が大きいほど強い）
    // layerは，8: Prey, 9: Predator, 10: ApexPredator, 11: Obstacle を使用．

    // 索敵距離【単位：Units】
    public float rOther { get; set; }

    // 異種個体を見つけた時の速度変化係数
    public float speedAlpha { get; set; }

    // 速度の上限・下限
    public float speedMax { get; set; }
    public float speedMin { get; set; }

    // 異種個体を見つけた時の最大回転率【単位：度/秒】
    public float thetaOther { get; set; }

    public void SetDefaultPreyValue() {
        this.scale = 1f;
        this.zor = 1f;
        this.zoo = 2.5f;
        this.zoa = 14f;
        this.fov = 270f;
        this.theta = 40f;
        this.speed = 3f;
        this.sd = 5f;
        this.rOther = 7f;
        this.speedAlpha = 1.05f;
        this.speedMax = 6f;
        this.speedMin = 3f;
        this.thetaOther = 90f;
    }

    public void SetDefaultPredatorValue() {
        this.scale = 2f;
        this.zor = 1f;
        this.zoo = 10f;
        this.zoa = 10f;
        this.fov = 200f;
        this.theta = 30f;
        this.speed = 2f;
        this.sd = 5f;
        this.rOther = 2.5f;
        this.speedAlpha = 1.001f;
        this.speedMax = 4f;
        this.speedMin = 2f;
        this.thetaOther = 100f;
 
    }

}
