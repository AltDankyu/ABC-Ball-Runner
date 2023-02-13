using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using SupersonicWisdomSDK;
 
public class GameSystem : MonoBehaviour {
 
	//　スタートボタンを押したら実行する
	public void StartGame() {
		SceneManager.LoadScene("SampleScene");
	}

	void Awake(){
		// Subscribe
		SupersonicWisdom.Api.AddOnReadyListener(OnSupersonicWisdomReady);
		// Then initialize
		SupersonicWisdom.Api.Initialize();
	}


	void OnSupersonicWisdomReady()
	{
   		// Start your game from this point
	}    
}