namespace Lexplosion
{
    enum ImportResult
    {
        Successful,
        ZipFileError,
        GameVersionError,
        IsOfflineMode,
        MovingFilesError,
        ServerFilesError,
        DirectoryCreateError
    }

    enum ExportResult
    {
        Successful,
        TempPathError,
        FileCopyError,
        InfoFileError,
        ZipFileError
    }

    enum AuthCode
    {
        Successfully,
        DataError,
        NoConnect
    }

    enum InstanceType
    {
        Nightworld,
        Local,
        Curseforge
    }

    enum ModpacksCategories
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


}