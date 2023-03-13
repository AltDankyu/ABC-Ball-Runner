using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphabetController : MonoBehaviour
{
    [SerializeField] private GameObject[] alphabet_balls; // 表示するモデルの配列

    private int currentModelIndex = 0; // 現在表示しているモデルのインデックス
    private float timePassed = 0f; // 前回のモデル切り替えからの経過時間
    public float switchTime = 0.1f; // モデルを切り替える間隔（秒）


    void Start () {
        // 最初に表示するモデルを有効化する
        alphabet_balls[currentModelIndex].SetActive(true);
    }

    void Update () {
        // 前回のモデル切り替えからの経過時間を増加する
        timePassed += Time.deltaTime;

        // モデルを切り替えるタイミングかどうかを確認する
        if (timePassed >= switchTime) {
            // 現在のモデルを無効化する
            alphabet_balls[currentModelIndex].SetActive(false);

            // 配列内の次のモデルに移動する
            currentModelIndex = (currentModelIndex + 1) % alphabet_balls.Length;

            // 次のモデルを有効化する
            alphabet_balls[currentModelIndex].SetActive(true);

            // 経過時間をリセットする
            timePassed = 0f;
        }
    }


}
