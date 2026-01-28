using Core.Interfaces;
using Data.Configs;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Audio;
using Infrastructure.Services.PhysicsCalculation;
using UnityEngine;
using Zenject;

namespace Physics
{
    /// <summary>
    /// Handles projectile physics, collision detection, damage dealing, visual effects and impact sounds.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class PhysicsProjectile : MonoBehaviour
    {
        private Rigidbody _rb;
        private IAerodynamicsCalculationService _calculator;
        private IGameObjectFactory _factory;
        private IAudioService _audioService;
        
        private float _mass;
        private float _radius;
        private float _dragCoefficient;
        private float _airDensity;
        private Vector3 _wind;
        private float _area;
        private float _damage;
        private bool _isLaunched;
        private PlantData _config;

        [Inject]
        public void Construct(IAerodynamicsCalculationService calculator, IGameObjectFactory factory, IAudioService audioService)
        {
            _calculator = calculator;
            _factory = factory;
            _audioService = audioService;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Initialize(float mass, float radius, float dragCoeff, float airDensity, Vector3 wind, Vector3 initialVelocity, float damage, PlantData config)
        {
            _mass = Mathf.Max(0.001f, mass);
            _radius = Mathf.Max(0.001f, radius);
            _dragCoefficient = Mathf.Max(0f, dragCoeff);
            _airDensity = Mathf.Max(0f, airDensity);
            _wind = wind;
            _damage = damage;
            _config = config;
            
            _area = _calculator.CalculateCrossSectionArea(_radius);
            
            _rb.mass = _mass;
            _rb.linearDamping = 0f; 
            _rb.useGravity = true;
            _rb.linearVelocity = initialVelocity;
            
            _isLaunched = true;
        }

        private void FixedUpdate()
        {
            if (!_isLaunched) return;

            Vector3 dragForce = _calculator.CalculateDragForce(_rb.linearVelocity, _airDensity, _dragCoefficient, _area, _wind);
            _rb.AddForce(dragForce, ForceMode.Force);
        }

        private async void OnCollisionEnter(Collision other)
        {
            // if (_config.impactEffect != null)
            // {
            //     await _factory.InstantiateAsync(_config.impactEffect, other.contacts[0].point, Quaternion.identity);
            // }

            if (_config.hitSound != null)
            {
                AudioSource.PlayClipAtPoint(_config.hitSound, transform.position, _audioService.SfxVolume);
            }

            var damageable = other.gameObject.GetComponent<IDamageable>();

            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(_damage);
            }
            
            Destroy(gameObject);
        }
    }
}