using System.Collections.Generic;
using Data.Enums;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data.Configs
{
    [CreateAssetMenu(fileName = "NewPlantData", menuName = "Configs/Plant Data")]
    public class PlantData : ScriptableObject
    {
        [Header("General Info")]
        [Tooltip("Unique ID auto-generated")]
        public int id;
        public PlantType type;
        public string plantName;
        [TextArea] public string description;

        [Header("Visuals")]
        public Sprite icon;
        public AssetReferenceGameObject prefabReference;

        [Header("Economy")]
        [Min(0)] public int cost;
        [Tooltip("Cooldown in seconds between planting")]
        public float cooldown = 5f;

        [Header("Combat Stats")]
        public float health = 100f;
        public float attackRange = 10f;
        public float attackRate = 1.5f;

#if UNITY_EDITOR
        private void Reset()
        {
            AssignUniqueId();
        }

        [ContextMenu("Recalculate ID")]
        private void AssignUniqueId()
        {
            string[] guids = AssetDatabase.FindAssets("t:PlantData");
            HashSet<int> usedIds = new HashSet<int>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PlantData data = AssetDatabase.LoadAssetAtPath<PlantData>(path);
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