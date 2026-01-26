using System.Collections.Generic;
using Core.Interfaces;
using Infrastructure.Providers.Context;
using UnityEngine;
using Zenject;

namespace Features.Context
{
    /// <summary>
    /// Holds configuration for the current level's layout, dimensions and grid visualization.
    /// </summary>
    public class LevelContext : MonoBehaviour
    {
        [Header("Anchors")]
        [SerializeField] private Transform _originPoint;

        [Header("Dimensions")]
        [SerializeField] private int _laneCount = 5;
        [SerializeField] private float _laneWidth = 2.0f;
        [SerializeField] private float _cellLength = 2.0f;
        [SerializeField] private float _zombieSpawnDistance = 50f;

        [Header("Grid Config")]
        [SerializeField] private int _rowsCount = 1;

        private ILevelProvider _levelProvider;
        
        private readonly List<IDamageable> _activeEnemies = new();

        public Vector3 OriginPosition => _originPoint != null ? _originPoint.position : transform.position;
        public Quaternion OriginRotation => _originPoint != null ? _originPoint.rotation : transform.rotation;
        
        public int LaneCount => _laneCount;
        public float LaneWidth => _laneWidth;
        public float CellLength => _cellLength;
        public int RowsCount => _rowsCount;
        public float ZombieSpawnDistance => _zombieSpawnDistance;

        public IReadOnlyList<IDamageable> ActiveEnemies => _activeEnemies;
        
        [Header("Camera")]
        [SerializeField] private Transform _cameraTacticalPoint;
        public Transform CameraTacticalPoint => _cameraTacticalPoint;

        [Inject]
        public void Construct(ILevelProvider levelProvider)
        {
            _levelProvider = levelProvider;
        }

        private void Awake()
        {
            _levelProvider.SetLevel(this);
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

        private void OnDrawGizmos()
        {
            Vector3 center = OriginPosition;
            Quaternion rot = OriginRotation;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center, 0.3f);

            float totalWidth = _laneCount * _laneWidth;
            float startX = -totalWidth / 2f;
            
            Gizmos.color = Color.cyan;
            for (int i = 0; i <= _laneCount; i++)
            {
                float xOffset = startX + (i * _laneWidth);

                Vector3 startPos = center + rot * new Vector3(xOffset, 0, -_rowsCount * _cellLength);
                Vector3 endPos = center + rot * new Vector3(xOffset, 0, _zombieSpawnDistance);
                
                Gizmos.DrawLine(startPos, endPos);
            }

            Gizmos.color = Color.yellow;
            for (int r = 0; r <= _rowsCount; r++)
            {
                float zOffset = -(r * _cellLength);
                
                Vector3 leftSide = center + rot * new Vector3(startX, 0, zOffset);
                Vector3 rightSide = center + rot * new Vector3(startX + totalWidth, 0, zOffset);
                
                Gizmos.DrawLine(leftSide, rightSide);
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < _laneCount; i++)
            {
                float centerX = startX + (i * _laneWidth) + (_laneWidth / 2f);
                Vector3 spawnPos = center + rot * new Vector3(centerX, 0, _zombieSpawnDistance);
                Gizmos.DrawWireSphere(spawnPos, 0.5f);
            }
        }
    }
}