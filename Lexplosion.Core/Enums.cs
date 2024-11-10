namespace Lexplosion
{
    public enum ImportResult
    {
        Successful,
        ZipFileError,
        GameVersionError,
        ManifestError,
        JavaDownloadError,
        IsOfflineMode,
        MovingFilesError,
        DownloadError,
        DirectoryCreateError,
        WrongUrl,
        UnknownFileType,
        Canceled,
        UnknownError
    }

    public enum ExportResult
    {
        Successful,
        TempPathError,
        FileCopyError,
        InfoFileError,
        ZipFileError,
        NotExistsValidAccount
    }

    public enum AuthCode
    {
        Successfully,
        DataError,
        NoConnect,
        TokenError,
        SessionExpired,
        NeedMicrosoftAuth
    }

    public static class EnumManager
    {
        public static ProjectSource InstanceSourceToProjectSource(InstanceSource instanceSource)
        {
            switch (instanceSource)
            {
                case InstanceSource.Curseforge:
                    return ProjectSource.Curseforge;
                case InstanceSource.Modrinth:
                    return ProjectSource.Modrinth;
                default:
                    return ProjectSource.None;
            }
        }

        public static AddonType ToAddonType(this CfProjectType cfProjectType)
        {
            switch (cfProjectType)
            {
                case CfProjectType.Mods:
                    return AddonType.Mods;
                case CfProjectType.Resourcepacks:
                    return AddonType.Resourcepacks;
                case CfProjectType.Maps:
                    return AddonType.Maps;
                default:
                    return AddonType.Unknown;
            }
        }

        public static CfProjectType ToCfProjectType(this AddonType cfProjectType)
        {
            switch (cfProjectType)
            {
                case AddonType.Mods:
                    return CfProjectType.Mods;
                case AddonType.Resourcepacks:
                    return CfProjectType.Resourcepacks;
                case AddonType.Maps:
                    return CfProjectType.Maps;
                default:
                    return CfProjectType.Mods;
            }
        }

        public static Modloader ToModloader(this ClientType cfProjectType)
        {
            switch (cfProjectType)
            {
                case ClientType.Forge:
                    return Modloader.Forge;
                case ClientType.Fabric:
                    return Modloader.Fabric;
                case ClientType.Quilt:
                    return Modloader.Quilt;
                default:
                    return Modloader.Forge;
            }
        }
    }

    public enum InstanceSource
    {
        Nightworld,
        Curseforge,
        Local,
        Modrinth,
        FreeSource,
        None = 255
    }

    public enum ProjectSource
    {
        Curseforge,
        Modrinth,
        None = 255
    }

    public enum CfSortField
    {
        Featured = 1,
        Popularity,
        LastUpdated,
        Name,
        //Author,
        TotalDownloads = 6,
        //Category,
        //GameVersion
    }

    public enum ModrinthSortField
    {
        Relevance,
        Downloads,
        Newest,
        Updated,
        Follows
    }

    public enum InstanceInit
    {
        Successful,
        DownloadFilesError,
        NightworldIdError,
        CurseforgeIdError,
        ServerError,
        GuardError,
        VersionError,
        ForgeVersionError,
        GamePathError,
        ManifestError,
        JavaDownloadError,
        IsCancelled,
        MoveFilesError,
        UnknownError
    }

    public enum ClientType
    {
        Vanilla,
        Forge,
        Fabric = 4,
        Quilt
    }

    public enum AdditionalInstallerType
    {
        Optifine
    }

    public enum GameType
    {
        Vanilla,
        Modded
    }

    public enum Modloader
    {
        Forge = 1,
        Fabric = 4,
        Quilt
    }

    public enum GameExtension
    {
        Optifine,
        Forge,
        Fabric = 4,
        Quilt,
        Neoforged
    }

    /// <summary>
    /// Типы аддонов. Численно совместимы с курсфорджем
    /// </summary>
    public enum AddonType
    {
        Unknown,
        Mods = 6,
        Resourcepacks = 12,
        Maps = 17,
        Shaders = 6552
    }

    /// <summary>
    /// Тип проекта с курсфорджа.
    /// </summary>
    public enum CfProjectType
    {
        Mods = 6,
        Resourcepacks = 12,
        Maps = 17,
        Modpacks = 4471
    }

    public enum StageType
    {
        Prepare,
        Client,
        Java
    }

    public enum ActivityStatus
    {
        Offline,
        Online,
        InGame,
        NotDisturb,
    }

    public enum AccountType
    {
        NoAuth,
        NightWorld,
        Microsoft,
        Mojang
    }

    public enum InstanceChangelog
    {
        Fixes,
        Changes,
        ModsRemoved,
        ModsAdded,
        ModsUpdated,
        ModsDowngraded,
        ModloaderUpdated,
        ModloaderDowngraded,
    }

    public enum UpdateType
    {
        Alpha,
        Beta,
        Release
    }

    public enum DownloadAddonRes
    {
        Successful,
        ProjectDataError,
        FileIdError,
        DownloadError,
        unknownAddonType,
        FileVersionError,
        UrlError,
        FileNameError,
        UzipError,
        IsCanselled,
        unknownError
    }

    /// <summary>
    /// Стадии скачивания файла.
    /// </summary>
    public enum DownloadFileProgress
    {
        /// <summary>
        /// Обновление процентов.
        /// </summary>
        PercentagesChanged,
        /// <summary>
        /// Скачивнаие завершено удачно.
        /// </summary>
        Successful,
        /// <summary>
        /// Скачивнаие завершено с ошибкой.
        /// </summary>
        Error
    }

    public enum ReleaseType
    {
        Alpha,
        Beta,
        Release
    }

    public enum MicrosoftAuthRes
    {
        UnknownError,
        UserDenied,
        Minor,
        NoXbox,
        Successful
    }

    public enum DownloadShareState
    {
        InQueue,
        InConnect,
        InProcess,
        PostProcessing
    }

    public enum FileRecvResult
    {
        Successful,
        ConnectionClose,
        UnknownError,
        Canceled
    }
}
