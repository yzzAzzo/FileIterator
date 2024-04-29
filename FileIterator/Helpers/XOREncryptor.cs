using FileIterator.Interfaces;

namespace FileIterator.Helpers;

public class XOREncryptor : ISimpleSymmetricEncriptor
{
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