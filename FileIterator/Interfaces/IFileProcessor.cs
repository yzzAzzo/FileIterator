using System.Collections.ObjectModel;

namespace FileIterator.Interfaces;

public interface IFileProcessor
{
    public void ProcessFile(string filePath, ObservableCollection<string> extensionsToProcess);
}