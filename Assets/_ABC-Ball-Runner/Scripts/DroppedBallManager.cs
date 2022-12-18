using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbcBallRunner
{
    public class DroppedBallManager : MonoBehaviour
    {
        [SerializeField] private char alphabet;

        public char GetAlphabet()
        {
            return alphabet;
        }
    }
}
