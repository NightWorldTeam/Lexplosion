namespace Lexplosion.Logic
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
        Exploration = 4476,
        ExtraLarge = 4482,
        Multiplayer = 4484
    }


}