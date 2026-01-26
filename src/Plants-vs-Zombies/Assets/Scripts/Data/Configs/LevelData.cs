using System;
using System.Collections.Generic;
using Data.Enums;
using UnityEngine;

namespace Data.Configs
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Configs/LevelData")]
    public class LevelData : ScriptableObject
    {
        [Header("Economy Settings")]
        public EconomySettings economy;

        [Header("Waves Settings")]
        public List<WaveInfo> waves;
    }

    [Serializable]
    public class EconomySettings
    {
        public int startingSun = 50;
        
        public float passiveIncomeInterval = 5.0f;

        public int passiveIncomeAmount = 25;
    }
  
    [Serializable]
    public class WaveInfo
    {
        [Tooltip("Delay before this wave starts")]
        public float startDelay = 5f;
        public List<WaveGroup> groups;
    }

    [Serializable]
    public class WaveGroup
    {
        public EnemyType enemyType;
        public int count = 5;
        public float spawnInterval = 2f;
    }
}