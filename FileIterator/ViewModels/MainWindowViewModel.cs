using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Input;
using FileIterator.Helpers;

namespace FileIterator.ViewModels;

public class MainWindowViewModel : PropertyChangedBase
{
    private string _path;

    public ObservableCollection<string> ExtensionsToEncryptCollection { get; set; }
    

    public string Path
    {
        get { return _path; }
        set
        {
            _path = value;
            OnPropertyChanged();
        }
    }
    private ICommand _addCommand;
    public ICommand AddCommand
    {
        get
        {
            if (_addCommand == null)
            {
                _addCommand = new RelayCommand(ExecuteAdd);
            }

            return _addCommand;
        }
    }

    private ICommand _deleteCommand;
    public ICommand DeleteCommand
    {
        get
        {
            if (_deleteCommand == null)
            {
                _deleteCommand = new RelayCommand(ExecuteDelete);
            }
            return _deleteCommand;
        }
    }

    private ICommand _openFolderDialogCommand;
    public ICommand OpenFolderDialogCommand
    {
        get
        {
            if (_openFolderDialogCommand == null)
            {
                _openFolderDialogCommand = new RelayCommand(ExecuteOpenFileDialog);
            }
            return _openFolderDialogCommand;
        }
    }

    private ICommand _traverseDictionariesCommand;
    public ICommand TraverseDictionariesCommand
    {
        get
        {
            if (_traverseDictionariesCommand == null)
            {
                _traverseDictionariesCommand = new RelayCommand(ExecuteTraverseDictionaries);
            }
            return _traverseDictionariesCommand;
        }
    }

	public MainWindowViewModel()
    {
        _path = @"E:\Jedi Survivor"; /* Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);*/
        ExtensionsToEncryptCollection = new ObservableCollection<string>();
    }

   

    /// <summary>
    /// Opens a Folder Dialog and saves the result to .
    /// </summary>
    private void ExecuteOpenFileDialog(object parameter)
    {
        // Configure open folder dialog box
        Microsoft.Win32.OpenFolderDialog dialog = new();

        dialog.Multiselect = false;
        dialog.Title = "Select a folder";

        // Show open folder dialog box
        bool? result = dialog.ShowDialog();

        // Process open folder dialog box results
        if (result == true)
        {
            // Get the selected folder
            _path = dialog.FolderName;
        }
    }

    private void ExecuteAdd(object parameter)
    {
        if (parameter is string selectedItem)
        {
            ExtensionsToEncryptCollection.Add(selectedItem);
        }
    }

    private void ExecuteDelete(object parameter)
    {
        if (parameter is string selectedItem)
        {
            ExtensionsToEncryptCollection.Remove(selectedItem);
        }
    }

    private async void ExecuteTraverseDictionaries(object parameter)
    {
        await ExecuteTraverseDictionariesAsync();
    }

    private async Task ExecuteTraverseDictionariesAsync()
    {
        // Perform file system traversal asynchronously
        await Task.Run(() => TraverseDirectories(_path));

        Trace.WriteLine("File system walk-through completed.");
    }

    private void TraverseDirectories(string path)
    {
        BlockingQueue<string> directories = new BlockingQueue<string>();
        var threads = new Thread[Environment.ProcessorCount];

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() => ProcessDirectories(directories));
            threads[i].Start();
        }

        directories.Enqueue(path);

        foreach (var thread in threads)
        {
            thread.Join();
        }
    }

    private void ProcessDirectories(BlockingQueue<string> directories)
    {
        while (true)
        {
            string directory;
            // Dequeue a directory to process
            if (!directories.TryDequeue(out directory))
                return; // No more directories to process

            // Process files in the current directory
            //TODO EnumerateFiles instead
            string[] files = Directory.GetFiles(directory);
            foreach (string file in files)
            {
                Trace.WriteLine("Processing file: " + file);

                var bytes = File.ReadAllBytes(file);

                //TODO put this key somewhere.
                byte key = 0x1F;

                byte[] encryptedData = XOREncryptor.DoCipher(bytes,key);

                // Process the file as needed
            }

            // Enqueue subdirectories for further processing
            string[] subDirectories = Directory.GetDirectories(directory);

            //Return because otherview deadlock waiting for pulse
            if (!subDirectories.Any())
            {
                return;
            }

            foreach (string subDir in subDirectories)
            {
                directories.Enqueue(subDir);
            }
        }
    }
}


// BlockingQueue implementation for thread-safe queue operations
public class BlockingQueue<T>
{
    private readonly Queue<T> _queue = new ();
    private readonly object _syncRoot = new ();

    public void Enqueue(T item)
    {
        lock (_syncRoot)
        {
            _queue.Enqueue(item);
            Monitor.Pulse(_syncRoot);
        }
    }

    public bool TryDequeue(out T result)
    {
        lock (_syncRoot)
        {
            while (_queue.Count == 0)
            {
                Monitor.Wait(_syncRoot);
            }

            result = _queue.Dequeue();
            return true;
        }
    }
}