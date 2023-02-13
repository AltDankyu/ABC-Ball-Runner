using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalLineController : MonoBehaviour
{

    [SerializeField] private GameObject Goal_UI;

    // 現在のステージ数を取得するための関数
    [SerializeField] private GameObject GameManager_1;

    // 今のステージ数を取得
    int nowLevel_1 = GameManager.currentLevel;


    private void OnTriggerEnter(Collider other)
    {        
        Goal_UI.SetActive(true);
        
        // ここにCompleteタグを書けばOK?
        Debug.Log("currentLevel_Goal == "+ nowLevel_1);
    }

}
