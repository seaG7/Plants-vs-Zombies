using System.Collections.Generic;
using System.Linq;
using Data.Configs;
using Data.Enums;
using Data.Path;
using UnityEngine;

namespace Infrastructure.Providers.StaticData
{
    public class StaticDataProvider : IStaticDataProvider
    {
        private Dictionary<PlantType, PlantData> _plants;
        private Dictionary<EnemyType, EnemyData> _enemies;
        private LevelData _levelData;
        private GameConfig _gameConfig;

        public void Load()
        {
            _plants = Resources.LoadAll<PlantData>(ConfigPaths.PLANTS_PATH)
                .ToDictionary(x => x.type, x => x);

            _enemies = Resources.LoadAll<EnemyData>(ConfigPaths.ENEMIES_PATH)
                .ToDictionary(x => x.type, x => x);

            var levels = Resources.LoadAll<LevelData>(ConfigPaths.LEVEL_PATH);
            if (levels.Length > 0)
                _levelData = levels[0];
            else
                Debug.LogError("[StaticDataProvider] No LevelConfig found!");
            
            _gameConfig = Resources.Load<GameConfig>(ConfigPaths.GAME_CONFIG_PATH);

            Debug.Log($"[StaticDataProvider] Loaded {_plants.Count} plants, {_enemies.Count} enemies.");
        }

        public PlantData GetPlantData(PlantType type) => 
            _plants.TryGetValue(type, out var data) ? data : null;

        public EnemyData GetEnemyData(EnemyType type) => 
            _enemies.TryGetValue(type, out var data) ? data : null;

        public LevelData GetLevelData() => _levelData;

        public List<PlantData> GetAllPlants() => _plants.Values.ToList();
        public List<EnemyData> GetAllEnemies() => _enemies.Values.ToList();
        
        public GameConfig GetGameConfig() => _gameConfig;
    }
}