using JetBrains.Annotations;
using UnityEngine.Events;

namespace Events.External
{
    [UsedImplicitly]
    public class GameStateEvents
    {
        private static bool isProjectInstallerStartRPCFired;
        public UnityAction LevelFail;
        public UnityAction LevelSuccess;
        public UnityAction RestartLevel;
        public UnityAction SceneInstallerStart;
        
        private UnityAction _projectInstallerStartRPC;

        /// <summary>
        /// Runs only once per session.
        /// </summary>
        /// <returns></returns>
        public UnityAction ProjectInstallerStartRPC
        {
            get => _projectInstallerStartRPC;
            set
            {
                if (isProjectInstallerStartRPCFired)
                {
                    value?.Invoke();
                    return;
                }
                
                _projectInstallerStartRPC = value;
            }
        }

        public GameStateEvents()
        {
            _projectInstallerStartRPC += OnProjectInstallerStartRPC;
        }

        private void OnProjectInstallerStartRPC()
        {
            isProjectInstallerStartRPCFired = true;
        }
    }
}