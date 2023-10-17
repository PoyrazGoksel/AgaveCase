using Datas;
using Datas.Players;
using Datas.Settings;
using Events.External;
using Extensions.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            InstallUtils();
        }

        private void InstallEvents()
        {
            // Container.Bind<PanelEvents>().AsSingle();
            Container.Bind<GameStateEvents>().AsSingle();
            Container.Bind<PlayerEvents>().AsSingle();
            // Container.Bind<UIEvents>().AsSingle();
        }

        private void InstallSettings()
        {
            Container.BindInstance(Resources.Load<MainSceneSettings>(MainSceneSettings.Path))
            .AsSingle();
        }

        private void InstallData()
        {
            Container.BindInterfacesTo<PlayerData>()
            .AsSingle();

            _gameData = new GameData();
            
            Container.BindInstance((IGameData)_gameData)
            .AsSingle();
        }

        private void InstallUtils()
        {
            Container.BindInterfacesTo<InputListener>().AsSingle();
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
            LoadGameScene();
        }

        private void OnRestartLevelBut()
        {
            LoadGameScene();
        }

        private void LoadGameScene()
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