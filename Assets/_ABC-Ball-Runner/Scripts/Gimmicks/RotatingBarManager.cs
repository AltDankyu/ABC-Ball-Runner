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

            // TODO: anchorPointsの子オブジェクトを使用して、transform移動で円を描くように動かす。
        }
    }
}
