namespace Lexplosion
{
    public enum ImportResult
    {
        Successful,
        ZipFileError,
        GameVersionError,
        JavaDownloadError,
        IsOfflineMode,
        MovingFilesError,
        ServerFilesError,
        DirectoryCreateError,
        ConnectionClose
    }

    public enum ExportResult
    {
        Successful,
        TempPathError,
        FileCopyError,
        InfoFileError,
        ZipFileError
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

    public enum InstanceSource
    {
        Nightworld,
        Curseforge,
        Local,
        None
    }

    public enum CfSortField
    {
        Featured = 1,
        Popularity,
        LastUpdated,
        Name,
        Author,
        TotalDownloads,
        Category,
        GameVersion
    }

    public enum InstanceInit
    {
        Successful,
        DownloadFilesError,
        NightworldIdError,
        CursforgeIdError,
        ServerError,
        GuardError,
        VersionError,
        ForgeVersionError,
        GamePathError,
        ManifestError,
        JavaDownloadError,
        IsCancelled,
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
        Quilt = 3
    }

    public enum GameExtension
    {
        Optifine,
        Forge,
        Fabric = 4,
        Quilt
    }

    /// <summary>
    /// Типа аддонов с курсфорджа
    /// </summary>
    public enum AddonType
    {
        Unknown,
        Mods = 6,
        Resourcepacks = 12,
        Maps = 17
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

    public enum DownloadStageTypes
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
        Mojang,
        Microsoft
    }

    public enum OnlineGameStatus
    {
        None,
        OpenWorld,
        ConnectedToUser
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
        UncnownAddonType,
        FileVersionError,
        UrlError,
        FileNameError,
        UzipError,
        IsCanselled,
        UncnownError
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

    public enum DistributionState
    {
        InQueue,
        InProcess
    }

    public enum FileRecvReult
    {
        Successful,
        ConnectionClose,
        UnknownError
    }
}