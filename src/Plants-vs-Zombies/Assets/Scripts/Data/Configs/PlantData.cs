using Data.Enums;
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

        [Header("Economy & Stats")]
        [Min(0)] public int cost;
        public float damage = 50f;
        
        [Header("Sunflower Settings")]
        public int sunGenerationAmount = 25;
        public float sunGenerationInterval = 10f;

        [Header("Cannon/Peashooter Physics")]
        public AssetReference projectileAsset;
        public Material trajectoryMaterial;
        public float trajectoryWidth = 0.4f;
        public float impactRadius = 1.5f;
        public float fireCooldown = 4f;
        public float rotationSpeed = 25f;
        public float mouseSensitivity = 0.05f;
        public float initialSpeed = 100f;
        public float minPitch = -30f;
        public float maxPitch = 30f;
        public float minYaw = -45f;
        public float maxYaw = 45f;
        
        [Header("Projectile Config")]
        public float projectileMass = 4f;
        public float projectileRadius = 0.2f;
        public float dragCoeff = 0.47f;
        public float airDensity = 1.225f;
        public Vector3 wind = Vector3.zero;

        [Header("Effects & Audio")]
        public AssetReferenceGameObject impactEffect;
        public AudioClip fireSound;
        public AudioClip hitSound;
        public AudioClip plantSound;

#if UNITY_EDITOR
        private void Reset()
        {
            // Editor logic
        }
#endif
    }
}