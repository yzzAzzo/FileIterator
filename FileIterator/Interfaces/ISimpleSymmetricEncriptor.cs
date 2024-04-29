namespace FileIterator.Interfaces;

public interface ISimpleSymmetricEncriptor
{
    byte[] DoCipher(byte[] data, byte key);
}