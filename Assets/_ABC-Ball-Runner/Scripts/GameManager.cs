using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using SupersonicWisdomSDK;



public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject canvas_1;
    

    public int _currentLevel = 1;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start_currentLevel == " + _currentLevel);
        canvas_1.SetActive(false);
        SupersonicWisdom.Api.NotifyLevelStarted(_currentLevel, null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Next() {   
        // _currentLevel++;
		SceneManager.LoadScene("SampleScene");

	}

}
