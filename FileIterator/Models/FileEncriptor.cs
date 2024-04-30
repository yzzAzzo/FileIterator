using FileIterator.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace FileIterator.Models;

public class FileEncriptor : IFileProcessor
{
    private readonly ISimpleSymmetricEncriptor _encriptor;
    private const byte KEY = 0x15;

    public FileEncriptor(ISimpleSymmetricEncriptor encriptor)
    {
        _encriptor = encriptor;
    }

    /// <summary>
    /// Performs an encryption on the file with the provided encryptor
    /// </summary>
    /// <param name="filePath">The path of the file to encrypt</param>
    /// <param name="extensionsToEncrypt">Collection of extensions that we want to encrypt.</param>
    public void ProcessFile(string filePath, ObservableCollection<string> extensionsToEncrypt)
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);

            if (extensionsToEncrypt.Contains(Path.GetExtension(filePath)))
            {
                byte[] encryptedData = _encriptor.DoCipher(bytes, KEY);
                File.WriteAllBytes(filePath, encryptedData);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could use Nlog for logging errors, but it's outside of the scope of this assessment.{e.Message}");
        }
    }
}