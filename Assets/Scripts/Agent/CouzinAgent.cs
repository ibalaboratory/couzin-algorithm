using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Couzinのアルゴリズムに従うエージェント．
// 注：Rigidbody と Transform の position の更新タイミングの違いを利用するため，
// 物理演算は使わないが Rigidbody が必要．
[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
public class CouzinAgent : Agent {

    public float scale = 1f;

    // 斥力【単位：Units】
    public float zor = 1f;

    // 定位【単位：Units】
    public float zoo = 10f;

    // 吸引【単位：Units】
    public float zoa = 10f;

    // 視野【単位：度】
    public float fov = 270f;

    // 回転率【単位：度/秒】
    public float theta = 40f;

    // 速度【単位：Units/秒】
    public float speed = 3f;

    // 誤差（S.D.）【単位：度】
    public float sd = 0.05f;


    // 異種個体がいる場合のパラメタ
    // 種はGameObject.tagで識別
    // 捕食関係はGameObject.layerで識別（レイヤ番号が大きいほど強い）
    // layerは，8: Prey, 9: Predator, 10: ApexPredator, 11: Obstacle を使用．

    // 索敵距離【単位：Units】
    public float rOther = 2f;

    // 異種個体を見つけた時の速度変化係数
    public float speedAlpha = 1.0f;

    // 速度の上限・下限
    public float speedMax = 10f;
    public float speedMin = 0.1f;

    // 異種個体を見つけた時の最大回転率【単位：度/秒】
    public float thetaOther = 40f;

    ///////////////////
    // 継承プロパティ //
    //////////////////

    [HideInInspector]
    public override Vector3 Position {
        get { return selfRb.position; }
    }

    [HideInInspector]
    public override Vector3 Velocity {
        get { return selfRb.rotation * Vector3.forward * speed; }
    }

    ///////////////
    // 内部用変数 //
    //////////////

    // プログラムの実行中に変数のいくつかはインスペクターからは編集できないようにする
    [HideInInspector] public bool lockOnPlay = false;

    // 標準正規分布に従う乱数のジェネレーター
    private IEnumerator<float> grg;

    // このオブジェクトのRigidbody, Collider
    private Rigidbody selfRb;
    private Collider selfCollider;

    // tagやlayer関係
    private string selfTag;
    private int selfLayer;
    private int allLayerMask;
    private int obstacleLayer;

    public override void SetParameters(AgentParameters agentParameters) {
        this.agentParameters = agentParameters;
        if (this.agentParameters is CouzinAgentParameters couzinAgentParameters) {
            this.scale = couzinAgentParameters.scale;
            this.zor = couzinAgentParameters.zor;
            this.zoo = couzinAgentParameters.zoo;
            this.zoa = couzinAgentParameters.zoa;
            this.fov = couzinAgentParameters.fov;
            this.theta = couzinAgentParameters.theta;
            this.speed = couzinAgentParameters.speed;
            this.sd = couzinAgentParameters.sd;
            this.rOther = couzinAgentParameters.rOther;
            this.speedAlpha = couzinAgentParameters.speedAlpha;
            this.speedMax = couzinAgentParameters.speedMax;
            this.speedMin = couzinAgentParameters.speedMin;
            this.thetaOther = couzinAgentParameters.thetaOther;
        }
    }

    public override void AgentInitialize(AgentSetting initialSetting, AgentParameters initialParameters) {
        this.initialSetting = initialSetting;
        SetParameters(initialParameters);

        // 一部の変数をインスペクター上で固定
        lockOnPlay = true;

        // 初期化
        grg = GaussianRandomGenerator();
        selfRb = GetComponent<Rigidbody>();
        selfCollider = GetComponent<Collider>();
        selfRb.freezeRotation = true;

        selfTag = gameObject.tag;
        selfLayer = gameObject.layer;
        allLayerMask = (1 << 8) + (1 << 9) + (1 << 10) + (1 << 11); // layer 8 ~ 11
        if(selfLayer == 10) allLayerMask -= (1 << 8); // apexPredatorはpreyを気にしない
        obstacleLayer = 11;
    }

    // オブジェクトの選択時に Gizmos を表示
    void OnDrawGizmosSelected()
    {
        // zor, zoo, zoa の可視化
        ShowZones();

        // TODO: Show fov.
    }


    ///////////////////////
    // エージェントの行動 //
    //////////////////////

    // 向きを変えて移動
    public override void CalculateMove() {
        // 次に動くべき方向を計算する．
        Vector3 nextDirection;
        bool foundOther; // 異種個体がいたかどうか
        (nextDirection, foundOther) = NextDirection();

        // 動く方向をガウス分布に基づいてランダムに少しずらす．
        nextDirection = AddNoise(nextDirection);

        // 最大回転角【ラジアン】
        float maxRadiansDelta;
        if(foundOther) maxRadiansDelta = thetaOther * Mathf.Deg2Rad * Time.fixedDeltaTime;
        else maxRadiansDelta = theta * Mathf.Deg2Rad * Time.fixedDeltaTime;

        // 移動距離
        if(foundOther) speed *= speedAlpha;
        else speed /= speedAlpha;
        speed = Mathf.Clamp(speed, speedMin, speedMax);
        float moveDistance = speed * Time.fixedDeltaTime * scale;

        // 回転角が制限を超えないようにする．
        nextDirection = Vector3.RotateTowards(transform.forward, nextDirection, maxRadiansDelta, 0f);

        // 計算した position と rotation を保存する
        // 他の魚の計算に、移動前の position や rotation が必要であるため、ここでは更新しない.
        NewRotation = nextDirection;
        NewPosition = transform.position + nextDirection * moveDistance;
    }

    public override void Move() {
        float radius = Mathf.Max((zor + zoo + zoa), rOther) * scale;
        Collider[] nearbyColliders = Physics.OverlapSphere(NewPosition, radius, allLayerMask);

        foreach (Collider collider in nearbyColliders) {
            if (collider == selfCollider) {
                continue;
            }

            // 壁にぶつかったら死ぬ.
            if (collider.gameObject.layer == obstacleLayer) {
                Vector3 closest = collider.ClosestPoint(NewPosition);
                if (closest == NewPosition) {
                    selfRb.velocity = Vector3.zero;
                    Died();
                    return;
                }
            }
        }
        selfRb.MoveRotation(Quaternion.LookRotation(NewRotation));
        selfRb.MovePosition(NewPosition);
    }

    // 次に動くべき方向を計算する．異種個体を見つけたかも返す．
    (Vector3, bool) NextDirection() {
        // 返り値
        Vector3 nextDirection; // 次に動くべき方向
        bool foundOther; // 周囲に異種個体や障害物が存在したかどうか


        // 周囲のエージェント・障害物をリストアップ
        float radius = Mathf.Max((zor + zoo + zoa), rOther) * scale;
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, radius, allLayerMask);

        // 距離などで分別
        List<Vector3> zorObstacles = new List<Vector3>();
        List<Vector3> rOtherObstacles = new List<Vector3>();
        List<Transform> zorEnemy = new List<Transform>();
        List<Transform> rOtherEnemy = new List<Transform>();
        List<Transform> zorAgents = new List<Transform>();
        List<Transform> zooAgents = new List<Transform>();
        List<Transform> zoaAgents = new List<Transform>();

        // 距離比較のために距離の2乗を用意
        float zorSqr = Mathf.Pow(zor * scale, 2);
        float zooSqr = Mathf.Pow((zor + zoo) * scale, 2);
        float zoaSqr = Mathf.Pow((zor + zoo + zoa) * scale, 2);
        float rOtherSqr = Mathf.Pow(rOther * scale, 2);

        foreach(Collider collider in nearbyColliders) {
            if (collider == selfCollider) {
                continue;
            }

            if (collider.gameObject.layer == obstacleLayer) {
                Vector3 obstacleClosestPoint = collider.ClosestPoint(transform.position);
                float distanceSqr = Vector3.SqrMagnitude(obstacleClosestPoint - transform.position);
                
                if (distanceSqr <= zorSqr) {
                    zorObstacles.Add(obstacleClosestPoint);
                } else if (distanceSqr <= rOtherSqr) {
                    rOtherObstacles.Add(obstacleClosestPoint);
                }
            } else if (collider.CompareTag(selfTag)) {
                float distanceSqr = Vector3.SqrMagnitude(collider.transform.position - transform.position);

                if (distanceSqr <= zorSqr) {
                    zorAgents.Add(collider.transform);
                } else if (distanceSqr <= zooSqr) {
                    if(InFOV(collider.transform)) zooAgents.Add(collider.transform);
                } else if (distanceSqr <= zoaSqr) {
                    if(InFOV(collider.transform)) zoaAgents.Add(collider.transform);
                }
            } else {
                float distanceSqr = Vector3.SqrMagnitude(collider.transform.position - transform.position);

                if(distanceSqr <= zorSqr) {
                    zorEnemy.Add(collider.transform);
                } else if (distanceSqr <= rOtherSqr) {
                    if(InFOV(collider.transform)) {
                        rOtherEnemy.Add(collider.transform);
                    }
                }
            }
        }

        // zorに異種個体か障害物が存在する場合
        if (zorEnemy.Any() || zorObstacles.Any()) {
            Vector3 chaseDirection = Vector3.zero;
            Vector3 fleeDirection = Vector3.zero;
            bool flee = zorObstacles.Any();

            foreach (Transform enemy in zorEnemy) {
                if ((!flee) && enemy.gameObject.layer < selfLayer) {
                    // 追う
                    chaseDirection += (enemy.position - transform.position).normalized;
                } else {
                    // 逃げる
                    fleeDirection -= (enemy.position - transform.position).normalized;
                    flee = true;
                }
            }

            foreach (Vector3 obstacle in zorObstacles) {
                fleeDirection -= (obstacle - transform.position).normalized;
            }

            if (flee) {
                nextDirection = fleeDirection;
            } else {
                nextDirection = chaseDirection;
            }

            foundOther = true;
            return (nextDirection, foundOther);
        }


        // rOtherに異種個体か障害物が存在する場合
        if(rOtherEnemy.Any() || rOtherObstacles.Any()) {
            Vector3 chaseDirection = Vector3.zero;
            Vector3 fleeDirection = Vector3.zero;
            bool flee = rOtherObstacles.Any();

            foreach (Transform enemy in rOtherEnemy) {
                if((!flee) && enemy.gameObject.layer < selfLayer){
                    // 追う
                    chaseDirection += (enemy.position - transform.position).normalized;
                } else {
                    // 逃げる
                    fleeDirection -= (enemy.position - transform.position).normalized;
                    flee = true;
                }
            }

            foreach(Vector3 obstacle in rOtherObstacles) {
                fleeDirection -= (obstacle - transform.position).normalized;
            }

            if(flee) nextDirection = fleeDirection;
            else nextDirection = chaseDirection;

            foundOther = true;
            return (nextDirection, foundOther);
        }


        // 周囲に異種個体がいない場合，通常のCouzinのアルゴリズム
        foundOther = false;


        // zorにエージェントがいる場合．
        if (zorAgents.Any()) {
            nextDirection = Vector3.zero;
            foreach (Transform agent in zorAgents) {
                nextDirection -= (agent.position - transform.position).normalized;
            }

            return (nextDirection, foundOther);
        }


        // zorにエージェントがいない場合．

        // zooに対して
        Vector3 d_o = transform.forward;
        bool zooAny = zooAgents.Any();
        if (zooAny) {
            foreach (Transform agent in zooAgents) {
                d_o += agent.forward;
            }
        }

        // zoaに対して
        Vector3 d_a = Vector3.zero;
        bool zoaAny = zoaAgents.Any();
        if (zoaAny) {
            foreach(Transform agent in zoaAgents) {
                d_a += (agent.position - transform.position).normalized;
            }
        }

        // 最終的なnextDirectionの計算
        if (zooAny && zoaAny) {
            // zooとzoaの両方にエージェントがいるとき
            nextDirection = (d_o + d_a) / 2f;
        } else if(zooAny) {
            // zooのみ
            nextDirection = d_o;
        } else if(zoaAny) {
            // zoaのみ
            nextDirection = d_a;
        } else {
            // 周囲にエージェントがいない
            nextDirection  = transform.forward;
        }

        return (nextDirection, foundOther);
    }

    // 死角チェック
    bool InFOV(Transform other)
    {
        Vector3 direction = other.position - transform.position;
        float angle = Vector3.Angle(transform.forward, direction);
        return angle <= fov / 2;
    }

    // 動く方向をガウス分布に基づいてランダムに少しずらす．
    Vector3 AddNoise(Vector3 nextDirection)
    {
        float phi1 = GaussianRandom() * sd;
        float phi2 = Random.Range(0f,180f);

        Vector3 axis;
        if(nextDirection.x==0 && nextDirection.z==0) axis = Vector3.right;
        else axis = Vector3.Cross(nextDirection,Vector3.up);

        nextDirection = Quaternion.AngleAxis(phi1, axis) * nextDirection;
        nextDirection = Quaternion.AngleAxis(phi2, nextDirection) * nextDirection;
        nextDirection = Vector3.Normalize(nextDirection);

        return nextDirection;
    }

    

    // 他のオブジェクトとの衝突後の処理
    void OnCollisionExit(Collision collision)
    {
        // 他の物体との衝突後に速度が残っていることがあるので処理
        selfRb.velocity = Vector3.zero;
    }

    // 捕食
    void OnCollisionEnter(Collision collision)
    {
        if(selfLayer <= 8) return;

        if(selfLayer == collision.gameObject.layer + 1)
        {
            Killed(collision);
            speed = speedMin;
        }
    }

    void Killed(Collision collision) {
        collision.gameObject.SetActive(false);
        collision.gameObject.GetComponent<Agent>().isDead = true;
        speed = speedMin;
    }

    void Died() {
        gameObject.SetActive(false);
        isDead = true;
    }


    ////////////////
    // Gizmos表示 //
    ///////////////

    // zor, zoo, zoa の可視化
    void ShowZones()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
        Gizmos.DrawSphere(transform.position, zor*scale);
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, (zor+zoo)*scale);
        Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
        Gizmos.DrawSphere(transform.position, (zor+zoo+zoa)*scale);
    }

    ////////////
    // その他 //
    ///////////

    // 標準正規分布に従う乱数のジェネレーター（ボックス＝ミュラー法）
    IEnumerator<float> GaussianRandomGenerator()
    {
        float u, v;
        while(true)
        {
        do u = Random.value; while(u==0);
        v = Random.Range(0f,360f);
        
        float r = Mathf.Sqrt(-2*Mathf.Log(u));
        yield return r * Mathf.Cos(v);
        yield return r * Mathf.Sin(v);
        }
    }

    // 標準正規分布に従う乱数を1つ返す．
    float GaussianRandom()
    {
        grg.MoveNext();
        return grg.Current;
    }
}
