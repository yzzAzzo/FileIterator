using System.Windows;
using FileIterator.Interfaces;
using FileIterator.Models;
using FileIterator.ViewModels;

namespace FileIterator.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(new FileEncriptor(new XOREncryptor()), new DictionaryTraverser());
        }
    }
}