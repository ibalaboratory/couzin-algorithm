using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using System.IO;

public class ObserveEnvironment : MonoBehaviour {
    [Header("Manager"), SerializeField] private AgentManager manager = null;
    private AgentManager gameManager => manager;
    
    [Header("Types"), SerializeField] public int types = 2;

    [HideInInspector]
    public List<CouzinAgentParameters> couzinAgentParametersList = new List<CouzinAgentParameters>();

    void Start() {
        for (int i = 0; i < types; i++) {
            couzinAgentParametersList.Add(new CouzinAgentParameters());
            couzinAgentParametersList[i].SetDefaultPreyValue();
        }
        couzinAgentParametersList[types - 1].SetDefaultPredatorValue();
        gameManager.Initialize(couzinAgentParametersList);
    }
    
    void FixedUpdate() {
        gameManager.Update();
    }
}
