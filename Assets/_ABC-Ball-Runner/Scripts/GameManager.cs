using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SupersonicWisdomSDK;


public class GameManager : MonoBehaviour
{

    // UIの表示に関する宣言
    [SerializeField] private GameObject MainCanvas;
    [SerializeField] private GameObject Start;
    [SerializeField] private GameObject Goal;


    public static int currentLevel = 1;




    void Awake()
    {
        // Subscribe
        SupersonicWisdom.Api.AddOnReadyListener(OnSupersonicWisdomReady);
        // Then initialize
        SupersonicWisdom.Api.Initialize();
    }




    // Update is called once per frame
    void Update()
    {

    }

    public void PushNext()
    {
        // NEXTボタンを押したときに現ステージ数に1を加算。
        AddCurrentLevel();
        SceneManager.LoadScene("SampleScene");
    }


    public void PushStart()
    {
        Start.SetActive(false);
        // ここにStartタグを書けばOK?

        SupersonicWisdom.Api.NotifyLevelStarted(currentLevel, null);
        Debug.Log("Start_currentLevel == "+ currentLevel);
    }

    public static void AddCurrentLevel()
    {
        currentLevel++;
    }


    void OnSupersonicWisdomReady()
    {
        // Start your game from this point
    }





}
