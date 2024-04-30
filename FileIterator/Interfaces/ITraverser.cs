namespace FileIterator.Interfaces;

public interface ITraverser
{
    public Task TraverseDirectoriesAsync(string rootPath, Func<string, Task> processFunc);
}