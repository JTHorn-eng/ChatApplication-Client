using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication_Client
{
    class ClientSecurity
    {

        private byte[] publicKey;
        private byte[] IV;


        //CLIENT MUST
        /**
         * Application init 
         * Make server request, get encrypted AES symmetric key, IV and unencrypted RSA public key from server, get RSA private key from machine and decrypt SK and IV
         * 
         * Client sending message
         * 1) hash plainText, encrypt using RSA public key 
         * 2) AES encrypt message
         * 3) Send encrypted message:signature
         */



        /*
         * methods for retrieving keys are similar: if a key container with
         * specified name doesn't exist, then one is created, else
         * key is automatically loaded into the current AA object
         * 
         * THEREFORE ALWAYS CREATE A KEY FIRST
         */

        //delete key if overwritting key first before generating new one
        private static string GenKey(string containerName)
        {

            CspParameters p = new CspParameters();
            p.KeyContainerName = containerName;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(p);
            rsa.PersistKeyInCsp = false;

            rsa.Clear();
            CspParameters p2 = new CspParameters();
            p.KeyContainerName = containerName;

            RSACryptoServiceProvider rsa2 = new RSACryptoServiceProvider(p2);

            return rsa2.ToXmlString(true);
        }
        private static string RetrievePrivateKey(string containerName)
        {
            CspParameters p = new CspParameters();
            p.KeyContainerName = containerName;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(p);
            return rsa.ToXmlString(true);
        }

        //AES Encrypt
        public byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] IV)
        {
            if (plainText.Length <= 0 || key.Length <= 0 || IV.Length <= 0)
            {
                //create an AES object
                Aes aesAlg = Aes.Create();
                aesAlg.Key = key;
                aesAlg.IV = IV;

                //create an ecryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                //Create the streams used for encryption
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

        //AES Decrypt
        public string DecryptBytesToString_Aes(byte[] cipherText, byte[] key, byte[] IV)
        {
            string plainText = null;
            if (cipherText.Length <= 0 || key.Length <= 0 || IV.Length <= 0)
            {
                Aes aesAlg = Aes.Create();
                aesAlg.Key = key;
                aesAlg.IV = IV;

                //create an ecryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                //Create the streams used for encryption
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



        //RSA Encrypt
        public byte[] RSAEncryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
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



        //RSA Decrypt
        public byte[] RSADecryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
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

        public byte[] generateDigitalSignature(string plainText)
        {
            //hash message
            HashAlgorithm sha = new SHA1CryptoServiceProvider();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        }



    }
}