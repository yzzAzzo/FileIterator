using FileIterator.Interfaces;

namespace FileIterator.Models;

public class XOREncryptor : ISimpleSymmetricEncriptor
{
    /// <summary>
    /// Performs a XOR encryption on the data with the given key
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public byte[] DoCipher(byte[] data, byte key)
    {
        byte[] cipheredData = new byte[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            cipheredData[i] = (byte)(data[i] ^ key);
        }

        return cipheredData;
    }
}