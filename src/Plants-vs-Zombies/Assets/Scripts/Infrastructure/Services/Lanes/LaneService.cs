using Infrastructure.Providers.Context;
using UnityEngine;

namespace Infrastructure.Services.Lanes
{
    public class LaneService : ILaneService
    {
        private readonly ILevelProvider _levelProvider;

        public int LaneCount => _levelProvider.CurrentLevel.LaneCount;

        public LaneService(ILevelProvider levelProvider)
        {
            _levelProvider = levelProvider;
        }

        public Vector3 GetSpawnPosition(int laneIndex)
        {
            var level = _levelProvider.CurrentLevel;
            float xOffset = CalculateLaneCenterLocalX(laneIndex);
            
            Vector3 localPos = new Vector3(xOffset, 0, level.ZombieSpawnDistance);
            return level.OriginPosition + (level.OriginRotation * localPos);
        }

        public Vector3 GetZombieEndPoint(int laneIndex)
        {
            var level = _levelProvider.CurrentLevel;
            float xOffset = CalculateLaneCenterLocalX(laneIndex);
            
            float zEnd = -(level.RowsCount * level.CellLength) - 2.0f;
            
            Vector3 localPos = new Vector3(xOffset, 0, zEnd);
            return level.OriginPosition + (level.OriginRotation * localPos);
        }

        public float GetLaneCenterLocalX(int laneIndex)
        {
            return CalculateLaneCenterLocalX(laneIndex);
        }
        
        public float GetLaneZCoordinate(int laneIndex) => 0;

        private float CalculateLaneCenterLocalX(int laneIndex)
        {
            var level = _levelProvider.CurrentLevel;
            float totalWidth = level.LaneCount * level.LaneWidth;
            float startX = -totalWidth / 2f;
            
            return startX + (laneIndex * level.LaneWidth) + (level.LaneWidth / 2f);
        }
    }
}