using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using System.IO;

public class AnnealEnvironment : MonoBehaviour {
    [Header("Manager"), SerializeField] private AgentManager manager = null;
    private AgentManager gameManager => manager;

    public static CouzinAgentParameters prayParameters = new CouzinAgentParameters();
    public static CouzinAgentParameters predatorParameters = new CouzinAgentParameters();

    [HideInInspector]
    public List<CouzinAgentParameters> couzinAgentParametersList;
    ILogger logger;

    // Unityによって初回に1回だけ呼ばれる関数
    void Start() {
        logger = new Logger(new logToFileHandler(System.DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")));
        
        prayParameters = new CouzinAgentParameters();
        prayParameters.SetDefaultPreyValue();

        predatorParameters = new CouzinAgentParameters();
        predatorParameters.SetDefaultPredatorValue();        
        
        couzinAgentParametersList = new List<CouzinAgentParameters>() {
            prayParameters,
            predatorParameters
        };

        gameManager.Initialize(couzinAgentParametersList);

        logger.Log(gameManager.SettingsToString());
        StartCoroutine(SimulatedAnneal());
    }
    
    // Unityによって毎フレーム(正確にはTime.fixedDeltaTime)実行される関数
    void FixedUpdate() {
        gameManager.Update();
    }

    /* --------------------------------------------------------------------- */
    // 1次元焼きなまし法でzooを最適化
    // https://ja.wikipedia.org/wiki/焼きなまし法 の疑似コードほぼそのまま

    IEnumerator SimulatedAnneal() {
        int maxIter = 500;

        float state = UnityEngine.Random.Range(0.0f, 15.0f);
        float e = 0;
        float bestState = state;
        float bestE = e;
        
        for (int iter = 0; iter < maxIter; iter++) {
            float nextState = NEIGHBOUR(state);
            
            float nextE = 0.0f;
            yield return EVAL(nextState, result => { nextE = result; });

            if (nextE < bestE) {
                bestState = nextState;
                bestE = nextE;
            }
            if (UnityEngine.Random.Range(0.0f, 1.0f) <= PROBABILITY(e, nextE, TEMPERATURE((float) iter / (float) maxIter))) {
                state = nextState;
                e = nextE;
            }
        }
    }

    IEnumerator EVAL(float state, Action<float> callback) {
        prayParameters.speed = state;

        int nSamples = 20;
        float e = 0;
        // 20回平均
        for (int i = 0; i < nSamples; i++) {
            gameManager.ResetAndSet(couzinAgentParametersList);
            // シミュレーション時間が80を超えるまで待つ
            while (gameManager.simulationTime < 50) {
                yield return null;
            }
            logger.Log(gameManager);
            e += gameManager.deadCount[0];
        }
        e /= nSamples;
        callback(e);
    }

    float NEIGHBOUR(float state) {
        if (UnityEngine.Random.Range(-1.0f, 1.0f) > 0) {
            return state + 1.0f;
        } else {
            return state - 1.0f;
        }
    }

    float TEMPERATURE(float r) {
        float alpha = 0.5f;
        return (float) Math.Pow(alpha, r);
    }

    float PROBABILITY(float e1, float e2, float t) {
        if (e1 >= e2) {
            return 1.0f;
        } else {
            return Mathf.Exp((e1 - e2) / t);
        }
    }
}