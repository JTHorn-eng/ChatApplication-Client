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
        private static RSAParameters RSAKeyInfo;

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

        //Only retrieve RSA public key as a string literal
        public static string RetrievePublicKey(string containerName)
        {
            CspParameters p = new CspParameters();
            p.KeyContainerName = containerName;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(p);
            return rsa.ToXmlString(false);


        }

        //Retrieve RSA public key only as an RSAParameter object
        private static RSAParameters GetPublicKeyInfo(string containerName)
        {
            CspParameters p = new CspParameters();
            p.KeyContainerName = containerName;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(p);
            return rsa.ExportParameters(false);
            
        }

        //Retrieve RSA key pair as an RSAParameter object
        private static RSAParameters GetKeyPairInfo(string containerName)
        {
            CspParameters p = new CspParameters();
            p.KeyContainerName = containerName;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(p);
            return rsa.ExportParameters(true);

        }

        //Encrypt plaintext using AES algorithm. Returns the ciphertext
        private static byte[] EncryptStringToBytes_Aes(string plainText)
        {
            if (plainText.Length >= 0)
            {
                //create an AES object, key and IV
                Aes aesAlg = Aes.Create();
             

                //create an ecryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                //Create the byte streams used for encryption
                MemoryStream msEncrypt = new MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                StreamWriter swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(plainText);
                publicKey = aesAlg.Key;
                IV = aesAlg.IV;
                return msEncrypt.ToArray();
            }
            else
            {
                Console.WriteLine("Could not encrypt plainText");
                return new byte[0];
            }
        }

        //Generates fully encrypted message
        //Currently uses client username as containerName
        //OUTPUT-FORMAT: cipher-text|encrypted-AES-public-key|encrypted-IV
        public static string EncryptMessage(string plainText, string containerName)
        {

            return Encoding.UTF8.GetString(EncryptStringToBytes_Aes(plainText)) + "|"
                  + Encoding.UTF8.GetString(RSAEncryption(publicKey, GetPublicKeyInfo(containerName), false)) + "|"
                  + Encoding.UTF8.GetString(RSAEncryption(IV, GetPublicKeyInfo(containerName), false));
        }

        //Decrypt fully encrypted message
        //Currently uses client username as containerName
        //OUTPUT-FORMAT: plain-text
        public static string DecryptMessage(string inputText, string containerName)
        {
            //Decrypt AES public key and IV
            byte[] publicKey = Encoding.UTF8.GetBytes(inputText.Split('|')[1]);
            byte[] IV = Encoding.UTF8.GetBytes(inputText.Split('|')[2]);
            publicKey = RSADecryption(publicKey, GetKeyPairInfo(containerName), false);
            IV = RSADecryption(IV, GetKeyPairInfo(containerName), false);

            //return AES decrypted cipher text
            return DecryptBytesToString_Aes(Encoding.UTF8.GetBytes(inputText), publicKey, IV);
        }



        //Decrypt ciphertext using AES algorithm. Returns the plaintext
        private static string DecryptBytesToString_Aes(byte[] cipherText, byte[] key, byte[] IV)
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
                Console.WriteLine("[INFO] Could not decrypt cipherText");
                return "";
            }
        }

        //Encrypt data using RSA algorithm. Returns the encrypted data
        public static byte[] RSAEncryption(byte[] Data, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                
                RSA.ImportParameters(RSAKeyInfo);
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
        private static byte[] RSADecryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
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