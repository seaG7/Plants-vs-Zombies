using UnityEngine;

namespace Infrastructure.Services.Physics
{
    /// <summary>
    /// Provides physics calculations for quadratic drag and aerodynamics.
    /// </summary>
    public class AerodynamicsCalculationService : IAerodynamicsCalculationService
    {
        private const float HALF_COEFFICIENT = 0.5f;

        public float CalculateCrossSectionArea(float radius)
        {
            return Mathf.PI * radius * radius;
        }

        public Vector3 CalculateDragForce(Vector3 velocity, float airDensity, float dragCoefficient, float area, Vector3 wind)
        {
            Vector3 relativeVelocity = velocity - wind;
            float speed = relativeVelocity.magnitude;

            if (speed < Mathf.Epsilon)
                return Vector3.zero;

            // Formula: Fd = -0.5 * rho * Cd * A * |v| * v
            float forceMagnitude = HALF_COEFFICIENT * airDensity * dragCoefficient * area * speed;
            
            return -forceMagnitude * relativeVelocity;
        }

        public Vector3 CalculateAcceleration(Vector3 gravity, Vector3 dragForce, float mass)
        {
            return gravity + (dragForce / mass);
        }
    }
}