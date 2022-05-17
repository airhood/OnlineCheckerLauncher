using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;
using System.Diagnostics;
using UnityEngine.UI;

public enum GameUpdate
{
    NoUpdate, RequiredUpdate, NonRequiredUpdate, Error
}

public enum VersionType
{
    Dev, Alpha, Beta, Release, PreRelease
}

public struct Version
{
    public VersionType versionType;
    public int major, minor, patch;

    public bool isTestVersion;

    public bool requiredUpdate;

    public Version(VersionType versionType, int major, int minor, int patch, bool isTestVersion, bool requiredUpdate)
    {
        this.versionType = versionType;
        this.major = major;
        this.minor = minor;
        this.patch = patch;
        this.isTestVersion = isTestVersion;
        this.requiredUpdate = requiredUpdate;
    }

    public Version(VersionType versionType, int major, int minor, int patch, bool isTestVersion)
    {
        this.versionType = versionType;
        this.major = major;
        this.minor = minor;
        this.patch = patch;
        this.isTestVersion = isTestVersion;
        requiredUpdate = true;
    }

    public string GetVersionCode()
    {
        return major + "." + minor + "." + patch;
    }

    public static bool operator >(Version version1, Version version2)
    {
        if (version1.major > version2.major)
        {
            return true;
        }
        else if (version1.major == version2.major)
        {
            if (version1.minor > version2.minor)
            {
                return true;
            }
            else if (version1.minor == version2.minor)
            {
                if (version1.patch > version2.patch)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public static bool operator <(Version version1, Version version2)
    {
        if (version1.major < version2.major)
        {
            return true;
        }
        else if (version1.major == version2.major)
        {
            if (version1.minor < version2.minor)
            {
                return true;
            }
            else if (version1.minor == version2.minor)
            {
                if (version1.patch < version2.patch)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public static bool operator ==(Version version1, Version version2)
    {
        if (version1.major == version2.major)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator !=(Version version1, Version version2)
    {
        if (version1.major != version2.major)
        {
            return true;
        }
        else if (version1.minor != version2.minor)
        {
            return true;
        }
        else if (version1.patch != version2.patch)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator >=(Version version1, Version version2)
    {
        if (version1.major > version2.major)
        {
            return true;
        }
        else if (version1.major == version2.major)
        {
            if (version1.minor > version2.minor)
            {
                return true;
            }
            else if (version1.minor == version2.minor)
            {
                if (version1.patch >= version2.patch)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public static bool operator <=(Version version1, Version version2)
    {
        if (version1.major < version2.major)
        {
            return true;
        }
        else if (version1.major == version2.major)
        {
            if (version1.minor < version2.minor)
            {
                return true;
            }
            else if (version1.minor == version2.minor)
            {
                if (version1.patch <= version2.patch)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    Version VersionCodeToVersion(string versionCode)
    {
        Version version = new Version();

        int start;
        if (versionCode.Substring(0, 3) == "Dev")
        {
            version.versionType = VersionType.Dev;
            start = 3;
        }
        else if (versionCode.Substring(0, 5) == "Alpha")
        {
            version.versionType = VersionType.Alpha;
            start = 5;
        }
        else if (versionCode.Substring(0, 4) == "Beta")
        {
            version.versionType = VersionType.Beta;
            start = 4;
        }
        else if (versionCode.Substring(0, 7) == "Release")
        {
            version.versionType = VersionType.Release;
            start = 7;
        }
        else if (versionCode.Substring(0, 11) == "Pre-release")
        {
            version.versionType = VersionType.PreRelease;
            start = 11;
        }
        else
        {
            return version;
        }

        version.major = versionCode[start + 1];
        version.minor = versionCode[start + 3];
        version.patch = versionCode[start + 5];

        return version;
    }
}

public class LauncherManager : MonoBehaviour
{
    [Header("Version")]
    public VersionType versionType;
    public int major;
    public int minor;
    public int patch;
    public bool isTestVersion;

    [Header("UI")]
    public Dropdown versionSelect;

    public struct userAttributes { }
    public struct appAttributes { }

    void Awake()
    {
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
    }

    void Start()
    {
        Version version = new Version(versionType, major, minor, patch, isTestVersion);

        switch (CheckVersion(version))
        {
            case GameUpdate.NoUpdate:
                break;
            case GameUpdate.RequiredUpdate:
                CallUpdate();
                break;
            case GameUpdate.NonRequiredUpdate:
                AskForUpdate(GetNewVersion());
                break;
        }
    }

    private void SetFunction_UI()
    {
        versionSelect.onValueChanged.AddListener(delegate
        {
            Function_Dropdown(versionSelect);
        });
    }

    private void Function_Dropdown(Dropdown select)
    {
        //select.options[select.value].text
    }

    GameUpdate CheckVersion(Version version)
    {
        Version newVersion = GetNewVersion();

        if (newVersion.Equals(version))
        {
            return GameUpdate.NoUpdate;
        }
        else
        {
            if (version < newVersion)
            {
                if (newVersion.requiredUpdate)
                {
                    return GameUpdate.RequiredUpdate;
                }
                return GameUpdate.NonRequiredUpdate;
            }
            return GameUpdate.Error;
        }
    }

    void CallUpdate()
    {
        string path = Application.dataPath + "/../GameUpdater.exe";
        Process.Start(path);
        Application.Quit();
    }

    void AskForUpdate(Version version)
    {

    }

    Version GetNewVersion()
    {
        Version newVersion = new Version();

        string versionType = ConfigManager.appConfig.GetString("gameVersionType");

        switch (versionType)
        {
            case "Dev":
                newVersion.versionType = VersionType.Dev;
                break;
            case "Alpha":
                newVersion.versionType = VersionType.Alpha;
                break;
            case "Beta":
                newVersion.versionType = VersionType.Beta;
                break;
            case "Release":
                newVersion.versionType = VersionType.Release;
                break;
            case "Pre-release":
                newVersion.versionType = VersionType.PreRelease;
                break;
        }

        newVersion.major = ConfigManager.appConfig.GetInt("gameVersionCodeMajor");
        newVersion.minor = ConfigManager.appConfig.GetInt("gameVersionCodeMinor");
        newVersion.patch = ConfigManager.appConfig.GetInt("gameVersionCodePatch");
        newVersion.isTestVersion = ConfigManager.appConfig.GetBool("gameVersionCodeIsTestVersion");

        return newVersion;
    }
}
