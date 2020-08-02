using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ChatClient
{
    public static class ClientSecurity
    {

        private static byte[] publicKey;
        private static byte[] IV;

        //Generates a new symmetric key and stores it in key container. If the key already
        //exists, then a new key is generated. Returns public-private key pair
   
        public static string GenKey(string containerName)
        {
            //Generate a new key container.
            CspParameters p = new CspParameters();
            p.KeyContainerName = containerName;

            //Generate public, private key pair
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(p);
            rsa.PersistKeyInCsp = false;

            //Overwrite key pair if same containerName is specified
            rsa.Clear();
            CspParameters p2 = new CspParameters();
            p.KeyContainerName = containerName;

            RSACryptoServiceProvider rsa2 = new RSACryptoServiceProvider(p2);
            return rsa2.ToXmlString(true);
        }

        //Retrieve key pair with specified key container name. Returns public-private key pair
        public static string RetrieveKeyPair(string containerName)
        {
            CspParameters p = new CspParameters();
            p.KeyContainerName = containerName;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(p);
            return rsa.ToXmlString(true);
        }

        //Only retrieve RSA public key
        public static string RetrievePublicKey(string containerName)
        {
            CspParameters p = new CspParameters();
            p.KeyContainerName = containerName;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(p);
            return rsa.ToXmlString(false);


        }

        //Encrypt plaintext using AES algorithm. Returns the ciphertext
        public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] IV)
        {
            if (plainText.Length <= 0 || key.Length <= 0 || IV.Length <= 0)
            {
                //create an AES object
                Aes aesAlg = Aes.Create();
                aesAlg.Key = key;
                aesAlg.IV = IV;

                //create an ecryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                //Create the byte streams used for encryption
                MemoryStream msEncrypt = new MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                StreamWriter swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(plainText);
                return msEncrypt.ToArray();
            }
            else
            {
                Console.WriteLine("Could not encrypt plainText");
                return new byte[0];
            }
        }

        //Decrypt ciphertext using AES algorithm. Returns the plaintext
        public static string DecryptBytesToString_Aes(byte[] cipherText, byte[] key, byte[] IV)
        {
            string plainText = null;
            if (cipherText.Length <= 0 || key.Length <= 0 || IV.Length <= 0)
            {
                Aes aesAlg = Aes.Create();
                aesAlg.Key = key;
                aesAlg.IV = IV;

                //create an ecryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                //Create the byte streams used for encryption
                MemoryStream msDecrypt = new MemoryStream(cipherText);
                CryptoStream cdDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                StreamReader srDecrypt = new StreamReader(cdDecrypt);
                plainText = srDecrypt.ReadToEnd();

                return plainText;
            }
            else
            {
                Console.WriteLine("Could not decrypt cipherText");
                return "";
            }
        }

        //Encrypt data using RSA algorithm. Returns the encrypted data
        public static byte[] RSAEncryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSA.ImportParameters(RSAKey);
                encryptedData = RSA.Encrypt(Data, DoOAEPPadding);
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Error encrypting message: " + e.Message);
            }
            return null;
        }

        //Decrypt data using RSA algorithm. Returns the decrypted data
        public static byte[] RSADecryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSA.ImportParameters(RSAKey);
                decryptedData = RSA.Decrypt(Data, DoOAEPPadding);
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Error encrypting message: " + e.Message);
            }
            return null;
        }

        //generate a digital signature for a message using the plaintext
        public static byte[] GenerateDigitalSignature(string plainText)
        {
            //hash message
            HashAlgorithm sha = new SHA1CryptoServiceProvider();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        }
    }
}