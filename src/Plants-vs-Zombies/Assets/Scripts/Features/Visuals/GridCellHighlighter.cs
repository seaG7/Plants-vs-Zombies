using UnityEngine;

namespace Features.Visuals
{
    [RequireComponent(typeof(LineRenderer))]
    public class GridCellHighlighter : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private const float HEIGHT_OFFSET = 0f; 

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;
            
            if (_lineRenderer.material == null)
                 _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

            Hide();
        }

        public void SetMaterial(Material material)
        {
            if (material == null) return;
            if (_lineRenderer == null) Awake();
            _lineRenderer.material = material;
        }

        public void SetStyle(float width, int sortingOrder = 0)
        {
            if (_lineRenderer == null) Awake();
            
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            _lineRenderer.sortingOrder = sortingOrder;
        }

        public void Show(Vector3 centerPos, float width, float length)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.loop = true;
            _lineRenderer.positionCount = 5;

            float halfWidth = width / 2f;
            float halfLength = length / 2f;
            float y = transform.position.y + HEIGHT_OFFSET;

            Vector3[] points = new Vector3[5];
            points[0] = new Vector3(centerPos.x - halfWidth, y, centerPos.z - halfLength);
            points[1] = new Vector3(centerPos.x - halfWidth, y, centerPos.z + halfLength);
            points[2] = new Vector3(centerPos.x + halfWidth, y, centerPos.z + halfLength);
            points[3] = new Vector3(centerPos.x + halfWidth, y, centerPos.z - halfLength);
            points[4] = points[0];

            _lineRenderer.SetPositions(points);
        }

        public void ShowLine(Vector3 start, Vector3 end)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.loop = false;
            _lineRenderer.positionCount = 2;
            
            float y = start.y + HEIGHT_OFFSET;
            _lineRenderer.SetPosition(0, new Vector3(start.x, y, start.z));
            _lineRenderer.SetPosition(1, new Vector3(end.x, y, end.z));
        }

        public void Hide()
        {
            if (_lineRenderer != null)
                _lineRenderer.enabled = false;
        }
    }
}