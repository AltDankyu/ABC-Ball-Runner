using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SupersonicWisdomSDK;

public class Floor : MonoBehaviour
{

    [SerializeField] private GameObject canvas_fail;


    // 現在のステージを取得
    int nowLevel_2 = GameManager.currentLevel;


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
            SupersonicWisdom.Api.NotifyLevelFailed(nowLevel_2, null);
        }
    }

}
