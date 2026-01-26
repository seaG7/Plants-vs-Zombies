using System.Collections.Generic;
using Data.Enums;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

        [Header("Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 1.5f;
        public float damage = 10f;
        public int killReward = 10;

#if UNITY_EDITOR
        private void Reset()
        {
            AssignUniqueId();
        }

        [ContextMenu("Recalculate ID")]
        private void AssignUniqueId()
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyData");
            HashSet<int> usedIds = new HashSet<int>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                if (data != null && data != this) usedIds.Add(data.id);
            }

            int nextId = 1;
            while (usedIds.Contains(nextId)) nextId++;

            if (id != nextId)
            {
                id = nextId;
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}