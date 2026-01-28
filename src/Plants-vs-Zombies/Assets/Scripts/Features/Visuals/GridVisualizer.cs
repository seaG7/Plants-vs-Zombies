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
        private const string FINISH_MAT_PATH = "Materials/FinishMaterial";

        private GridCellHighlighter _cursorHighlighter;
        private GridCellHighlighter _finishLine;
        private GridCellHighlighter _tutorialHighlighter; // New
        
        private readonly List<GridCellHighlighter> _staticGrid = new();
        private readonly List<GridCellHighlighter> _laneLines = new();
        
        private Transform _container;
        private Material _cursorMaterial;
        private Material _gridMaterial;
        private Material _finishMaterial;

        private const float HEIGHT_LANES = 0.5f;
        private const float HEIGHT_FINISH = 0.55f;
        private const float HEIGHT_GRID = 0.6f;
        private const float HEIGHT_CURSOR = 0.65f;
        private const float HEIGHT_TUTORIAL = 0.70f; // Topmost
        
        private const float STATIC_WIDTH = 0.35f;
        private const float CURSOR_WIDTH = 0.9f;
        private const float FINISH_WIDTH = 0.9f;

        public void Initialize()
        {
            _container = new GameObject("GridVisuals_Container").transform;
            _container.SetParent(transform);

            LoadMaterials();

            _cursorHighlighter = CreateHighlighter("Cursor");
            _finishLine = CreateHighlighter("FinishLine");
            _tutorialHighlighter = CreateHighlighter("TutorialHighlight");
        }

        private void LoadMaterials()
        {
            _cursorMaterial = Resources.Load<Material>(CURSOR_MAT_PATH);
            _gridMaterial = Resources.Load<Material>(GRID_MAT_PATH);
            _finishMaterial = Resources.Load<Material>(FINISH_MAT_PATH);
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
                line.SetStyle(STATIC_WIDTH, sortingOrder: 0);
                line.transform.position = new Vector3(0, HEIGHT_LANES, 0);

                float xOffset = startX + (i * level.LaneWidth);

                Vector3 startPos = center + rot * new Vector3(xOffset, 0, level.FinishZCoordinate);
                Vector3 endPos = center + rot * new Vector3(xOffset, 0, level.ZombieSpawnDistance);
                
                line.ShowLine(startPos, endPos);
            }
            ShowFinishLine(level, startX, totalWidth, center, rot);
        }

        private void ShowFinishLine(Features.Context.LevelContext level, float startX, float totalWidth, Vector3 center, Quaternion rot)
        {
            _finishLine.SetMaterial(_finishMaterial);
            _finishLine.SetStyle(FINISH_WIDTH, sortingOrder: 1);
            _finishLine.transform.position = new Vector3(0, HEIGHT_FINISH, 0);

            float zFinish = level.FinishZCoordinate;
            Vector3 p1 = center + rot * new Vector3(startX, 0, zFinish);
            Vector3 p2 = center + rot * new Vector3(startX + totalWidth, 0, zFinish);

            _finishLine.ShowLine(p1, p2);
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
                    highlighter.SetStyle(STATIC_WIDTH, sortingOrder: 2);
                    highlighter.transform.position = new Vector3(0, HEIGHT_GRID, 0);
                    
                    Vector3 pos = gridService.GridToWorld(lane, row);
                    highlighter.Show(pos, level.LaneWidth, level.CellLength);
                }
            }
        }

        public void UpdateCursor(Vector3 position, float width, float length, bool isValid)
        {
            _cursorHighlighter.transform.position = new Vector3(0, HEIGHT_CURSOR, 0);
            _cursorHighlighter.SetMaterial(_cursorMaterial);
            _cursorHighlighter.SetStyle(CURSOR_WIDTH, sortingOrder: 3);
            _cursorHighlighter.Show(position, width, length);
        }

        public void ShowTutorialHighlight(Vector3 position, float width, float length)
        {
            _tutorialHighlighter.transform.position = new Vector3(0, HEIGHT_TUTORIAL, 0);
            _tutorialHighlighter.SetMaterial(_cursorMaterial); 
            _tutorialHighlighter.SetStyle(CURSOR_WIDTH * 1.1f, sortingOrder: 10);
            _tutorialHighlighter.Show(position, width, length);
        }
        public void HideTutorialHighlight() => _tutorialHighlighter.Hide();

        public void HideCursor() => _cursorHighlighter.Hide();
        public void HideStaticGrid() { foreach (var h in _staticGrid) h.Hide(); }

        public void HideAllInteractive()
        {
            HideCursor();
            HideStaticGrid();
            HideTutorialHighlight();
        }
        public void HideLaneLines() 
        { 
            foreach (var h in _laneLines) h.Hide();
            _finishLine.Hide();
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