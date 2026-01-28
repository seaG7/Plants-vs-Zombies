using System;
using Core.Interfaces;
using Data.Configs;
using Infrastructure.Factories.Objects;
using Infrastructure.Services.Audio;
using Infrastructure.Services.Input;
using Physics;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Features.Plants
{
    /// <summary>
    /// Controls the Cannon plant, including aiming, firing, and playing fire sounds.
    /// </summary>
    public class CannonController : MonoBehaviour, IPossessablePlant
    {
        [Header("References")]
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _horizontalAxis;
        [SerializeField] private Transform _verticalAxis;
        [SerializeField] private TrajectoryVisualizer _visualizer;
        [SerializeField] private Transform _cameraMountPoint; 
        
        public Transform CameraMountPoint => _cameraMountPoint;

        public event Action OnFireSuccess;
        public event Action OnFireFailedCooldown;

        private IInputService _inputService;
        private IGameObjectFactory _factory;
        private IAudioService _audioService;
        private PlantData _config;
        
        private bool _isPossessed;
        private float _currentYaw;
        private float _currentPitch;
        
        private float _lastFireTime = -999f;

        [Inject]
        public void Construct(IInputService inputService, IGameObjectFactory factory, IAudioService audioService)
        {
            _inputService = inputService;
            _factory = factory;
            _audioService = audioService;
        }

        public void Initialize(PlantData config)
        {
            _config = config;
            if (_config.trajectoryMaterial != null)
            {
                _visualizer.SetMaterial(_config.trajectoryMaterial);
                _visualizer.SetWidth(_config.trajectoryWidth);
            }
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
        
        public float GetReloadProgress()
        {
            float timeSinceFire = Time.time - _lastFireTime;
            return Mathf.Clamp01(timeSinceFire / _config.fireCooldown);
        }

        public bool IsReadyToFire() => GetReloadProgress() >= 1f;

        private void HandleAiming()
        {
            Vector2 keyInput = _inputService.GetAimInput();
            Vector2 mouseInput = _inputService.GetLookDelta();
            
            float inputX = (keyInput.x * _config.rotationSpeed * Time.deltaTime) + (mouseInput.x * _config.mouseSensitivity);
            float inputY = (keyInput.y * _config.rotationSpeed * Time.deltaTime) + (mouseInput.y * _config.mouseSensitivity);

            if (Mathf.Abs(inputX) > Mathf.Epsilon || Mathf.Abs(inputY) > Mathf.Epsilon)
            {
                _currentYaw = Mathf.Clamp(_currentYaw + inputX, _config.minYaw, _config.maxYaw);
                _horizontalAxis.localRotation = Quaternion.Euler(0, _currentYaw, 0);
                
                float pitchDelta = -inputY; 
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
            {
                if (IsReadyToFire())
                {
                    Fire();
                }
                else
                {
                    OnFireFailedCooldown?.Invoke();
                }
            }
        }

        private async void Fire()
        {
            _lastFireTime = Time.time;
            OnFireSuccess?.Invoke();

            if (_config.fireSound != null)
            {
                AudioSource.PlayClipAtPoint(_config.fireSound, _muzzle.position, _audioService.SfxVolume);
            }

            float currentMass = Random.Range(_config.projectileMass * 0.95f, _config.projectileMass * 1.05f);

            var projectileObj = await _factory.InstantiateAsync(_config.projectileAsset, _muzzle.position, Quaternion.identity);
            var physicsComp = projectileObj.GetComponent<PhysicsProjectile>();
            
            if (physicsComp != null)
            {
                Vector3 startVel = _muzzle.forward * _config.initialSpeed;
                physicsComp.Initialize(currentMass, _config.projectileRadius, _config.dragCoeff, 
                    _config.airDensity, _config.wind, startVel, _config.damage, _config);
            }
        }
    }
}