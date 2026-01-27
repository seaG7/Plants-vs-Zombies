using System.Collections.Generic;
using Data.Configs;
using Infrastructure.Factories.Enemies;
using Infrastructure.Providers.StaticData;
using Infrastructure.Services.Economy;
using Infrastructure.Services.Lanes;
using UnityEngine;
using Zenject;

namespace Infrastructure.Services.Waves
{
    public class WaveService : IWaveService, ITickable
    {
        private readonly IEnemyFactory _enemyFactory;
        private readonly ILaneService _laneService;
        private readonly IStaticDataProvider _staticData;
        private readonly IEconomyService _economyService; 

        private LevelData _levelConfig;
        private int _currentWaveIndex = 0;
        private float _stateTimer;
        private bool _isRunning;
        
        private Queue<WaveGroup> _currentGroupsQueue = new();
        private WaveGroup _activeGroup;
        private int _spawnedInGroupCount;

        public event System.Action<int> OnWaveStarted;

        public WaveService(
            IEnemyFactory enemyFactory, 
            ILaneService laneService, 
            IStaticDataProvider staticData, 
            IEconomyService economyService)
        {
            _enemyFactory = enemyFactory;
            _laneService = laneService;
            _staticData = staticData;
            _economyService = economyService;
        }

        public void StartLevel()
        {
            _levelConfig = _staticData.GetLevelData();
            if (_levelConfig == null || _levelConfig.waves.Count == 0)
            {
                Debug.LogError("[WaveService] No Level Config found!");
                return;
            }

            _currentWaveIndex = 0;
            _isRunning = true;
            StartWave(_currentWaveIndex);
        }

        public void Tick()
        {
            if (!_isRunning) return;
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0) ProcessWaveLogic();
        }

        private void ProcessWaveLogic()
        {
            if (_activeGroup == null)
            {
                StartWave(_currentWaveIndex);
                return;
            }

            SpawnEnemy(_activeGroup.enemyType);
            _spawnedInGroupCount++;

            if (_spawnedInGroupCount >= _activeGroup.count)
                NextGroup();
            else
                _stateTimer = _activeGroup.spawnInterval;
        }

        private async void SpawnEnemy(Data.Enums.EnemyType type)
        {
            var enemyData = _staticData.GetEnemyData(type);
            if (enemyData == null) return;

            int randomLane = UnityEngine.Random.Range(0, _laneService.LaneCount);
            Vector3 spawnPos = _laneService.GetSpawnPosition(randomLane);

            var zombie = await _enemyFactory.CreateZombie(enemyData.prefabReference, spawnPos);
            
            zombie.OnDeath += () => _economyService.AddSun(enemyData.killReward);
        }
        
        private void NextGroup()
        {
            if (_currentGroupsQueue.Count == 0)
            {
                _currentWaveIndex++;
                if (_currentWaveIndex < _levelConfig.waves.Count)
                    _stateTimer = _levelConfig.waves[_currentWaveIndex].startDelay;
                else
                    _isRunning = false;
                return;
            }
            _activeGroup = _currentGroupsQueue.Dequeue();
            _spawnedInGroupCount = 0;
            _stateTimer = 0f; 
        }

        private void StartWave(int index)
        {
            if (index >= _levelConfig.waves.Count)
            {
                _isRunning = false;
                return;
            }
            var waveInfo = _levelConfig.waves[index];
            _currentGroupsQueue = new Queue<WaveGroup>(waveInfo.groups);
            OnWaveStarted?.Invoke(index + 1);
            NextGroup();
        }
        
        public void StopLevel()
        {
            _isRunning = false;
            _currentGroupsQueue.Clear();
            _activeGroup = null;
        }
    }
}