using Installers;
using UnityEngine;
using Utils;

namespace Datas.Settings
{
    [CreateAssetMenu(fileName = nameof(MainSceneSettings), menuName = EnvironmentVariables.SettingsPath + nameof(MainSceneSettings))]
    public class MainSceneSettings : ScriptableObject
    {
        [SerializeField] private MainSceneInstaller.Settings _settings;
        public MainSceneInstaller.Settings Settings => _settings;
    }
}