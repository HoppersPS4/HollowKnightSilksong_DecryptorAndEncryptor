using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "Hollow Knight Text Asset Decryptor/Encryptor - made by hoppers";
        if (args.Length < 3)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  HollowKnight_TextAssetDecryptor.exe -d <inputDir> -o <outputDir>");
            Console.WriteLine("  HollowKnight_TextAssetDecryptor.exe -e <inputDir> -o <outputDir>");
            return;
        }

        bool decryptMode = false;
        bool encryptMode = false;
        string inputDir = "";
        string outputDir = "";

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-d":
                    decryptMode = true;
                    inputDir = args[++i];
                    break;
                case "-e":
                    encryptMode = true;
                    inputDir = args[++i];
                    break;
                case "-o":
                case "--output":
                    outputDir = args[++i];
                    break;
                case "-dir":
                case "--directory":
                    inputDir = args[++i];
                    break;
            }
        }

        if (!Directory.Exists(inputDir))
        {
            Console.WriteLine($"Input directory does not exist: {inputDir}");
            return;
        }

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        string[] files = Directory.GetFiles(inputDir, "*.txt", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            try
            {
                string content = File.ReadAllText(file, Encoding.UTF8);
                string result;

                if (decryptMode)
                {
                    byte[] encryptedBytes = Convert.FromBase64String(content);

                    using (RijndaelManaged rijndael = new RijndaelManaged())
                    {
                        rijndael.Key = Encoding.UTF8.GetBytes("UKu52ePUBwetZ9wNX88o54dnfKRu0T1l"); // Hardcoded key, ah yes, good idea idiot. Add a way to dump from Binary...
                        rijndael.Mode = CipherMode.ECB;
                        rijndael.Padding = PaddingMode.PKCS7;

                        using (ICryptoTransform decryptor = rijndael.CreateDecryptor())
                        {
                            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                            result = Encoding.UTF8.GetString(decryptedBytes);
                        }
                    }
                }
                else if (encryptMode)
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(content);

                    using (RijndaelManaged rijndael = new RijndaelManaged())
                    {
                        rijndael.Key = Encoding.UTF8.GetBytes("UKu52ePUBwetZ9wNX88o54dnfKRu0T1l");
                        rijndael.Mode = CipherMode.ECB;
                        rijndael.Padding = PaddingMode.PKCS7;

                        using (ICryptoTransform encryptor = rijndael.CreateEncryptor())
                        {
                            byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                            result = Convert.ToBase64String(encryptedBytes);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("You must specify either -d (decrypt) or -e (encrypt).");
                    return;
                }

                string outPath = Path.Combine(outputDir, Path.GetFileName(file));
                File.WriteAllText(outPath, result, Encoding.UTF8);

                Console.WriteLine($"{(decryptMode ? "Decrypted" : "Encrypted")}: {Path.GetFileName(file)} -> {outPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process {file}: {ex.Message}");
            }
        }

        Console.WriteLine("Done.");
    }
}
