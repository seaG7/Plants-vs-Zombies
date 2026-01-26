using UnityEngine;

namespace Infrastructure.Services.Grid
{
    public interface IGridService
    {
        // Converts world click to grid coordinates (Lane Index, Row Index)
        bool WorldToGrid(Vector3 worldPos, out int laneIndex, out int rowIndex);
        
        // Converts grid coordinates to World Position (center of cell)
        Vector3 GridToWorld(int laneIndex, int rowIndex);
        
        bool IsCellOccupied(int lane, int row);
        bool TryOccupyCell(int lane, int row, GameObject plant);
        void ReleaseCell(int lane, int row);
    }
}