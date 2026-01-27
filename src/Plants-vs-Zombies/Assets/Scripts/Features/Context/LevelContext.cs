using System.Collections.Generic;
using Core.Interfaces;
using Infrastructure.Providers.Context;
using Infrastructure.Services.Waves;
using UnityEngine;
using Zenject;

namespace Features.Context
{
    public class LevelContext : MonoBehaviour
    {
        [Header("Debug")]
        public bool ForceStartWaves;

        [Header("Anchors")]
        [SerializeField] private Transform _originPoint;

        [Header("Dimensions")]
        [SerializeField] private int _laneCount = 5;
        [SerializeField] private float _laneWidth = 2.0f;
        [SerializeField] private float _cellLength = 2.0f;
        [SerializeField] private float _zombieSpawnDistance = 50f;

        [Header("Grid Config")]
        [SerializeField] private int _rowsCount = 10;

        private ILevelProvider _levelProvider;
        private IWaveService _waveService;
        private FinishLineTrigger _finishTrigger;
        
        private readonly List<IDamageable> _activeEnemies = new();

        public Vector3 OriginPosition => _originPoint != null ? _originPoint.position : transform.position;
        public Quaternion OriginRotation => _originPoint != null ? _originPoint.rotation : transform.rotation;
        
        public int LaneCount => _laneCount;
        public float LaneWidth => _laneWidth;
        public float CellLength => _cellLength;
        public int RowsCount => _rowsCount;
        public float ZombieSpawnDistance => _zombieSpawnDistance;
        public float FinishZCoordinate => 0f;

        public FinishLineTrigger FinishTrigger => _finishTrigger;
        public IReadOnlyList<IDamageable> ActiveEnemies => _activeEnemies;
        
        [Header("Camera")]
        [SerializeField] private Transform _cameraTacticalPoint;
        public Transform CameraTacticalPoint => _cameraTacticalPoint;

        [Inject]
        public void Construct(ILevelProvider levelProvider, IWaveService waveService)
        {
            _levelProvider = levelProvider;
            _waveService = waveService;
        }

        private void Awake()
        {
            _levelProvider.SetLevel(this);
            CreateFinishTrigger();
        }

        private void CreateFinishTrigger()
        {
            var triggerObj = new GameObject("FinishTrigger");
            triggerObj.transform.SetParent(_originPoint != null ? _originPoint : transform);
            
            triggerObj.transform.localPosition = new Vector3(0, 1f, FinishZCoordinate);
            triggerObj.transform.localRotation = Quaternion.identity;

            float totalWidth = _laneCount * _laneWidth;
            
            var col = triggerObj.AddComponent<BoxCollider>();
            col.size = new Vector3(totalWidth, 5f, 1f); 

            _finishTrigger = triggerObj.AddComponent<FinishLineTrigger>();
        }

        private void Update()
        {
            if (ForceStartWaves)
            {
                ForceStartWaves = false;
                _waveService.StartLevel();
            }
        }

        private void OnDestroy()
        {
            _levelProvider.ClearLevel();
        }

        public void RegisterEnemy(IDamageable enemy)
        {
            if (!_activeEnemies.Contains(enemy))
                _activeEnemies.Add(enemy);
        }

        public void UnregisterEnemy(IDamageable enemy)
        {
            if (_activeEnemies.Contains(enemy))
                _activeEnemies.Remove(enemy);
        }
    }
}