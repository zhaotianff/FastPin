namespace FastPin.Api.Contracts;

public class FileCacheUpdateRequest
{
    public bool IsCached { get; set; }
    public byte[]? CachedFileData { get; set; }
}
