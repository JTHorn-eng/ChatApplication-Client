using ChatClient;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ChatServer
{
    public static class ClientSecurity
    {
        //store encrypted symmetric keys
        static byte[] encryptedSymmetricKey;
        static byte[] encryptedSymmetricIV;
        static byte[] SymmetricKey;
        static byte[] SymmetricIV;
        static byte[] generatedCipherText;

        static string RSAPublicKey; //XAML representation of client's public key
        static RSAParameters RSAPrivateKey;

        


        public static string Encrypt(string plaintext, string publicKey)
        {
            generatedCipherText = AESEncrypt(plaintext);
            EncryptPublicKeyAndIV(publicKey);
            return "<A>" + Encoding.ASCII.GetString(generatedCipherText) + "<B>" + Encoding.ASCII.GetString(encryptedSymmetricKey) + "<C>" + Encoding.ASCII.GetString(encryptedSymmetricIV) + "<D>";
        }
        public static string Decrypt(string cipherText)
        {
            Console.WriteLine("Cipher text: " + cipherText);
            byte[] encPubKey;
            byte[] encIV;
            string encPubKeyVerbatim = "";
            string encIVVerbatim = "";
            for (int x = cipherText.IndexOf("<B>") + "<B>".Length; x < cipherText.LastIndexOf("<C>"); x++)
            {
                encPubKeyVerbatim += cipherText[x];
            }
            for (int x = cipherText.IndexOf("<C>") + "<C>".Length; x < cipherText.LastIndexOf("<D>"); x++)
            {
                encIVVerbatim += cipherText[x];
            }
            
            encPubKey = Encoding.ASCII.GetBytes(encPubKeyVerbatim);
            encIV = Encoding.ASCII.GetBytes(encIVVerbatim);
            Console.WriteLine("LEN OF PUB KEY: " + encPubKey.Length);
            DecryptPublicKeyAndIV(encPubKey, encIV);
            return AESDecrypt(Encoding.ASCII.GetBytes(cipherText));
        }

        private static void EncryptPublicKeyAndIV(string publicKey)
        {

            //Create a new instance of RSACryptoServiceProvider.

            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            RSA.FromXmlString(publicKey);
          
            //Encrypt AES public key and IV
            encryptedSymmetricKey = RSA.Encrypt(SymmetricKey, false);
            encryptedSymmetricIV = RSA.Encrypt(SymmetricIV, false);

        }
        private static void DecryptPublicKeyAndIV(byte[] PublicKey, byte[] IV)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            //Import key parameters into RSA
            RSA.ImportParameters(RSAPrivateKey);

            Console.WriteLine("LEN OF PUB KEY RECEIVED: " + PublicKey.Length);

            SymmetricKey = RSA.Decrypt(PublicKey, false);
            SymmetricIV = RSA.Decrypt(IV, false);
        }
        
        public static string GetRSAPublicKey(string container)
        {
            //Create crypto service provider parameters
            CspParameters p = new CspParameters();
            p.KeyContainerName = container;

            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(p);
            RSA.PersistKeyInCsp = false;

            return RSA.ToXmlString(false);
        }

        private static RSAParameters GetRSAPrivateKey(string container)
        {
            //Create crypto service provider parameters
            CspParameters p = new CspParameters();
            p.KeyContainerName = container;

            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(p);
            RSA.PersistKeyInCsp = false;


            return RSA.ExportParameters(true);
        }


        //Establish AES public key and IV
        public static void Init(string keyContainer)
        {
            Aes aes = Aes.Create();
            RSAPublicKey = GetRSAPublicKey(keyContainer);
            RSAPrivateKey = GetRSAPrivateKey(keyContainer);
            aes.GenerateKey();
            aes.GenerateIV();
            Console.WriteLine("[INFO] RSAPublicKey : " + RSAPublicKey);
            Console.WriteLine("[INFO] RSAPrivateKey : " + RSAPrivateKey);

            Console.WriteLine("[INFO] aes key : " + aes.Key);
            Console.WriteLine("[INFO] aes IV : " + aes.IV);
            SymmetricKey = aes.Key;
            SymmetricIV = aes.IV;

        }

        private static byte[] AESEncrypt(string plainText)
        {
            byte[] encrypted;

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {


                aesAlg.Key = SymmetricKey;
                aesAlg.IV = SymmetricIV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        private static string AESDecrypt(byte[] cipherText)
        {
            string plaintext = "";
            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = SymmetricKey;
                aesAlg.IV = SymmetricIV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
       

    }
}

