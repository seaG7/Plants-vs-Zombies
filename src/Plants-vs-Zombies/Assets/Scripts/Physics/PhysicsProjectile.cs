using Infrastructure.Services.Physics;
using UnityEngine;
using Zenject;

namespace Physics
{
    /// <summary>
    /// Applies quadratic drag forces to the Rigidbody in FixedUpdate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class PhysicsProjectile : MonoBehaviour
    {
        private Rigidbody _rb;
        private IAerodynamicsCalculationService _calculator;
        
        private float _mass;
        private float _radius;
        private float _dragCoefficient;
        private float _airDensity;
        private Vector3 _wind;
        private float _area;
        private bool _isLaunched;

        [Inject]
        public void Construct(IAerodynamicsCalculationService calculator)
        {
            _calculator = calculator;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Initialize(float mass, float radius, float dragCoeff, float airDensity, Vector3 wind, Vector3 initialVelocity)
        {
            _mass = Mathf.Max(0.001f, mass);
            _radius = Mathf.Max(0.001f, radius);
            _dragCoefficient = Mathf.Max(0f, dragCoeff);
            _airDensity = Mathf.Max(0f, airDensity);
            _wind = wind;
            
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
    }
}