using System;
using System.Collections.Generic;
using Datas.Players;
using Datas.Settings;
using Events.External;
using Extensions.System;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class MainSceneInstaller : MonoInstaller<MainSceneInstaller>
    {
        [Inject] private MainSceneSettings MainSceneSettings { get; set; }
        [Inject] private GameStateEvents GameStateEvents { get; set; }
        [Inject] private IPlayerData PlayerData { get; set; }
        private Settings _mySettings;

        public override void InstallBindings() {}

        private void OnEnable()
        {
            RegisterEvents();
        }

        public override void Start()
        {
            base.Start();
            _mySettings = MainSceneSettings.Settings;
            SetUIStartState();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

        private void SetUIStartState()
        {
            /*
            PanelEvents.ShopSetActive?.Invoke(false);
            PanelEvents.ChallengePanelSetActive?.Invoke(false);
            PanelEvents.FailPanelSetActive?.Invoke(false);
            PanelEvents.LevelMapPanelSetActive?.Invoke(true);
            PanelEvents.SettingsPanelSetActive?.Invoke(false);
            PanelEvents.WinPanelSetActive?.Invoke(false);
            PanelEvents.GamePlayPanelSetActive?.Invoke(false);
        */
        }

        private void RegisterEvents()
        {
            // UIEvents.PlayButton += OnPlayButton;
            GameStateEvents.LevelSuccess += OnLevelSuccess;
            GameStateEvents.LevelFail += OnLevelFail;
        }

        private void OnPlayButton()
        {
            GameObject currentLevel = Container.InstantiatePrefab
            (_mySettings.LevelList[PlayerData.CurrentLevel.ToIndex()].LevelPrefab);
        }

        private void OnLevelSuccess()
        {
            // PanelEvents.WinPanelSetActive?.Invoke(true);
        }

        private void OnLevelFail()
        {
            // PanelEvents.FailPanelSetActive?.Invoke(true);
        }

        private void UnRegisterEvents()
        {
            // UIEvents.PlayButton -= OnPlayButton;
            GameStateEvents.LevelSuccess -= OnLevelSuccess;
            GameStateEvents.LevelFail -= OnLevelFail;
        }

        [Serializable]
        public class Settings
        {
            [InlineEditor][SerializeField] private List<LevelSettings> _levelList;
            public List<LevelSettings> LevelList => _levelList;
        }
    }
}