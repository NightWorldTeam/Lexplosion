namespace Lexplosion
{
    public enum ImportResult
    {
        Successful,
        ZipFileError,
        GameVersionError,
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
        Local
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
        UnknownError
    }

    public enum ModloaderType
    {
        None,
        Forge,
        Fabric
    }
}