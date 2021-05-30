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


}