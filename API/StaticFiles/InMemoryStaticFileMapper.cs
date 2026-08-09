namespace API.StaticFiles;

public class InMemoryStaticFileMapper : IStaticFileMapper
{
    private readonly Dictionary<string, string> _fileMapping = new();
    
    public void MapStaticFilesDirectory(string directoryPath, string mappedPath, bool recursive = true)
    {
        var dirInfo = new DirectoryInfo(AppContext.BaseDirectory + directoryPath);
        if (!dirInfo.Exists) throw new DirectoryNotFoundException();
        
        if (recursive) MapDirectory(dirInfo, mappedPath);
        else MapDirectoryFiles(dirInfo, mappedPath);
    }

    private void MapDirectoryFiles(DirectoryInfo dirInfo, string currPath)
    {
        foreach (var file in dirInfo.EnumerateFiles())
        {
            _fileMapping.Add(currPath + "/" + file.Name, file.FullName);
        }
    }

    private void MapDirectory(DirectoryInfo dirInfo, string currPath)
    {
        MapDirectoryFiles(dirInfo, currPath);
        foreach (var dir in dirInfo.EnumerateDirectories())
        {
            MapDirectory(dir, currPath + "/" + dir.Name);
        }
    }

    public string? GetStaticFile(string filePath)
    {
        return _fileMapping.GetValueOrDefault(filePath);
    }
}