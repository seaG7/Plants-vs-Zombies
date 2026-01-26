using System.Collections.Generic;
using Infrastructure.Providers.Context;
using Infrastructure.Services.Lanes;
using UnityEngine;

namespace Infrastructure.Services.Grid
{
    public class GridService : IGridService
    {
        private readonly ILevelProvider _levelProvider;
        private readonly LaneService _laneService;
        private readonly Dictionary<Vector2Int, GameObject> _occupiedCells = new();

        public GridService(ILevelProvider levelProvider, ILaneService laneService)
        {
            _levelProvider = levelProvider;
            _laneService = (LaneService)laneService;
        }

        public bool WorldToGrid(Vector3 worldPos, out int laneIndex, out int rowIndex)
        {
            laneIndex = -1;
            rowIndex = -1;
            var level = _levelProvider.CurrentLevel;

            Vector3 localPos = Quaternion.Inverse(level.OriginRotation) * (worldPos - level.OriginPosition);

            float totalWidth = level.LaneCount * level.LaneWidth;
            float halfWidth = totalWidth / 2f;
            
            float xRelative = localPos.x + halfWidth;
            
            if (xRelative < 0 || xRelative > totalWidth) return false;

            laneIndex = Mathf.FloorToInt(xRelative / level.LaneWidth);
            
            float zRelative = localPos.z;
            if (zRelative > 0 || zRelative < -(level.RowsCount * level.CellLength)) return false;

            rowIndex = Mathf.FloorToInt(Mathf.Abs(zRelative) / level.CellLength);

            return laneIndex >= 0 && laneIndex < level.LaneCount &&
                   rowIndex >= 0 && rowIndex < level.RowsCount;
        }

        public Vector3 GridToWorld(int laneIndex, int rowIndex)
        {
            var level = _levelProvider.CurrentLevel;
            float xOffset = _laneService.GetLaneCenterLocalX(laneIndex);
            
            float zOffset = -(rowIndex * level.CellLength) - (level.CellLength / 2f);

            Vector3 localPos = new Vector3(xOffset, 0, zOffset);
            return level.OriginPosition + (level.OriginRotation * localPos);
        }

        public bool IsCellOccupied(int lane, int row) => _occupiedCells.ContainsKey(new Vector2Int(lane, row));

        public bool TryOccupyCell(int lane, int row, GameObject plant)
        {
            var key = new Vector2Int(lane, row);
            if (_occupiedCells.ContainsKey(key)) return false;

            _occupiedCells[key] = plant;
            return true;
        }

        public void ReleaseCell(int lane, int row)
        {
            var key = new Vector2Int(lane, row);
            if (_occupiedCells.ContainsKey(key))
                _occupiedCells.Remove(key);
        }
    }
}