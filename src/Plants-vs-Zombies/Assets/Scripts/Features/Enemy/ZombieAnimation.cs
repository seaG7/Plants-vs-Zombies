using UnityEngine;

namespace Features.Enemy
{
    [RequireComponent(typeof(Animator))]
    public class ZombieAnimation : MonoBehaviour
    {
        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        private static readonly int DieHash = Animator.StringToHash("Die");

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void PlayWalk()
        {
            _animator.SetBool(IsWalkingHash, true);
        }

        public void PlayIdle()
        {
            _animator.SetBool(IsWalkingHash, false);
        }

        public void PlayDeath()
        {
            _animator.SetTrigger(DieHash);
        }
        
        public void Stop()
        {
            _animator.speed = 0f;
        }
    }
}