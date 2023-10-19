using System;
using Datas;
using Datas.Players;
using Datas.Settings;
using Events.External;
using Extensions.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using Utils;
using Zenject;

namespace Installers
{
    public class ProjectInstaller : MonoInstaller<ProjectInstaller>
    {
        private const float LoadingTimerMin = 1f;
        private const string MainSceneName = "Main";
        private GameStateEvents _gameStateEvents;
        private GameData _gameData;
        private MainSceneSettings _mainSceneSettings;
        private RoutineHelper _loadingRoutine;
        private float _loadingTimer;
        private AsyncOperation _loadingOperation;

        private void Awake()
        {
            _loadingRoutine = new RoutineHelper
            (
                this,
                new WaitForFixedUpdate(),
                UpdateLoading,
                () => true
            );
        }

        public override void InstallBindings()
        {
            InstallEvents();
            InstallSettings();
            InstallData();
        }

        private void InstallEvents()
        {
            Container.Bind<GameStateEvents>().AsSingle();
            Container.Bind<PlayerEvents>().AsSingle();
            Container.Bind<GridEvents>().AsSingle();
            Container.Bind<CameraEvents>().AsSingle();
        }

        private void InstallSettings()
        {
            Container.BindInstance(Resources.Load<MainSceneSettings>(EnvironmentVariables.MainSceneSettingsPath))
            .AsSingle();
            
            Container.BindInstance(Resources.Load<GridInstallerSettings>
            (EnvironmentVariables.GridInstallerSettingsPath)).AsSingle();
        }

        private void InstallData()
        {
            Container.BindInterfacesTo<PlayerData>()
            .AsSingle();

            _gameData = new GameData();
            
            Container.BindInstance((IGameData)_gameData)
            .AsSingle();
        }

        public override void Start()
        {
            _mainSceneSettings = Container.Resolve<MainSceneSettings>();
            _gameStateEvents = Container.Resolve<GameStateEvents>();
            // _uiEvents = Container.Resolve<UIEvents>();

            _gameData.SetLevelCount(_mainSceneSettings.Settings.LevelList.Count);
            
            OnRegisterEvents();
            
            _gameStateEvents.ProjectInstallerStartRPC?.Invoke();
            LoadGameScene();
        }

        private void OnRegisterEvents()
        {
            /*
            _uiEvents.NextLevelBut += OnNextLevelBut;
            _uiEvents.RestartLevelBut += OnRestartLevelBut;
            _uiEvents.GamePlayRestartBUT += OnRestartLevelBut;
        */
        }

        private void OnNextLevelBut()
        {
            LoadGameSceneAsync();
        }

        private void OnRestartLevelBut()
        {
            LoadGameSceneAsync();
        }
        
        private void LoadGameScene()
        {
            SceneManager.LoadScene(MainSceneName, LoadSceneMode.Single);
        }

        private void LoadGameSceneAsync()
        {
            _loadingOperation = SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Single);
            _loadingOperation.allowSceneActivation = false;
            _loadingTimer = 0f;
            _loadingRoutine.StartCoroutine();
        }

        private void UpdateLoading()
        {
            float progress = _loadingOperation.progress;

            if (_loadingTimer / LoadingTimerMin > progress)
            {
                progress = _loadingTimer / LoadingTimerMin;
            }

            _loadingTimer += Time.deltaTime;

            if (progress >= 0.99f)
            {
                _loadingOperation.allowSceneActivation = true;
            }
        }
    }
}