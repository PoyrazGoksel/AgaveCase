using Installers;
using UnityEngine;
using Utils;

namespace Datas.Settings
{
    [CreateAssetMenu(fileName = nameof(MainSceneSettings), menuName = EnvironmentVariables.SettingsPath + nameof(MainSceneSettings))]
    public class MainSceneSettings : ScriptableObject
    {
        public static readonly string Path = EnvironmentVariables.SettingsPath + nameof(MainSceneSettings);
        
        [SerializeField] private MainSceneInstaller.Settings _settings;
        public MainSceneInstaller.Settings Settings => _settings;
    }
}