using UnityEngine;

namespace Infrastructure.Services.Grid
{
    public interface IGridService
    {
        bool WorldToGrid(Vector3 worldPos, out int laneIndex, out int rowIndex);
        Vector3 GridToWorld(int laneIndex, int rowIndex);
        bool IsCellOccupied(int lane, int row);
        GameObject GetPlantAt(int lane, int row);
        bool TryOccupyCell(int lane, int row, GameObject plant);
        void ReleaseCell(int lane, int row);
        void Reset();
    }
}