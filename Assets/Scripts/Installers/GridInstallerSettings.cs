using UnityEngine;
using Utils;

namespace Installers
{
    [CreateAssetMenu(fileName = nameof(GridInstallerSettings), menuName = EnvironmentVariables.SettingsPath + nameof(GridInstallerSettings))]
    public class GridInstallerSettings : ScriptableObject
    {
        [SerializeField] private Grid2DInstaller.Settings _settings;
        public Grid2DInstaller.Settings Settings => _settings;
    }
}