using System.Windows;
using FileIterator.Helpers;
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
            DataContext = new MainWindowViewModel(new XOREncryptor());
        }
    }
}