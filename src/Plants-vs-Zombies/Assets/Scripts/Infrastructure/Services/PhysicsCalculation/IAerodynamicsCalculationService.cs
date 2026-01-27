using UnityEngine;

namespace Infrastructure.Services.Physics
{
    public interface IAerodynamicsCalculationService
    {
        float CalculateCrossSectionArea(float radius);
        Vector3 CalculateDragForce(Vector3 velocity, float airDensity, float dragCoefficient, float area, Vector3 wind);
        Vector3 CalculateAcceleration(Vector3 gravity, Vector3 dragForce, float mass);
    }
}