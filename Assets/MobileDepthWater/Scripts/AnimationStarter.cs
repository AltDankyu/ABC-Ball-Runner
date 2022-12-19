namespace Assets.MobileOptimizedWater.Scripts
{
    using UnityEngine;

    public class AnimationStarter : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Motion newanimation;

        public void Awake()
        {
            animator.Play(newanimation.name);
        }
    }
}
