using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

namespace ChatClient
{
    // Handles AES and RSA encryption, decryption and key generation
    // An EncryptionHandler object is specific to the chat username passed to the constructor
    public class EncryptionHandler
    {
        // Indicates whether key containers (and associated RSA keys) persist
        private const bool PersistRSAKeys = true;

        // Dictionary of the symmetric keys for each username for this session
        private readonly Dictionary<string, byte[]> aesKeyDict = new Dictionary<string, byte[]>();

        // Dictionary of the (pub key) encrypted symmetric keys for each username for this session
        private readonly Dictionary<string, byte[]> encAesKeyDict = new Dictionary<string, byte[]>();

        // XAML representation of this user's public key
        private string rsaPublicKey;

        // Stores important parameters related to the RSA key pair from which the private key can be deduced
        private RSAParameters rsaParameters;

        // The chat username that this object is specific to
        private string username;

        // Initialise the object by setting the RSA key fields
        // Takes the chat username to get/generate keys for
        public EncryptionHandler(string username)
        {
            this.username = username;

            // Retrieve the RSA public and private keys for this username. The keys are generated if they do not exist
            rsaPublicKey = GetRSAPublicKey();
            rsaParameters = GetRSAParameters();

            Console.WriteLine("[INFO] RSA public key is: " + rsaPublicKey);
            Console.WriteLine("[INFO] RSA private key is: " + rsaParameters);
        }

        // Retrieve the RSA public key (as an XML string) for the given chat username from the local PC key container
        // A container and public/private key pair are created if they do not exist
        public string GetRSAPublicKey()
        {
            // Create an object to store the parameters for the cryptographic service provider (CSP)
            CspParameters cspParams = new CspParameters
            {
                // Set the key container name
                KeyContainerName = username
            };

            // Create an (RSA) CSP that accesses the key container we've just set up
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider(cspParams);

            // Set persistence of the container according to the class default
            rsaCSP.PersistKeyInCsp = PersistRSAKeys;

            // Return the public key as an XML string
            return rsaCSP.ToXmlString(false);
        }

        // Retrieve the RSAParameters object for the given chat username from the local PC key container
        // A container and public/private key pair are created if they do not exist
        private RSAParameters GetRSAParameters()
        {
            // Create an object to store the parameters for the cryptographic service provider (CSP)
            CspParameters cspParams = new CspParameters
            {
                // Set the key container name
                KeyContainerName = username
            };

            // Create an (RSA) CSP that accesses the key container we've just set up
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider(cspParams);

            // Set persistence of the container across reboots according to the class default
            rsaCSP.PersistKeyInCsp = PersistRSAKeys;

            // Return the RSAParameters object for the CSP
            return rsaCSP.ExportParameters(true);
        }

        // Takes a plaintext string and encrypts it using AES
        // Also encrypts the AES key and IV using the provided RSA public key
        // Returns a string in the following format:
        // "<A>cipher_text<B>encrypted_aes_key<C>encrypted_aes_iv<D>"
        // If we've got an AES key for the recipient user already, we'll reuse it (but use a fresh IV)
        public string EncryptString(string messageText, string recipientUserName, string recipientPubKey)
        {
            // Set up an RSA CSP with the recipient's public key so we can encrypt strings with the key
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            rsaCSP.FromXmlString(recipientPubKey);

            // Set up an Aes object we can use for key and IV generation
            Aes aes = Aes.Create();

            // If we haven't already got an AES key for the recipient
            if(!encAesKeyDict.ContainsKey(recipientUserName))
            {
                // Generate an AES key
                aes.GenerateKey();

                // Add the AES key to the key dictionary
                aesKeyDict.Add(recipientUserName, aes.Key);

                // Encrypt the AES key using RSA and the recipient's public key and add it to the encrypted key dictionary
                encAesKeyDict.Add(recipientUserName, rsaCSP.Encrypt(aes.Key, false));
            }

            // Generate an AES IV
            aes.GenerateIV();

            // Encrypt the plaintext string using AES with the key for this user and the IV we've just generated
            byte[] encryptedMessage = AESEncrypt(aesKeyDict[recipientUserName], aes.IV, messageText);

            // Encrypt the IV using RSA and the recipient's public key
            byte[] encryptedIV = rsaCSP.Encrypt(aes.IV, false);

            Console.WriteLine(PrintBytes(encAesKeyDict[recipientUserName]));

            // Combine the encrypted message, encrypted AES key and encrypted AES IV into a string
            return "<A>" + Convert.ToBase64String(encryptedMessage) + "<B>" + Convert.ToBase64String(encAesKeyDict[recipientUserName]) + "<C>" + Convert.ToBase64String(encryptedIV) + "<D>";
        }

