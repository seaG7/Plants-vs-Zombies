using System.Collections.Generic;
using Infrastructure.Providers.Context;
using Infrastructure.Services.Grid;
using UnityEngine;

namespace Features.Visuals
{
    public class GridVisualizer : MonoBehaviour
    {
        private const string CURSOR_MAT_PATH = "Materials/GridMaterial"; 
        private const string GRID_MAT_PATH = "Materials/SelectedCellMaterial";

        private GridCellHighlighter _cursorHighlighter;
        private readonly List<GridCellHighlighter> _staticGrid = new();
        private readonly List<GridCellHighlighter> _laneLines = new();
        
        private Transform _container;
        private Material _cursorMaterial;
        private Material _gridMaterial;

        private readonly Color _staticColor = new Color(0, 1, 0, 0.3f);
        private readonly Color _cursorValidColor = new Color(0, 1, 0, 1.0f);
        private readonly Color _cursorInvalidColor = new Color(1, 0, 0, 1.0f);
        
        private const float STATIC_WIDTH = 0.35f;
        private const float CURSOR_WIDTH = 0.7f;

        public void Initialize()
        {
            _container = new GameObject("GridVisuals_Container").transform;
            _container.SetParent(transform);

            LoadMaterials();

            _cursorHighlighter = CreateHighlighter("Cursor");
            _cursorHighlighter.SetMaterial(_cursorMaterial);
            _cursorHighlighter.SetStyle(CURSOR_WIDTH, _cursorValidColor, sortingOrder: 10);
        }

        private void LoadMaterials()
        {
            _cursorMaterial = Resources.Load<Material>(CURSOR_MAT_PATH);
            _gridMaterial = Resources.Load<Material>(GRID_MAT_PATH);

            if (_cursorMaterial == null) Debug.LogError($"[GridVisualizer] Material not found at Resources/{CURSOR_MAT_PATH}");
            if (_gridMaterial == null) Debug.LogError($"[GridVisualizer] Material not found at Resources/{GRID_MAT_PATH}");
        }

        public void ShowLaneLines(ILevelProvider levelProvider)
        {
            HideLaneLines();

            var level = levelProvider.CurrentLevel;
            if (level == null) return;

            float totalWidth = level.LaneCount * level.LaneWidth;
            float startX = -totalWidth / 2f;
            Vector3 center = level.OriginPosition;
            Quaternion rot = level.OriginRotation;

            for (int i = 0; i <= level.LaneCount; i++)
            {
                var line = GetOrCreateLaneHighlighter();
                line.SetMaterial(_gridMaterial);
                line.SetStyle(STATIC_WIDTH, Color.white, sortingOrder: 0);

                float xOffset = startX + (i * level.LaneWidth);
                Vector3 startPos = center + rot * new Vector3(xOffset, 0, -level.RowsCount * level.CellLength);
                Vector3 endPos = center + rot * new Vector3(xOffset, 0, level.ZombieSpawnDistance);
                
                line.ShowLine(startPos, endPos);
            }
        }

        public void ShowStaticGrid(ILevelProvider levelProvider, IGridService gridService)
        {
            HideStaticGrid();
            
            var level = levelProvider.CurrentLevel;
            if (level == null) return;

            for (int lane = 0; lane < level.LaneCount; lane++)
            {
                for (int row = 0; row < level.RowsCount; row++)
                {
                    if (gridService.IsCellOccupied(lane, row)) continue;

                    var highlighter = GetOrCreateStaticHighlighter();
                    highlighter.SetMaterial(_gridMaterial); 
                    highlighter.SetStyle(STATIC_WIDTH, _staticColor, sortingOrder: 1);
                    
                    Vector3 pos = gridService.GridToWorld(lane, row);
                    highlighter.Show(pos, level.LaneWidth, level.CellLength);
                }
            }
        }

        public void UpdateCursor(Vector3 position, float width, float length, bool isValid)
        {
            Color color = isValid ? _cursorValidColor : _cursorInvalidColor;
            
            _cursorHighlighter.SetMaterial(_cursorMaterial);
            _cursorHighlighter.SetStyle(CURSOR_WIDTH, color, sortingOrder: 10);
            _cursorHighlighter.Show(position, width, length);
        }

        public void HideCursor() => _cursorHighlighter.Hide();
        public void HideStaticGrid() { foreach (var h in _staticGrid) h.Hide(); }
        public void HideLaneLines() { foreach (var h in _laneLines) h.Hide(); }

        public void HideAllInteractive()
        {
            HideCursor();
            HideStaticGrid();
        }

        private GridCellHighlighter GetOrCreateStaticHighlighter() => GetFromPool(_staticGrid, "StaticCell");
        private GridCellHighlighter GetOrCreateLaneHighlighter() => GetFromPool(_laneLines, "LaneLine");

        private GridCellHighlighter GetFromPool(List<GridCellHighlighter> pool, string namePrefix)
        {
            foreach (var h in pool)
            {
                if (!h) continue;
                
                if (!h.gameObject.activeSelf || !h.GetComponent<LineRenderer>().enabled)
                {
                    h.gameObject.SetActive(true);
                    return h;
                }
            }
            var newH = CreateHighlighter($"{namePrefix}_{pool.Count}");
            pool.Add(newH);
            return newH;
        }

        private GridCellHighlighter CreateHighlighter(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_container);
            var lr = go.AddComponent<LineRenderer>();
            return go.AddComponent<GridCellHighlighter>();
        }

        private void OnDestroy()
        {
            if (_container != null) Destroy(_container.gameObject);
        }
    }
}