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
        public int id;
        public PlantType type;
        public string plantName;
        [TextArea] public string description;

        [Header("Visuals")]
        public Sprite icon;
        public AssetReferenceGameObject prefabReference;

        [Header("Economy")]
        [Min(0)] public int cost;
        public float cooldown = 5f;

        [Header("Combat Stats")]
        public float health = 100f;
        
        [Header("Cannon Physics")]
        public AssetReference projectileAsset;
        public float rotationSpeed = 45f;
        public float initialSpeed = 20f;
        public float minPitch = -30f;
        public float maxPitch = 30f;
        public float minYaw = -45f;
        public float maxYaw = 45f;
        
        [Header("Projectile Config")]
        public float projectileMass = 2f;
        public float projectileRadius = 0.5f;
        public float dragCoeff = 0.47f;
        public float airDensity = 1.225f;
        public Vector3 wind = Vector3.zero;

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