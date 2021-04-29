namespace Lexplosion.Logic
{
    enum ImportResult : byte
    {
        Successful,
        ZipFileError,
        GameVersionError,
        IsOfflineMode,
        MovingFilesError,
        ServerFilesError,
        DirectoryCreateError
    }

    enum ExportResult : byte
    {
        Successful,
        TempPathError,
        FileCopyError,
        InfoFileError,
        ZipFileError

    }

    public enum AuthCode : byte
    {
        Successfully,
        DataError,
        NoConnect
    }

}