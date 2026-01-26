using UnityEngine;

namespace Infrastructure.Services.Lanes
{
    public interface ILaneService
    {
        public int LaneCount { get; }
        Vector3 GetSpawnPosition(int laneIndex);
        Vector3 GetZombieEndPoint(int laneIndex);
        float GetLaneCenterLocalX(int laneIndex);
        float GetLaneZCoordinate(int laneIndex);
    }
}