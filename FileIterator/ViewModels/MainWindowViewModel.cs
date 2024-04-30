using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using FileIterator.Helpers;
using FileIterator.Interfaces;

namespace FileIterator.ViewModels;

public class MainWindowViewModel : PropertyChangedBase
{
    private readonly ITraverser _directoryTraverser;
    private readonly IFileProcessor _fileProcessor;

    public ObservableCollection<string> ExtensionsToEncryptCollection { get; set; }

    private bool _traverseComplete = true;
    public bool TraverseComplete
    {
        get => _traverseComplete;
        set
        {
            _traverseComplete = value;
            OnPropertyChanged();
        }
    }


    private string _rootPath;
    public string RootPath
    {
        get { return _rootPath; }
        set
        {
            _rootPath = value;
            OnPropertyChanged();
        }
    }
    public ICommand AddCommand => new RelayCommand(ExecuteAdd);
    public ICommand DeleteCommand => new RelayCommand(ExecuteDelete);
    public ICommand OpenFolderDialogCommand => new RelayCommand(ExecuteOpenFileDialog);
    public ICommand TraverseDictionariesCommand => new RelayCommand(ExecuteTraverseDictionaries);


	public MainWindowViewModel(IFileProcessor fileProcessor, ITraverser traverser)
    {
        _fileProcessor = fileProcessor;
        _directoryTraverser = traverser;
        _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        ExtensionsToEncryptCollection = new ObservableCollection<string>{".txt"};
    }

    /// <summary>
    /// Opens a Folder Dialog and saves the result to _rootPath.
    /// </summary>
    private void ExecuteOpenFileDialog(object parameter)
    {
        Microsoft.Win32.OpenFolderDialog dialog = new();

        dialog.Multiselect = false;
        dialog.Title = "Select a folder";

        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            RootPath = dialog.FolderName;
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
        TraverseComplete = false;
        await _directoryTraverser.TraverseDirectoriesAsync(_rootPath, ProcessDirectories);
        TraverseComplete = true;
        Trace.WriteLine("----------- Walk through completed -----------");
    }

    private async Task ProcessDirectories(string dictionaryPath)
    {
        try
        {
            await Task.Run(() =>
            {
                foreach (string filePath in Directory.EnumerateFiles(dictionaryPath))
                {
                    _fileProcessor.ProcessFile(filePath, ExtensionsToEncryptCollection);
                }
            });
        }
        catch (UnauthorizedAccessException)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("You don't have permission to perform this operation. Restart under administrator.", "Unauthorized Access", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(); 
            });
        }
    }
}