using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using System.IO;

struct Particle
{
    public List<float> x;
    public float fx;
    public List<float> p;
    public float fp;
    public List<float> v;

    public Particle(List<float> initx, float initfx, List<float> initp, float initfp, List<float> initv) {
        x = initx;
        fx = initfx;
        p = initp;
        fp = initfp;
        v = initv;
    }
}

public class PSOEnvironment : MonoBehaviour {
    [Header("Manager"), SerializeField] private AgentManager manager = null;
    private AgentManager gameManager => manager;
    
    public static CouzinAgentParameters preyParameters = new CouzinAgentParameters();
    public static CouzinAgentParameters predatorParameters = new CouzinAgentParameters();
    
    [HideInInspector]
    public List<CouzinAgentParameters> couzinAgentParametersList;
    ILogger logger;

    // 初回に1回だけ呼ばれる関数
    void Start() {
        logger = new Logger(new logToFileHandler(System.DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")));

        preyParameters = new CouzinAgentParameters();
        predatorParameters = new CouzinAgentParameters();

        couzinAgentParametersList = new List<CouzinAgentParameters> {
            preyParameters,
            predatorParameters
        };

        preyParameters.SetDefaultPreyValue();
        predatorParameters.SetDefaultPredatorValue();        
        gameManager.Initialize(couzinAgentParametersList);

        logger.Log(gameManager.SettingsToString());
        StartCoroutine(PSO());
    }
    
    // Unityによって初回に1回だけ呼ばれる関数
    void FixedUpdate() {
        gameManager.Update();
    }

    /* --------------------------------------------------------------------- */
    // PSOでzooとzoaを最適化
    // https://en.wikipedia.org/wiki/Particle_swarm_optimization の疑似コード

    IEnumerator PSO() {
        // ハイパーパラメータ
        int n = 2; // 2次元
        List<float> b_lo = new List<float> {0.0f, 0.0f};
        List<float> b_up = new List<float> {15.0f, 15.0f};
        int S = 10;
        float w = 0.6f;
        float phi_p = 2.0f;
        float phi_g = 2.0f;
        
        List<Particle> particles = new List<Particle> ();

        // 最も成績が良い座標を持つ粒子のidx
        // (疑似コードのgはidxではなく座標であり異なる。座標はparticles[g].fpで得られる)
        int g = -1;

        for (int i = 0; i < S; i++) {
            // x_i ~ U(b_lo, b_up)
            List<float> x = new List<float> {
                UnityEngine.Random.Range(b_lo[0], b_up[0]),
                UnityEngine.Random.Range(b_lo[1], b_up[1])
            };

            List<float> p = x;
            
            // f(p)を計算
            float fp = 0.0f;
            yield return f(p, result => { fp = result; });
            
            if (g == -1 || fp < particles[g].fp) {
                g = i;
            }
            
            // v_i ~ U(-|b_up - b_lo|, |b_up - b_lo|)
            List<float> v = new List<float> {
                UnityEngine.Random.Range(-Math.Abs(b_up[0] - b_lo[0]), Math.Abs(b_up[0] - b_lo[0])),
                UnityEngine.Random.Range(-Math.Abs(b_up[1] - b_lo[1]), Math.Abs(b_up[1] - b_lo[1]))
            };
           
            Particle particle = new Particle() {
                x = x,
                fx = fp,
                p = x,
                fp = fp,
                v = v
            };
            particles.Add(particle);
            
        }
       
        // termination criteriaは指定していないので適宜止める
        while (true) {
            for (int i = 0; i < S; i++) {
                List<float> x_i = particles[i].x;
                float fx_i = particles[i].fx;
                List<float> p_i = particles[i].p;
                float fp_i = particles[i].fp;
                List<float> v_i = particles[i].v;

                for (int d = 0; d < n; d++) {
                    // r_p, r_g ~ U(0, 1)
                    float rp = UnityEngine.Random.Range(0.0f, 1.0f);
                    float rg = UnityEngine.Random.Range(0.0f, 1.0f);
                    
                    v_i[d] = w * v_i[d] + phi_p * rp * (p_i[d] - x_i[d]) + phi_g * rg * (particles[g].p[d] - x_i[d]);
                }
                
                // x_i <- x_i + v_i
                bool inside = true;
                List<float> new_x = new List<float>() {0.0f, 0.0f};
                for (int d = 0; d < n; d++) {
                    new_x[d] = x_i[d] + v_i[d];
                    // 値の上限や下限を超えないか確認
                    if (new_x[d] <= b_lo[d]) {
                        inside = false;
                        break;
                    } else if (new_x[d] >= b_up[d]) {
                        inside = false;
                        break;
                    }
                }
                x_i = new_x;

                // 範囲外に出た場合は初期化(ここは疑似コードと異なる)
                if (!inside) {
                    // x_i ~ U(b_lo, b_up)
                    x_i = new List<float> {
                        UnityEngine.Random.Range(b_lo[0], b_up[0]),
                        UnityEngine.Random.Range(b_lo[1], b_up[1])
                    };
                    
                    p_i = x_i;
                    
                    // v_i ~ U(-|b_up - b_lo|, |b_up - b_lo|)
                    v_i = new List<float> {
                        UnityEngine.Random.Range(-Math.Abs(b_up[0] - b_lo[0]), Math.Abs(b_up[0] - b_lo[0])),
                        UnityEngine.Random.Range(-Math.Abs(b_up[1] - b_lo[1]), Math.Abs(b_up[1] - b_lo[1]))
                    };
                }
                
                // f(x)を計算
                yield return f(x_i, result => {fx_i = result; });
                
                if (fx_i < fp_i) {
                    p_i = x_i;
                    fp_i = fx_i;
                    if (fp_i < particles[g].fp) {
                        g = i;
                    }
                }
                
                Particle updatedParticle = new Particle() {
                    x = x_i,
                    fx = fx_i,
                    p = p_i,
                    fp = fp_i,
                    v = v_i
                };
                particles[i] = updatedParticle;
           }
        }
    }

    IEnumerator f(List<float> x, Action<float> callback) {
        preyParameters.zoo = x[0];
        preyParameters.zoa = x[1];
        int nSamples = 1;
        int result = 0;
        // nSamples回平均
        for (int i = 0; i < nSamples; i++) {
            gameManager.ResetAndSet(couzinAgentParametersList);
            // シミュレーション時間が80を超えるまで待つ
            while (gameManager.simulationTime < 120) {
                yield return null;
            }
            result += gameManager.deadCount[0];
            logger.Log(gameManager);
        }
        callback((float) result / (float) nSamples);
    }
}
