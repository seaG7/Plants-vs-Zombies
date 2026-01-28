using Data.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data.Configs
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Configs/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("General Info")]
        public int id;
        public EnemyType type;
        public string enemyName;
        
        [Header("Visuals")]
        public AssetReferenceGameObject prefabReference;
        public AssetReferenceGameObject spawnEffect;

        [Header("Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 1.5f;
        public float damage = 10f;
        public int killReward = 10;
        
        [Header("Audio")]
        public AudioClip spawnSound;
        public AudioClip deathSound;

#if UNITY_EDITOR
        private void Reset()
        {
            // Editor logic
        }
#endif
    }
}