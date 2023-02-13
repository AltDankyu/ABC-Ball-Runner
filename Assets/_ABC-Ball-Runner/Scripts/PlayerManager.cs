using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AbcBallRunner
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private BallManager unionedBall;
        [SerializeField] private BallManager splittedBallLeft;
        [SerializeField] private BallManager splittedBallRight;
        [SerializeField] private float speed;
        public float Speed() { return speed; }

        [SerializeField] private float splitSpan;

        [SerializeField] private bool isUnioned;

        [SerializeField] private Transform leftBallPosition;
        [SerializeField] private Transform centerBallPosition;
        [SerializeField] private Transform rightBallPosition;

        [SerializeField] private float tapInterval;


        // UIを表示させるための宣言


        private float coolTime;

        private void Update()
        {
            coolTime -= Time.deltaTime;

            // 前方移動処理
            transform.position += Vector3.forward * speed * Time.deltaTime;

            // タップ判定
            if (coolTime <= 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    coolTime = tapInterval;

                    if (isUnioned)
                    {
                        if (unionedBall.CurrentAlphabet() != 'A')
                        {
                            isUnioned = false;
                            Split();
                        }
                    }
                    else
                    {
                        isUnioned = true;
                        Union();
                    }
                }
            }
        }

        private void Split()
        {
            // アルファベットの更新
            splittedBallLeft.SetAlphabet(unionedBall.PreviousAlphabet());
            splittedBallRight.SetAlphabet(unionedBall.PreviousAlphabet());

            // ボールが移動中だったら止めておく
            StopAllCoroutines();

            // 位置の初期化
            splittedBallLeft.transform.position = centerBallPosition.position;
            splittedBallRight.transform.position = centerBallPosition.position;

            // 移動開始
            StartCoroutine(SplitCoroutine());
        }

        private IEnumerator SplitCoroutine()
        {
            unionedBall.gameObject.SetActive(false);
            splittedBallLeft.gameObject.SetActive(true);
            splittedBallRight.gameObject.SetActive(true);
            splittedBallLeft.PlayEmphasisEffect();
            splittedBallRight.PlayEmphasisEffect();

            var timeElapsed = 0f;

            while (timeElapsed <= splitSpan)
            {
                timeElapsed += Time.deltaTime;

                // 横移動処理
                var leftBallPositionX = Mathf.Lerp(centerBallPosition.position.x, leftBallPosition.position.x, timeElapsed / splitSpan);
                var rightBallPositionX = Mathf.Lerp(centerBallPosition.position.x, rightBallPosition.position.x, timeElapsed / splitSpan);

                splittedBallLeft.transform.position = new Vector3(leftBallPositionX, splittedBallLeft.transform.position.y, splittedBallLeft.transform.position.z);
                splittedBallRight.transform.position = new Vector3(rightBallPositionX, splittedBallRight.transform.position.y, splittedBallRight.transform.position.z);

                // 回転処理
                splittedBallLeft.AddForce(Vector3.left * speed);
                splittedBallRight.AddForce(Vector3.right * speed);

                yield return null;
            }

            // 位置がピッタリ合うように修正する場合はこのようにする（カクツキの調整が必要）
            // splittedBallLeft.transform.position = leftBallPosition.position;
            // splittedBallRight.transform.position = rightBallPosition.position;
        }

        private void Union()
        {
            // アルファベットの更新
            // アルファベットが違った場合は大きい方のアルファベットに。同じ場合は次のアルファベットに。
            if (splittedBallLeft.CurrentAlphabet() == splittedBallRight.CurrentAlphabet())
            {
                unionedBall.SetAlphabet(splittedBallLeft.NextAlphabet());
            }
            else
            {
                var max = Mathf.Max(splittedBallLeft.CurrentAlphabet(), splittedBallRight.CurrentAlphabet());
                unionedBall.SetAlphabet((char)max);
            }

            // ボールが移動中だったら止めておく
            StopAllCoroutines();

            // 移動開始
            StartCoroutine(UnionCoroutine());
        }

        private IEnumerator UnionCoroutine()
        {
            var timeElapsed = 0f;

            while (timeElapsed <= splitSpan)
            {
                timeElapsed += Time.deltaTime;

                // 横移動処理
                var leftBallPositionX = Mathf.Lerp(leftBallPosition.position.x, centerBallPosition.position.x, timeElapsed / splitSpan);
                var rightBallPositionX = Mathf.Lerp(rightBallPosition.position.x, centerBallPosition.position.x, timeElapsed / splitSpan);

                splittedBallLeft.transform.position = new Vector3(leftBallPositionX, splittedBallLeft.transform.position.y, splittedBallLeft.transform.position.z);
                splittedBallRight.transform.position = new Vector3(rightBallPositionX, splittedBallRight.transform.position.y, splittedBallRight.transform.position.z);

                // 回転処理
                splittedBallLeft.AddForce(Vector3.left * speed);
                splittedBallRight.AddForce(Vector3.right * speed);

                yield return null;
            }

            // 位置がピッタリ合うように修正する
            splittedBallLeft.transform.position = centerBallPosition.position;
            splittedBallRight.transform.position = centerBallPosition.position;

            // ボールの表示
            unionedBall.gameObject.SetActive(true);
            splittedBallLeft.gameObject.SetActive(false);
            splittedBallRight.gameObject.SetActive(false);

            unionedBall.PlayEmphasisEffect();
        }

        private void Upgrade()
        {

        }

        private void Downgrade()
        {

        }
    }
}