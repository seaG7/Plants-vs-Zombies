using Infrastructure.Factories.Objects;
using Infrastructure.Services.Input;
using Physics;
using UnityEngine;
using Zenject;

namespace Features.Cannon
{
    /// <summary>
    /// Controls static plant rotation (WASD) and firing (Space).
    /// </summary>
    public class CannonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _horizontalAxis;
        [SerializeField] private Transform _verticalAxis;
        [SerializeField] private TrajectoryVisualizer _visualizer;
        
        [Header("Settings")]
        [SerializeField] private string _projectileAddress;
        [SerializeField] private float _rotationSpeed = 45f;
        [SerializeField] private float _initialSpeed = 20f;

        [SerializeField] private float _minPitch = -10f;
        [SerializeField] private float _maxPitch = 45f;
        [SerializeField] private float _minYaw = -45f;
        [SerializeField] private float _maxYaw = 45f;
        
        [Header("Physics Config")]
        [SerializeField] private float _mass = 2f;
        [SerializeField] private float _radius = 0.2f;
        [SerializeField] private float _dragCoeff = 0.47f;
        [SerializeField] private float _airDensity = 1.225f;
        [SerializeField] private Vector3 _wind = Vector3.zero;

        private IInputService _inputService;
        private IGameObjectFactory _factory;
        private bool _isActive;

        // Текущие углы для клампа
        private float _currentYaw;
        private float _currentPitch;

        [Inject]
        public void Construct(IInputService inputService, IGameObjectFactory factory)
        {
            _inputService = inputService;
            _factory = factory;
        }

        private void Start()
        {
            _isActive = true; 
            _inputService.OnFirePerformed += HandleFireInput;
        }

        private void OnDestroy()
        {
            if (_inputService != null)
                _inputService.OnFirePerformed -= HandleFireInput;
        }

        private void Update()
        {
            if (!_isActive) return;

            HandleAiming();
            UpdateTrajectory();
        }

        public void SetControlActive(bool isActive)
        {
            _isActive = isActive;
            if (!_isActive)
            {
                _visualizer.Clear();
            }
        }

        private void HandleAiming()
        {
            Vector2 input = _inputService.GetAimInput();

            if (input.sqrMagnitude > Mathf.Epsilon)
            {
                // Horizontal (A/D) -> Yaw (Y axis)
                float yawDelta = input.x * _rotationSpeed * Time.deltaTime;
                _currentYaw = Mathf.Clamp(_currentYaw + yawDelta, _minYaw, _maxYaw);
                _horizontalAxis.localRotation = Quaternion.Euler(0, _currentYaw, 0);

                // Vertical (W/S) -> Pitch (X axis)
                // Обычно W тянет ствол вверх (отрицательный угол в Unity или положительный, зависит от модели)
                // Допустим, W (input.y > 0) поднимает ствол (уменьшает X euler если ось стандартная)
                float pitchDelta = -input.y * _rotationSpeed * Time.deltaTime; 
                _currentPitch = Mathf.Clamp(_currentPitch + pitchDelta, _minPitch, _maxPitch);
                _verticalAxis.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
            }
        }

        private void UpdateTrajectory()
        {
            Vector3 startVel = _muzzle.forward * _initialSpeed;
            _visualizer.SimulateTrajectory(_muzzle.position, startVel, _mass, _radius, _dragCoeff, _airDensity, _wind);
        }

        private void HandleFireInput()
        {
            if (_isActive)
                Fire();
        }

        private async void Fire()
        {
            float currentMass = Random.Range(_mass * 0.95f, _mass * 1.05f);

            var projectileObj = await _factory.InstantiateAsync(_projectileAddress, _muzzle.position, Quaternion.identity);
            var physicsComp = projectileObj.GetComponent<PhysicsProjectile>();
            
            if (physicsComp != null)
            {
                Vector3 startVel = _muzzle.forward * _initialSpeed;
                physicsComp.Initialize(currentMass, _radius, _dragCoeff, _airDensity, _wind, startVel);
            }
        }
    }
}