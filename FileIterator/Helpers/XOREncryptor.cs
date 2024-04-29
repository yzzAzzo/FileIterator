namespace FileIterator.Helpers;

public static class XOREncryptor
{
    public static byte[] DoCipher(byte[] data, byte key)
    {
        byte[] cipheredData = new byte[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            cipheredData[i] = (byte)(data[i] ^ key); 
        }

        return cipheredData;
    }

    
}