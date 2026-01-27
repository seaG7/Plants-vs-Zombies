using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Infrastructure.Services.Camera
{
    public class CameraService : ICameraService
    {
        private Transform _mainCameraTransform;

        public CameraService()
        {
            if (UnityEngine.Camera.main != null) 
                _mainCameraTransform = UnityEngine.Camera.main.transform;
        }

        public async void SetTacticalView(Transform tacticalPoint, float duration = 1f)
        {
            if (_mainCameraTransform == null || tacticalPoint == null) return;
            
            _mainCameraTransform.DOMove(tacticalPoint.position, duration).SetEase(Ease.InOutSine);
            _mainCameraTransform.DORotateQuaternion(tacticalPoint.rotation, duration).SetEase(Ease.InOutSine);

            await UniTask.WaitForSeconds(1);
        }

        public async UniTask MoveToTarget(Transform target, float duration = 0.5f)
        {
            if (_mainCameraTransform == null || target == null) return;
            
            var sequence = DOTween.Sequence();
            sequence.Join(_mainCameraTransform.DOMove(target.position, duration).SetEase(Ease.OutCubic));
            sequence.Join(_mainCameraTransform.DORotateQuaternion(target.rotation, duration).SetEase(Ease.OutCubic));

            await sequence.AsyncWaitForCompletion();
        }
    }
}