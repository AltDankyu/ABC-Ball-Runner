using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbcBallRunner
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private Transform player;

        [SerializeField] private Vector3 _offset;

        private void Awake()
        {
            // _offset = transform.position - player.position;
        }

        private void Update()
        {
            transform.position = player.position + _offset;
        }
    }
}