        // Takes an AES key, IV and plaintext string
        // Encrypts the string with the key and IV and returns the result as a byte array
        private byte[] AESEncrypt(byte[] aesKey, byte[] aesIV, string plainText)
        {
            // The encrypted text to return
            byte[] cipherText;

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = aesKey;
                aesAlg.IV = aesIV;

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream
                            swEncrypt.Write(plainText);
                        }
                        cipherText = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted text
            return cipherText;
        }

        // Performs the reverse of EncryptString
        // Takes a string in the following format where the key and IV were encrypted using this user's public key
        // "<A>cipher_text<B>encrypted_aes_key<C>encrypted_aes_iv<D>"
        // RSA decrypts the AES key and IV and uses these to AES decrypt the cipher text
        // Returns the decrypted cipher text
        public string DecryptString(string encryptedString)
        {
            // Set up an RSA CSP with the our private key so we can decrypt strings with the key
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            rsaCSP.ImportParameters(rsaParameters);

            // Parse the encrypted AES key out of the encrypted string
            string encryptedAESKeyString = encryptedString.Split(new string[] {"<B>"}, StringSplitOptions.None)[1].Split(new string[] {"<C>"}, StringSplitOptions.None)[0];

            // Translate the AES key back to bytes
            byte[] encryptedAESKey = Convert.FromBase64String(encryptedAESKeyString);

            // Parse the encrypted AES IV out of the encrypted string
            string encryptedAESIVString = encryptedString.Split(new string[] {"<C>"}, StringSplitOptions.None)[1].Split(new string[] {"<D>"}, StringSplitOptions.None)[0];

            // Translate the AES IV back to bytes
            byte[] encryptedAESIV = Convert.FromBase64String(encryptedAESIVString);

            Console.WriteLine(PrintBytes(encryptedAESKey));

            // RSA decrypt the AES key and IV
            byte[] aesKey = rsaCSP.Decrypt(encryptedAESKey, false);
            byte[] aesIV = rsaCSP.Decrypt(encryptedAESIV, false);

            // Parse the cipher text out of the encrypted string
            string cipherTextString = encryptedString.Split(new string[] {"<A>"}, StringSplitOptions.None)[1].Split(new string[] {"<B>"}, StringSplitOptions.None)[0];

            // Translate the ciper text back into bytes
            byte[] cipherText = Convert.FromBase64String(cipherTextString);

            // AES decrypt the cipher text using the key and IV and return the result
            return AESDecrypt(aesKey, aesIV, cipherText);
        }

        // Takes an AES key, IV and ciphertext string
        // Decrypts the string with the key and IV and returns the result as a string
        private string AESDecrypt(byte[] aesKey, byte[] aesIV, byte[] cipherText)
        {
            // The plaintext to return
            string plaintext = "";

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = aesKey;
                aesAlg.IV = aesIV;

                // Create a decryptor to perform the stream transform
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            // Return the decrypted string
            return plaintext;
        }


        public string PrintBytes(byte[] byteArray)
        {
            var sb = new StringBuilder("new byte[] { ");
            for (var i = 0; i < byteArray.Length; i++)
            {
                var b = byteArray[i];
                sb.Append(b);
                if (i < byteArray.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" }");
            return sb.ToString();
        }
    }

}

