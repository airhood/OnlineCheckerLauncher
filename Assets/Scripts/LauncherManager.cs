using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;
using System.Diagnostics;

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
}

public class LauncherManager : MonoBehaviour
{
    [Header("Version")]
    public VersionType versionType;
    public int major;
    public int minor;
    public int patch;
    public bool isTestVersion;

    public struct userAttributes { }
    public struct appAttributes { }

    void Awake()
    {
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());

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
