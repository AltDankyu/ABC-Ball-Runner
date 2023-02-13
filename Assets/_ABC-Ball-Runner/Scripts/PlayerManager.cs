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


        // UI��\�������邽�߂̐錾


        private float coolTime;

        private void Update()
        {
            coolTime -= Time.deltaTime;

            // �O���ړ�����
            transform.position += Vector3.forward * speed * Time.deltaTime;

            // �^�b�v����
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
            // �A���t�@�x�b�g�̍X�V
            splittedBallLeft.SetAlphabet(unionedBall.PreviousAlphabet());
            splittedBallRight.SetAlphabet(unionedBall.PreviousAlphabet());

            // �{�[�����ړ�����������~�߂Ă���
            StopAllCoroutines();

            // �ʒu�̏�����
            splittedBallLeft.transform.position = centerBallPosition.position;
            splittedBallRight.transform.position = centerBallPosition.position;

            // �ړ��J�n
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

                // ���ړ�����
                var leftBallPositionX = Mathf.Lerp(centerBallPosition.position.x, leftBallPosition.position.x, timeElapsed / splitSpan);
                var rightBallPositionX = Mathf.Lerp(centerBallPosition.position.x, rightBallPosition.position.x, timeElapsed / splitSpan);

                splittedBallLeft.transform.position = new Vector3(leftBallPositionX, splittedBallLeft.transform.position.y, splittedBallLeft.transform.position.z);
                splittedBallRight.transform.position = new Vector3(rightBallPositionX, splittedBallRight.transform.position.y, splittedBallRight.transform.position.z);

                // ��]����
                splittedBallLeft.AddForce(Vector3.left * speed);
                splittedBallRight.AddForce(Vector3.right * speed);

                yield return null;
            }

            // �ʒu���s�b�^�������悤�ɏC������ꍇ�͂��̂悤�ɂ���i�J�N�c�L�̒������K�v�j
            // splittedBallLeft.transform.position = leftBallPosition.position;
            // splittedBallRight.transform.position = rightBallPosition.position;
        }

        private void Union()
        {
            // �A���t�@�x�b�g�̍X�V
            // �A���t�@�x�b�g��������ꍇ�͑傫�����̃A���t�@�x�b�g�ɁB�����ꍇ�͎��̃A���t�@�x�b�g�ɁB
            if (splittedBallLeft.CurrentAlphabet() == splittedBallRight.CurrentAlphabet())
            {
                unionedBall.SetAlphabet(splittedBallLeft.NextAlphabet());
            }
            else
            {
                var max = Mathf.Max(splittedBallLeft.CurrentAlphabet(), splittedBallRight.CurrentAlphabet());
                unionedBall.SetAlphabet((char)max);
            }

            // �{�[�����ړ�����������~�߂Ă���
            StopAllCoroutines();

            // �ړ��J�n
            StartCoroutine(UnionCoroutine());
        }

        private IEnumerator UnionCoroutine()
        {
            var timeElapsed = 0f;

            while (timeElapsed <= splitSpan)
            {
                timeElapsed += Time.deltaTime;

                // ���ړ�����
                var leftBallPositionX = Mathf.Lerp(leftBallPosition.position.x, centerBallPosition.position.x, timeElapsed / splitSpan);
                var rightBallPositionX = Mathf.Lerp(rightBallPosition.position.x, centerBallPosition.position.x, timeElapsed / splitSpan);

                splittedBallLeft.transform.position = new Vector3(leftBallPositionX, splittedBallLeft.transform.position.y, splittedBallLeft.transform.position.z);
                splittedBallRight.transform.position = new Vector3(rightBallPositionX, splittedBallRight.transform.position.y, splittedBallRight.transform.position.z);

                // ��]����
                splittedBallLeft.AddForce(Vector3.left * speed);
                splittedBallRight.AddForce(Vector3.right * speed);

                yield return null;
            }

            // �ʒu���s�b�^�������悤�ɏC������
            splittedBallLeft.transform.position = centerBallPosition.position;
            splittedBallRight.transform.position = centerBallPosition.position;

            // �{�[���̕\��
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