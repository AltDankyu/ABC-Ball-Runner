using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SupersonicWisdomSDK;

public class GoalController : MonoBehaviour
{
    [SerializeField] GameObject canvas_2;
    [SerializeField] GameObject GameManager_Main;    

    private int currentLevel;


    private void OnTriggerEnter(Collider other)
        {
            currentLevel = GameManager_Main.GetComponent<GameManager>()._currentLevel;

            Debug.Log("GOAL_Curent_Level ==" + currentLevel);
            SupersonicWisdom.Api.NotifyLevelCompleted(currentLevel, null);
            currentLevel++;

            // Debug.Log("ゴーール");
            canvas_2.SetActive(true);

        }



}
