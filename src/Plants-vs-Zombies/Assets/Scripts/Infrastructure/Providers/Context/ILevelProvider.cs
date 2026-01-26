using System;
using Features.Context;

namespace Infrastructure.Providers.Context
{
    public interface ILevelProvider
    {
        LevelContext CurrentLevel { get; }
        event Action OnLevelLoaded;
        void SetLevel(LevelContext context);
        void ClearLevel();
    }
}