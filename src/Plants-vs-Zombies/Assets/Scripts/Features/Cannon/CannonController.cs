using Cysharp.Threading.Tasks;
using Data.Configs;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Input;
using Physics;
using UnityEngine;
using Zenject;

namespace Features.Cannon
{
    /// <summary>
    /// Controls static plant rotation and firing using injected data.
    /// </summary>
    public class CannonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _horizontalAxis;
        [SerializeField] private Transform _verticalAxis;
        [SerializeField] private TrajectoryVisualizer _visualizer;
        [SerializeField] private Transform _cameraMountPoint; 
        
        public Transform CameraMountPoint => _cameraMountPoint;

        private IInputService _inputService;
        private IGameObjectFactory _factory;
        private PlantData _config;
        
        private bool _isPossessed;
        private float _currentYaw;
        private float _currentPitch;

        [Inject]
        public void Construct(IInputService inputService, IGameObjectFactory factory)
        {
            _inputService = inputService;
            _factory = factory;
        }

        public void Initialize(PlantData config)
        {
            _config = config;
        }

        private void Start()
        {
            _inputService.OnFirePerformed += HandleFireInput;
            SetPossessed(false);
        }

        private void OnDestroy()
        {
            if (_inputService != null)
                _inputService.OnFirePerformed -= HandleFireInput;
        }

        private void Update()
        {
            if (!_isPossessed || _config == null) return;

            HandleAiming();
            UpdateTrajectory();
        }
        
        public void SetPossessed(bool isPossessed)
        {
            _isPossessed = isPossessed;
            if (!_isPossessed)
            {
                _visualizer.Clear();
            }
        }

        private void HandleAiming()
        {
            Vector2 input = _inputService.GetAimInput();

            if (input.sqrMagnitude > Mathf.Epsilon)
            {
                float yawDelta = input.x * _config.rotationSpeed * Time.deltaTime;
                _currentYaw = Mathf.Clamp(_currentYaw + yawDelta, _config.minYaw, _config.maxYaw);
                _horizontalAxis.localRotation = Quaternion.Euler(0, _currentYaw, 0);
                
                float pitchDelta = -input.y * _config.rotationSpeed * Time.deltaTime; 
                _currentPitch = Mathf.Clamp(_currentPitch + pitchDelta, _config.minPitch, _config.maxPitch);
                _verticalAxis.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
            }
        }

        private void UpdateTrajectory()
        {
            Vector3 startVel = _muzzle.forward * _config.initialSpeed;
            _visualizer.SimulateTrajectory(_muzzle.position, startVel, _config.projectileMass, 
                _config.projectileRadius, _config.dragCoeff, _config.airDensity, _config.wind);
        }

        private void HandleFireInput()
        {
            if (_isPossessed && _config != null)
                Fire();
        }

        private async void Fire()
        {
            float currentMass = Random.Range(_config.projectileMass * 0.95f, _config.projectileMass * 1.05f);

            var projectileObj = await _factory.InstantiateAsync(_config.projectileAsset, _muzzle.position, Quaternion.identity);
            var physicsComp = projectileObj.GetComponent<PhysicsProjectile>();
            
            if (physicsComp != null)
            {
                Vector3 startVel = _muzzle.forward * _config.initialSpeed;
                physicsComp.Initialize(currentMass, _config.projectileRadius, _config.dragCoeff, 
                    _config.airDensity, _config.wind, startVel);
            }
        }
    }
}