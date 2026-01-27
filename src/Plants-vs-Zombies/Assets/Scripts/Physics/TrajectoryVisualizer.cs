using Infrastructure.Services.Physics;
using UnityEngine;
using Zenject;

namespace Physics
{
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryVisualizer : MonoBehaviour
    {
        [SerializeField] private int _maxPoints = 100;
        [SerializeField] private float _timeStep = 0.02f;
        [SerializeField] private LayerMask _collisionMask;
        
        private LineRenderer _lineRenderer;
        private IAerodynamicsCalculationService _calculator;

        [Inject]
        public void Construct(IAerodynamicsCalculationService calculator)
        {
            _calculator = calculator;
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
        }

        public void SetMaterial(Material mat)
        {
            if (mat != null) _lineRenderer.material = mat;
        }

        public void SetWidth(float width)
        {
            _lineRenderer.startWidth = _lineRenderer.endWidth = width;
        }

        public void SimulateTrajectory(Vector3 startPos, Vector3 startVelocity, float mass, float radius, float dragCoeff, float airDensity, Vector3 wind)
        {
            if (_maxPoints < 2) return;

            _lineRenderer.positionCount = _maxPoints;
            _lineRenderer.SetPosition(0, startPos);

            Vector3 currentPos = startPos;
            Vector3 currentVel = startVelocity;
            float area = _calculator.CalculateCrossSectionArea(radius);

            for (int i = 1; i < _maxPoints; i++)
            {
                Vector3 dragForce = _calculator.CalculateDragForce(currentVel, airDensity, dragCoeff, area, wind);
                Vector3 acceleration = _calculator.CalculateAcceleration(UnityEngine.Physics.gravity, dragForce, mass);

                currentVel += acceleration * _timeStep;
                Vector3 nextPos = currentPos + currentVel * _timeStep;

                if (CheckCollision(currentPos, nextPos, radius, out Vector3 hitPoint))
                {
                    _lineRenderer.SetPosition(i, hitPoint);
                    _lineRenderer.positionCount = i + 1;
                    return;
                }

                currentPos = nextPos;
                _lineRenderer.SetPosition(i, currentPos);
            }
        }

        private bool CheckCollision(Vector3 from, Vector3 to, float radius, out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;
            Vector3 dir = to - from;
            float dist = dir.magnitude;

            if (UnityEngine.Physics.SphereCast(from, radius, dir.normalized, out RaycastHit hit, dist, _collisionMask))
            {
                hitPoint = hit.point;
                return true;
            }
            return false;
        }
        
        public void Clear()
        {
            _lineRenderer.positionCount = 0;
        }
    }
}