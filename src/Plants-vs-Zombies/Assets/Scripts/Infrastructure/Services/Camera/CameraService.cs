using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Infrastructure.Services.Camera
{
    public class CameraService : ICameraService
    {
        private Transform _cachedCameraTransform;

        private Transform CameraTransform
        {
            get
            {
                if (_cachedCameraTransform == null)
                {
                    if (UnityEngine.Camera.main != null)
                        _cachedCameraTransform = UnityEngine.Camera.main.transform;
                }
                return _cachedCameraTransform;
            }
        }

        public async void SetTacticalView(Transform tacticalPoint, float duration = 1f)
        {
            var cam = CameraTransform;
            if (cam == null || tacticalPoint == null) return;
            
            cam.DOMove(tacticalPoint.position, duration).SetEase(Ease.InOutSine);
            cam.DORotateQuaternion(tacticalPoint.rotation, duration).SetEase(Ease.InOutSine);

            await UniTask.WaitForSeconds(1);
        }

        public async UniTask MoveToTarget(Transform target, float duration = 0.5f)
        {
            var cam = CameraTransform;
            if (cam == null || target == null) return;
            
            var sequence = DOTween.Sequence();
            sequence.Join(cam.DOMove(target.position, duration).SetEase(Ease.OutCubic));
            sequence.Join(cam.DORotateQuaternion(target.rotation, duration).SetEase(Ease.OutCubic));

            await sequence.AsyncWaitForCompletion();
        }
    }
}