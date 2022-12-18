using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AbcBallRunner
{
    public class BallManager : MonoBehaviour
    {
        [SerializeField] PlayerManager playerManager;
        [SerializeField] private GameObject[] balls;
        [SerializeField] private float rotationBias;

        private Rigidbody _rigidbody;

        public Rigidbody GetRigidbody()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            return _rigidbody;
        }

        private void Update()
        {
            // 垂直方向分の回転処理（RigidbodyのFreezePositionにチェックが入っているから位置移動はしない）
            GetRigidbody().AddForce(Vector3.forward * playerManager.Speed() * Time.deltaTime * rotationBias);
        }

        public void AddForce(Vector3 force)
        {
            // 回転処理（RigidbodyのFreezePositionにチェックが入っているから位置移動はしない）
            GetRigidbody().AddForce(force * Time.deltaTime * rotationBias);
        }

        public char CurrentAlphabet()
        {
            for (var i = 0; i < balls.Length; i++)
            {
                if (balls[i].activeSelf)
                {
                    return ConvertFromIndexToAlphabet(i);
                }
            }

            throw new NotImplementedException();
        }

        public char PreviousAlphabet()
        {
            var currentAlphabet = CurrentAlphabet();

            if (currentAlphabet == 'A')
            {
                return 'A';
            }
            else
            {
                return (char)(currentAlphabet - 1);
            }
        }

        public char NextAlphabet()
        {
            var currentAlphabet = CurrentAlphabet();

            if (currentAlphabet == 'Z')
            {
                return 'Z';
            }
            else
            {
                return (char)(currentAlphabet + 1);
            }
        }

        public int ConvertFromAlphabetToIndex(char alphabet)
        {
            return alphabet - 65;
        }

        public char ConvertFromIndexToAlphabet(int index)
        {
            return (char)(index + 65);
        }

        public void SetAlphabet(char alphabet)
        {
            for (var i = 0; i < balls.Length; i++)
            {
                balls[i].SetActive(false);
            }

            var index = ConvertFromAlphabetToIndex(alphabet);
            balls[index].SetActive(true);
        }

        private void Upgrade()
        {
            var currentAlphabet = CurrentAlphabet();

            if (currentAlphabet == 'Z')
            {
                return;
            }
            else
            {
                SetAlphabet(NextAlphabet());
                PlayEmphasisEffect();
            }
        }

        private void Downgrade()
        {
            var currentAlphabet = CurrentAlphabet();

            if (currentAlphabet == 'A')
            {
                return;
            }
            else
            {
                // 今のボールを置き去りにする
                for (var i = 0; i < balls.Length; i++)
                {
                    if (balls[i].activeSelf)
                    {
                        var ball = Instantiate(balls[i]);
                        ball.transform.position = balls[i].transform.position;
                    }
                }

                SetAlphabet(PreviousAlphabet());
                StartCoroutine(PopCoroutine());
            }
        }

        private IEnumerator PopCoroutine()
        {
            var duration = 0.15f;

            while (duration > 0)
            {
                transform.position += Vector3.up * 0.03f;
                duration -= Time.deltaTime;
                yield return null;
            }
        }

        public void PlayEmphasisEffect()
        {
            StartCoroutine(EmphasisEffectCoroutine());
        }

        private IEnumerator EmphasisEffectCoroutine()
        {
            var timeElapsed = 0f;
            var buffer = transform.localScale;

            while (timeElapsed < 0.2f)
            {
                transform.localScale += Vector3.one * 0.005f;
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            timeElapsed = 0;

            while (timeElapsed < 0.2f)
            {
                transform.localScale -= Vector3.one * 0.005f;
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = buffer;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var droppedBallManager = collision.gameObject.GetComponent<DroppedBallManager>();

            if (droppedBallManager != null)
            {
                if (CurrentAlphabet() == droppedBallManager.GetAlphabet())
                {
                    Destroy(droppedBallManager.gameObject);
                    Upgrade();
                }

                return;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var rotatingBarManager = other.gameObject.GetComponent<RotatingBarManager>();

            if (rotatingBarManager != null)
            {
                Downgrade();

                return;
            }
        }
    }
}
