using UnityEngine;
using Zenject;

namespace Infrastructure.Services.FPS
{
    /// <summary>
    /// Calculates application frame rate using a smoothing buffer.
    /// </summary>
    public class FPSService : IFPSService, ITickable
    {
        private const int FRAME_RANGE = 60;
        private readonly int[] _fpsBuffer = new int[FRAME_RANGE];
        private int _fpsBufferIndex;
        
        public float CurrentFps { get; private set; }

        public void Tick()
        {
            int currentFps = (int)(1f / Time.unscaledDeltaTime);
            
            _fpsBufferIndex = (_fpsBufferIndex + 1) % FRAME_RANGE;
            _fpsBuffer[_fpsBufferIndex] = currentFps;

            CalculateAverage();
        }

        private void CalculateAverage()
        {
            int sum = 0;
            for (int i = 0; i < FRAME_RANGE; i++)
            {
                sum += _fpsBuffer[i];
            }
            
            CurrentFps = (float)sum / FRAME_RANGE;
        }
    }
}