using System;
using Core.Interfaces;
using Data.Configs;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Audio;
using UnityEngine;
using Zenject;

namespace Features.Enemy
{
    /// <summary>
    /// Controls zombie behavior, movement, health, effects and audio on spawn/death.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(ZombieAnimation))]
    public class ZombieController : MonoBehaviour, IDamageable
    {
        private ZombieAnimation _animation;
        private CharacterController _characterController;
        private IGameObjectFactory _factory;
        private IAudioService _audioService;
        private EnemyData _data;
        
        private float _currentHealth;
        private bool _isAlive;
        private Vector3 _moveDirection = Vector3.back; 

        public bool IsAlive => _isAlive;
        public event Action OnDeath;
        public event Action<float> OnHealthChanged;

        [Inject]
        public void Construct(IGameObjectFactory factory, IAudioService audioService)
        {
            _factory = factory;
            _audioService = audioService;
        }

        private void Awake()
        {
            _animation = GetComponent<ZombieAnimation>();
            _characterController = GetComponent<CharacterController>();
        }

        public async void Initialize(EnemyData data)
        {
            _data = data;
            _currentHealth = _data.maxHealth;
            _isAlive = true;
            _animation.PlayWalk();
            
            if (_data.spawnEffect != null && _data.spawnEffect.RuntimeKey != null)
            {
                await _factory.InstantiateAsync(_data.spawnEffect, transform.position, Quaternion.identity);
            }

            if (_data.spawnSound != null)
            {
                AudioSource.PlayClipAtPoint(_data.spawnSound, transform.position, _audioService.SfxVolume);
            }
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
            _characterController.Move(_moveDirection * _data.moveSpeed * Time.deltaTime);
        }

        private void Die()
        {
            _isAlive = false;
            _animation.PlayDeath();
            OnDeath?.Invoke();

            if (_data.deathSound != null)
            {
                AudioSource.PlayClipAtPoint(_data.deathSound, transform.position, _audioService.SfxVolume);
            }
            
            _characterController.enabled = false;
            Destroy(gameObject, 5f); 
        }
    }
}