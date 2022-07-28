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

    public enum ModpacksCategories
    {
        All,
        Tech = 4472,
        Magic = 4473,
        Sci__Fi = 4474,
        AdventureAndRPG = 4475,
        Exploration = 4476,
        MiniGame = 4477,
        Quests = 4478,
        Hardcore = 4479,
        MapBased = 4480,
        Small_Light = 4481,
        ExtraLarge = 4482,
        Combat_PvP = 4483,
        Multiplayer = 4484,
        FTB = 4487,
        Skyblock = 4736,
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
        Fabric
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
        OnlyOnline
    }

    public enum AccountType
    {
        NoAuth,
        NightWorld,
        Mojang
    }

    public enum ModCategory
    {
        AllMods,
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
        Armor__Tools_and_Weapon,
        Server_Utility,
        Food,
        Redstone,
        Twitch_Integration,
        MCreator,
        Utility_CharAnd_QoL,
        Education
    }

    public enum ResourcePacksCategory
    {
        All_Resource_Packs,
        x16,
        x32,
        x64,
        x128,
        x256,
        x512_and_Higher,
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
        ModdedWorld,
    }

    public enum OnlineGameStatus
    {
        None,
        OpenWorld,
        ConnectToUser
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
}