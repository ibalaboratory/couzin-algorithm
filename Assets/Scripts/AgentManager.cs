using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AgentSetting;
using System.Reflection;
using System.Text;

// 設定したエージェントをランダムに配置するなど．
// エージェントの管理．
public class AgentManager : MonoBehaviour {
    // 時間幅
    [LockIf("lockOnPlay")]
    public float tau = 0.1f;
    [HideInInspector]
    public bool lockOnPlay = false;
    
    public float simulationTime;

    public List<int> deadCount = new List<int>();
    
    // 複数の種類のエージェントの設定
    [SerializeField]
    public List<AgentSetting> agentSettings = new List<AgentSetting>(){ new AgentSetting() };

    // インスタンス化したエージェントのリスト
    public List<List<Agent>> agents;
    
    // パラメータの初期化とエージェントの作成
    public void Initialize<T>(List<T> agentParametersList) where T : AgentParameters {
        agents = new List<List<Agent>>();
        for (int agentType = 0; agentType < agentParametersList.Count; agentType++) {
            deadCount.Add(0);
            agents.Add(AgentInstantiate(agentSettings[agentType], agentParametersList[agentType]));
        }
        
        Time.fixedDeltaTime = tau;
        lockOnPlay = true;
        simulationTime = 0;
    }
    
    // FixedUpdate()によって呼ばれる1フレーム分の更新.
    // 各エージェントを移動させ, simulationTimeやdeadCountなどの変数も更新する.
    public void Update() {
        simulationTime += Time.fixedDeltaTime;
        for (int agentType = 0; agentType < agents.Count; agentType++) {
            foreach (Agent agent in agents[agentType]) {
                agent.CalculateMove();
            }
        }
        for (int agentType = 0; agentType < agents.Count; agentType++) {
            foreach (Agent agent in agents[agentType]) {
                agent.Move();
            }
        }
         
        for (int agentType = 0; agentType < agents.Count; agentType++) {
            int count = 0;
            foreach (Agent agent in agents[agentType]) {
                if (agent.isDead) {
                    count += 1;
                }
            }
            deadCount[agentType] = count;
        }
    }

    // パラメータの更新と位置のリセット
    public void ResetAndSet<T>(List<T> agentParametersList) where T : AgentParameters {
        simulationTime = 0;
        for (int agentType = 0; agentType < agents.Count; agentType++) {
            AgentParameters agentParameters = agentParametersList[agentType];
            deadCount[agentType] = 0;
            
            foreach (Agent agent in agents[agentType]) {
                agent.Reset();
                agent.SetParameters(agentParameters);
            }
        }
    }

    // agentSettingをもとにエージェントを配置していく．
    List<Agent> AgentInstantiate(AgentSetting agentSetting, AgentParameters agentParameters) {
        List<Agent> agents = new List<Agent>();
        for (int i = 0; i < agentSetting.nAgent; i++) {
            Vector3 position = Vector3.zero;
            position.x = Random.Range(agentSetting.initialPositionLowerBound.x, agentSetting.initialPositionUpperBound.x);
            position.y = Random.Range(agentSetting.initialPositionLowerBound.y, agentSetting.initialPositionUpperBound.y);
            position.z = Random.Range(agentSetting.initialPositionLowerBound.z, agentSetting.initialPositionUpperBound.z);
            Quaternion rotation = Quaternion.identity;

            Agent newAgent = Instantiate(agentSetting.original, position, rotation).GetComponent<Agent>();
            newAgent.AgentInitialize(agentSetting, agentParameters);
            
            agents.Add(newAgent);
        }

        // すべてのエージェントをactiveにする．
        // ついでに分かりやすい名前をつけておく．
        string agentTag = agentSetting.original.tag;
        string agentLayer = LayerMask.LayerToName(agentSetting.original.gameObject.layer);

        int agentCount = 0;
        foreach (Agent agent in agents) {
            agent.gameObject.SetActive(true);
            agent.transform.name = $"{agentTag} {agentLayer} ({agentCount})";
            agentCount++;
        }

        // エージェントのリストのリストに追加
        // this.agents.Add(agents);
        return agents;
    }

    ////////////////////////////
    // 集団の特徴を計算する関数 //
    ////////////////////////////

    // 集団極性:集団の連帯度合い
    static public float GroupPolarization(List<Agent> agents) {
        int nAgents = 0;
        Vector3 v = Vector3.zero;

        foreach(Agent agent in agents) {
            v += agent.Velocity;
            nAgents++;
        }

        return v.magnitude / nAgents;
    }

    // 集団の重心
    static public Vector3 GroupCenter(List<Agent> agents) {
        int nAgents = 0;
        Vector3 c = Vector3.zero;

        foreach(Agent agent in agents) {
            c += agent.Position;
            nAgents++;
        }

        return c / nAgents;
    }

    // 集団角運動量：集団の重心まわりの回転度合い
    static public float GroupAngularMomentum(List<Agent> agents) {
        int nAgents = 0;
        Vector3 m = Vector3.zero;
        Vector3 c = GroupCenter(agents);

        foreach(Agent agent in agents) {
            m += Vector3.Cross(agent.Position - c, agent.Velocity);
            nAgents++;
        }

        return m.magnitude / nAgents;
    }
    
    // Log用関数
    // .ToString()をoverrideしているのでlogger.Log(gameManager);でこの関数の返り値が記入される.
    public override string ToString() {
        List<string> paramsList = new List<string>();
        List<string> deadCountList = new List<string>();

        for (int agentType = 0; agentType < agents.Count; agentType++) {
            List<string> param = new List<string>();
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] properties = agents[agentType][0].agentParameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var prop in properties) {
                object propValue = prop.GetValue(agents[agentType][0].agentParameters, null);
                param.Add(string.Format("\"{0}\": {1}", prop.Name, propValue));
            }

            deadCountList.Add(string.Format("{0}", deadCount[agentType]));
            paramsList.Add(string.Format("{{{0}}}", string.Join(", ", param)));
        }

        return string.Format("---------------\n[AgentParameters]\n[{0}]\n[AgentManager]\n{{\"simulationTime\": {1}, \"deadCount\": [{2}]}}\n",
               string.Join(", ", paramsList), simulationTime, string.Join(", ", deadCountList));
    }
    
    // 設定自体をログに記入する
    public string SettingsToString() {
        string returnString = "===============\n";
        returnString += string.Format("[AgentManager Settings] tau: {0}\n", tau);
        List<string> nAgentsList = new List<string>();
        for (int agentType = 0; agentType < agents.Count; agentType++) {
            nAgentsList.Add(string.Format("[{0}] {1}", agentType, agentSettings[agentType].nAgent));
        }
        returnString += string.Format("[Agent Settings] nAgents: {0}\n", string.Join(" ", nAgentsList));
        returnString += "===============\n";
        return returnString;
    }
}