using Datas.Settings;
using Installers;

namespace Utils
{
    public static class EnvironmentVariables
    {
        public const string SettingsPath = "Settings/";
        public const string LevelsPath = SettingsPath + "Levels/";
        public const string SaveFileExt = ".sav";
        public const string MainSceneSettingsPath =
        SettingsPath + nameof(MainSceneSettings);
        public const string GridInstallerSettingsPath =
        SettingsPath + nameof(GridInstallerSettings);
    }
}