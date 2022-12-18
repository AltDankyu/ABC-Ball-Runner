using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbcBallRunner
{
    public class RotatingBarManager : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed;
        [SerializeField] private Transform anchorPointsRoot;
        [SerializeField] private float moveDuration;

        private void Update()
        {
            transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));

            // TODO: anchorPoints�̎q�I�u�W�F�N�g���g�p���āAtransform�ړ��ŉ~��`���悤�ɓ������B
        }
    }
}
