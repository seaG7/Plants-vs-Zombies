using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Services.Camera
{
    public interface ICameraService
    {
        void SetTacticalView(Transform tacticalPoint, float duration = 1f);
        UniTask MoveToTarget(Transform target, float duration = 0.5f);
    }
}