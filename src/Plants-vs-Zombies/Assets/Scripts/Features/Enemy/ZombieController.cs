using System;
using Core.Interfaces;
using UnityEngine;

namespace Features.Enemy
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(ZombieAnimation))]
    public class ZombieController : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _moveSpeed = 1.5f;

        public event Action OnDeath;
        public event Action<float> OnHealthChanged;

        private ZombieAnimation _animation;
        private float _currentHealth;
        private bool _isAlive;
        private CharacterController _characterController;
        private Vector3 _moveDirection = Vector3.back; 

        public bool IsAlive => _isAlive;

        private void Awake()
        {
            _animation = GetComponent<ZombieAnimation>();
            _characterController = GetComponent<CharacterController>();
        }

        public void Initialize()
        {
            _isAlive = true;
            _animation.PlayWalk();
        }

        private void Update()
        {
            if (!_isAlive) return;
            Move();
        }

        public void StopMovement()
        {
            _isAlive = false;
            _characterController.enabled = false;
            _animation.Stop();
        }

        public void TakeDamage(float damage)
        {
            if (!_isAlive) return;

            _currentHealth -= damage;
            OnHealthChanged?.Invoke(_currentHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Move()
        {
            _characterController.Move(_moveDirection * _moveSpeed * Time.deltaTime);
        }

        private void Die()
        {
            _isAlive = false;
            _animation.PlayDeath();
            OnDeath?.Invoke();
            
            _characterController.enabled = false;
            Destroy(gameObject, 5f); 
        }
    }
}