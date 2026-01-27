using System;
using Features.Enemy;
using UnityEngine;

namespace Features.Context
{
    [RequireComponent(typeof(BoxCollider))]
    public class FinishLineTrigger : MonoBehaviour
    {
        public event Action<ZombieController> OnZombieCrossed;

        private void Awake()
        {
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var zombie = other.GetComponent<ZombieController>();
            if (zombie != null && zombie.IsAlive)
            {
                OnZombieCrossed?.Invoke(zombie);
            }
        }
    }
}