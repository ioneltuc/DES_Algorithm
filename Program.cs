using DES_Algorithm;
using System.Text;

//Encryption

//KEY HERE
string key = "bestKey1";

if (key.Length < 8)
    key = key.PadRight(8, ' ');

string keyInHex = Converter.FromPlainTextToHex(key);

var subKeysGenerator = new SubKeysGenerator(keyInHex);

List<SubKey> subKeys = subKeysGenerator.EncryptionPhaseI();

var blockEncoder = new BlockEncoder(subKeys);

string encryptedMessage = "";

//MESSAGE HERE
string message = "Ioneltuc xD";
int messageLength = 8;

for (int i = 0; i < message.Length; i += 8)
{
    if (message.Length - i < 8)
        messageLength = message.Length - i;

    string messageBlock = message.Substring(i, messageLength);

    if (messageBlock.Length < 8)
        messageBlock = messageBlock.PadRight(8, ' ');

    string messageInHex = Converter.FromPlainTextToHex(messageBlock);
    blockEncoder.HexadecimalMessage = messageInHex;
    encryptedMessage += blockEncoder.EncryptBlock();
}

Console.WriteLine($"\n\nFinal permutation: {encryptedMessage}");
Console.WriteLine($"\nMessage in hexadecimal (cipher text): {Converter.BinaryStringToHexString(encryptedMessage)}");

//Decryption

var blockDencoder = new BlockDecoder(subKeys);
string messageToDecrypt = encryptedMessage;
string decryptedMessage = "";
int messageToDecryptBlockLength = 64;

for (int i = 0; i < messageToDecrypt.Length; i += 64)
{
    string messageToDecryptBlock = messageToDecrypt.Substring(i, messageToDecryptBlockLength);

    blockDencoder.BinaryMessage = messageToDecryptBlock;
    decryptedMessage += blockDencoder.DecryptBlock();
}

Console.WriteLine($"\nDecrypted message: {Encoding.ASCII.GetString(Converter.GetBytesFromBinaryString(decryptedMessage))}");