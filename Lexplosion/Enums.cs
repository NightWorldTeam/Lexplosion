using System.ComponentModel;

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
        DirectoryCreateError
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
        NoConnect
    }

    public enum InstanceSource
    {
        Nightworld,
        Curseforge,
        Local,
        None
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
        UnknownError
    }

    public enum ModloaderType
    {
        None,
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

    public enum ModCategory
    {
        AllMods = -1,
        WorldGen,
        Technology,
        Magic,
        Storage,
        API_and_Library,
        Adventure_and_RPG,
        MapAndInformation,
        Cosmetics,
        Miscellaneous,
        Addons,
        Armor__Tools_and_Weapons,
        Server_Utility,
        Food,
        Redstone,
        Twitch_Integration,
        MCreator,
        Utility_CharAnd_QoL,
        Education
    }

    public enum SubCategoryWorldgen
    {
        Biomes,
        Ores_and_Resources,
        Structures,
        Dimensions,
        Mobs
    }

    public enum SubCategoryTechnology
    {
        Processing,
        PlayerTransport,
        Energy__Fluid__and_Item_Transport,
        Farming,
        Energy,
        Genetics,
        Automation
    }

    public enum SubCategoryAddons 
    {
        ThermalExpantion,
        Tinker___Constract,
        IndustrialCraft,
        Thaumcraft,
        BuildCraft,
        Forestry,
        BloodMagic,
        AppliedEnergistics2,
        CraftTweaker,
        Galacticraft,
        KubeJS
    }


    public enum ResourcePacksCategory
    {
        All_Resource_Packs,
        x16,
        x32,
        x64,
        x128,
        x256,
        x512Plus,
        Steampunk,
        PhotoRealistic,
        Modern,
        Mediaeval,
        Traditional,
        Animated,
        Miscellaneous,
        ModSupport,
        DataPacks,
        FontPacks
    }

    public enum WorldsCategory
    {
        All_Worlds,
        Adventure,
        Creation,
        GameMap,
        Parkour,
        Puzzle,
        Survival,
        ModdedWorld
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

    public enum CfSortBy
    {
        DateCreated,
        LastUpdated,
        Name,
        Popularity,
        TotalDownloads
    }
}