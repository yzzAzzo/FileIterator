using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Input;
using FileIterator.Helpers;
using FileIterator.Interfaces;

namespace FileIterator.ViewModels;

public class MainWindowViewModel : PropertyChangedBase
{
    private const byte KEY = 0x1F;
    private ISimpleSymmetricEncriptor _encriptor;

    public ObservableCollection<string> ExtensionsToEncryptCollection { get; set; }

    private string _directoryPath;
    public string DirectoryPath
    {
        get { return _directoryPath; }
        set
        {
            _directoryPath = value;
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

	public MainWindowViewModel(ISimpleSymmetricEncriptor encriptor)
    {
        _encriptor = encriptor;
        _directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        ExtensionsToEncryptCollection = new ObservableCollection<string>{".txt"};
    }

    /// <summary>
    /// Opens a Folder Dialog and saves the result to _directoryPath.
    /// </summary>
    private void ExecuteOpenFileDialog(object parameter)
    {
        Microsoft.Win32.OpenFolderDialog dialog = new();

        dialog.Multiselect = false;
        dialog.Title = "Select a folder";

        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            DirectoryPath = dialog.FolderName;
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
        await Task.Run(() => TraverseDirectories(_directoryPath));

        Trace.WriteLine("File system walk-through completed.");
    }

    private void TraverseDirectories(string path)
    {
        BlockingQueue<string> directories = new BlockingQueue<string>();

        //setting thread number above the actual physical core count wouldn't result in performance benefits when it comes to I/O tasks so limit it.
        int maxThreads = int.TryParse(ConfigurationManager.AppSettings["MaxThreads"], out int result) 
                         && result <= Environment.ProcessorCount ? result : Environment.ProcessorCount;

        var threads = new Thread[maxThreads];

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
            
            if (!directories.TryDequeue(out directory))
                return; 

            string[] files = Directory.GetFiles(directory);

            foreach (string file in files)
            {
                Trace.WriteLine("Processing file: " + file);

                var bytes = File.ReadAllBytes(file);

                if (ExtensionsToEncryptCollection.Contains(Path.GetExtension(file)))
                {
                    byte[] encryptedData = _encriptor.DoCipher(bytes, KEY);

                    //We overwrite so we won't flood with files every time we do an encrypt/decrypt.
                    File.WriteAllBytes(file, encryptedData); 
                }
            }

            // queue subdirectories for further processing
            string[] subDirectories = Directory.GetDirectories(directory);

            //Return because otherwise deadlock waiting for pulse
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