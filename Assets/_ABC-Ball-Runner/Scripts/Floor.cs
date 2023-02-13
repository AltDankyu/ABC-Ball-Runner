using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SupersonicWisdomSDK;


public class Floor : MonoBehaviour
{

    private int currentLevel_Fail;

    [SerializeField] private GameObject canvas_fail;
    [SerializeField] GameObject GameManager_Main;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if(other.gameObject.name == "Splitted Ball Left")
        {
            // 失敗処理
            canvas_fail.SetActive(true);

            currentLevel_Fail = GameManager_Main.GetComponent<GameManager>()._currentLevel;
            SupersonicWisdom.Api.NotifyLevelFailed(currentLevel_Fail, null);
        }
    }

}
