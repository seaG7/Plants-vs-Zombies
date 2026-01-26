using System;

namespace Infrastructure.Services.Waves
{
    public interface IWaveService
    {
        void StartLevel();
        event Action<int> OnWaveStarted;
    }
}