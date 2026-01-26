using System;
using Features.Context;

namespace Infrastructure.Providers.Context
{
    public class LevelProvider : ILevelProvider
    {
        public LevelContext CurrentLevel { get; private set; }
        public event Action OnLevelLoaded;

        public void SetLevel(LevelContext context)
        {
            CurrentLevel = context;
            OnLevelLoaded?.Invoke();
        }

        public void ClearLevel()
        {
            CurrentLevel = null;
        }
    }
}