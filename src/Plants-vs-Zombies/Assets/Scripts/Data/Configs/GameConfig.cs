using UnityEngine;

namespace Data.Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Audio")]
        public AudioClip mainMenuMusic;
    }
}