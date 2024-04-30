using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Windows;
using FileIterator.Interfaces;

namespace FileIterator.Models;

public class DictionaryTraverser : ITraverser
{
    /// <summary>
    /// Asynchronously walks through the file structure, starting from the rootPath and processes each direcrtory with the Func that was given.
    /// </summary>
    /// <param name="rootPath">Starting point of traverse</param>
    /// <param name="processDirectory">Processing action for each directory</param>
    /// <returns></returns>
    public async Task TraverseDirectoriesAsync(string rootPath, Func<string, Task> processDirectory)
    {
        ConcurrentQueue<string> directories = new ConcurrentQueue<string>();
        directories.Enqueue(rootPath);

        //setting thread number above the actual physical core count wouldn't result in performance benefits when it comes to I/O tasks so limit it(tho it might be useful for encription).
        int maxThreads = int.TryParse(ConfigurationManager.AppSettings["MaxThreads"], out int result)
                         && result <= Environment.ProcessorCount ? result : Environment.ProcessorCount;

        var tasks = Enumerable.Range(0, maxThreads).Select(_ => Task.Run(async () =>
        {
            while (!directories.IsEmpty)
            {
                if (directories.TryDequeue(out string currentDirectory))
                {
                    await processDirectory(currentDirectory);
                    try
                    {
                        foreach (string subDirectory in Directory.EnumerateDirectories(currentDirectory))
                        {
                            directories.Enqueue(subDirectory);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("You don't have permission to perform this operation. Please contact your system administrator.", "Unauthorized Access", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Could use Nlog for logging errors, but it's outside of the scope of this assessment.{e.Message}");
                    }
                }
            }
        }));

        await Task.WhenAll(tasks);
    }
}