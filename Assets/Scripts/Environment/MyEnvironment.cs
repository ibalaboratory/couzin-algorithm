using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using System.IO;

public class MyEnvironment : MonoBehaviour {
    /* ========================================================
    Environmentを設定するにはUnityのEnvironmentコンポーネントの
    インスペクタ上で使いたいEnvironmentにチェックを入れる.

    以下のコードは説明用/改変用のダミーであり一部改変しないと動かないので注意.
    動くEnvironmentについてはAnnealEnvironment.cs, PSOEnvironment.csを参照.
    ======================================================== */

    /* ------------------------------------ */
    // ここでAgentManagerを指定する. 特に変更しなくて良いはず.
    [Header("Manager"), SerializeField] private AgentManager manager = null;
    private AgentManager gameManager => manager;
    /* ------------------------------------ */

    
    /* ------------------------------------ */
    // ファイルに書き込む用のLoggerを指定する. 特に変更しなくて良いはず.
    // デフォルトではデスクトップのCouzinAlgorithmsLogsというフォルダにログが生成される
    // 実装やパス変更はScripts/Other/logToFileHandler.csを参照
    ILogger logger;
    /* ------------------------------------ */

    
    /* ------------------------------------ */
    // AgentのパラメータをまとめたクラスのListを作成する
    // 例) preyとpredatorがいる場合は、それぞれのパラメータを保存した
    //     CouzinAgentParameterインスタンスを作り, それらをListとする.
    // パラメータのセットはStart()の内部で行うため、ここではグローバル変数の宣言のみを行う.
    public static CouzinAgentParameters preyParameters;
    public static CouzinAgentParameters predatorParameters;

    [HideInInspector]
    public List<CouzinAgentParameters> couzinAgentParametersList;
    /* ------------------------------------ */


    /* ------------------------------------ */
    // Unityにより初回に1回だけ呼ばれる関数.
    void Start() {
        // 日時を名前とするログ用のファイルを作成.
        logger = new Logger(new logToFileHandler(System.DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")));
        
        // ここではパラメータにデフォルトの値を設定している.
        // なおデフォルトとして設定されている値はそれなりに良い解なので
        // 最適化アルゴリズムによる改善が見られないことがある.
        // 自分で最適化を行う値については初期値をデフォルトにせず
        // 乱数や0などで初期化することを推奨する.
        // 実装はCouzinAgentParameters.csを参照.
        preyParameters = new CouzinAgentParameters();
        preyParameters.SetDefaultPreyValue();

        predatorParameters = new CouzinAgentParameters();
        predatorParameters.SetDefaultPredatorValue();        
        
        couzinAgentParametersList = new List<CouzinAgentParameters> {
            preyParameters,
            predatorParameters
        };


        // AgentManagerの初期化
        gameManager.Initialize(couzinAgentParametersList);

        // ログ関数
        // gameManager.SettingsToString() はgameManagerの設定をまとめた文字列を返す
        // (詳細はAgentManager.csのSettingsToString())
        logger.Log(gameManager.SettingsToString());

        // MyOptimizationを始める
        // ここでCoroutineを使うのは, シミュレーションが終わるまで最適化計算が進められなく
        // 非同期に処理を行う必要があるからである.
        StartCoroutine(MyOptimization());
    }
    /* ------------------------------------ */


    /* ------------------------------------ */
    // Unityによって毎フレーム呼ばれる関数

    // シミュレーションを現在の状態から1フレーム分進める.
   
    void FixedUpdate() {
        gameManager.Update();
    }
    /* ------------------------------------ */


    /* ------------------------------------ */
    // 何かしらの最適化プログラム
    IEnumerator MyOptimization() {
        // 最適化プログラムはおおよそ
        // while condition:
        //     1. それまでの実験結果から候補となるパラメータxを一つ決める(xはn次元)
        //     2. パラメータxにおける実験結果を得る
        // というよう形になるはず. その雛形を示す.
        // 実際の実装はAnnealEnvironment.cs, PSOEnvironment.csなどを参照.
        
        while (true) {
            // 1
            // このコードでは毎回 x = (0.0, 0.0)
            List<float> x = new List<float>() {0.0f, 0.0f};
            
            // 2
            // xにおける結果を待つ
            float result = 0;
            yield return f(x, response => { result = response; });
        }
    }
    /* ------------------------------------ */


    /* ------------------------------------ */
    // シミュレーションの初期状態とパラメータを設定し, 結果が出るまで待つ関数. 
    // FixedUpdate()を止めることはできないので, まずリセット時の位置や速度情報とパラメータを代入し, 
    // シミュレーションが進んでいる間は最適化の計算は行わずFixedUpdate()を待つ.
    // シミュレーションが目標値に達していると観測したらリセットとパラメータのセットを行い, 
    // 再びシミュレーションが目標値に達するのを待つ, ということを繰り返す.
    // 少し分かりづらいのでREADME.mdやシーケンス図などを参照.
    IEnumerator f(List<float> x, Action<float> callback) {
        // ここでパラメータを設定する
        // 設定できるパラメータはCouzinAgentParameters.csに記載してある
        // 例) preyのパラメータを変える場合.
        preyParameters.zoo = x[0];
        preyParameters.zoa = x[1];

        // 設定したパラメータをgameManagerに受け渡す
        gameManager.ResetAndSet(couzinAgentParametersList);


        // AgentManager.cs内の変数を参照できるので,
        // それを使ってシミュレーションの終了判定や返り値を決める.

        // 終了判定
        // 例1) シミュレーション時間が120を超えるまで待つ場合
        while (gameManager.simulationTime < 120) {
            yield return null;
        }
        // 例2) 人口の50%が死ぬのを待つ場合
        // while (gameManager.deadCount[0] > gameManager.agentSettings.nAgent[0] * 0.5) {
        //     yield return null;
        // }
        
        // response(返り値)
        // 例1) 指定時間内に死んだpreyの数
        callback(gameManager.deadCount[0]);
        // 例2) 終了判定に達するまでにかかった時間
        // callback(gameManager.simulationTime);
    }
}
