using JetBrains.Annotations;
using Zenject;

namespace Datas
{
    [UsedImplicitly]
    public class GameData : IGameData, IInitializable
    {
        private int _levelCount;

        int IGameData.LevelCount => _levelCount;

        public void Initialize() {}

        public void SetLevelCount(int totalLevelCount)
        {
            _levelCount = totalLevelCount;
        }
    }

    public interface IGameData
    {
        int LevelCount { get; }
    }
}