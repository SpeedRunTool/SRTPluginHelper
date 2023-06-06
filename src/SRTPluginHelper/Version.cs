using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using SRTPluginBase;

namespace SRTPluginHelper
{
    /// <summary>
    /// A static helper class with various methods for detecting and reporting program versions.
    /// </summary>
	public static class Version
	{
        /// <summary>
        /// Detects the version based on its file checksum and a dictionary of supported versions.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin producer type.</typeparam>
        /// <typeparam name="THashAlgorithm">The hash algorithm to use for checksum calculation.</typeparam>
        /// <typeparam name="TVersionType">The program version enumeration type.</typeparam>
        /// <param name="pluginInstance">The instance of the plugin.</param>
        /// <param name="filePath">The path to the file to check.</param>
        /// <param name="supportedVersions">A dictionary containing byte array checksums and their corresponding versions.</param>
        /// <returns>The detected version of the program, or default if the version is unknown or not supported.</returns>
        public static TVersionType? DetectVersion<TPlugin, THashAlgorithm, TVersionType>(TPlugin pluginInstance, string? filePath, Dictionary<byte[], TVersionType> supportedVersions)
			where TPlugin : IPluginProducer
			where THashAlgorithm : HashAlgorithm
        {
			// Check if the file path is null, empty, or contains only whitespace
			if (string.IsNullOrWhiteSpace(filePath))
			{
				var ex = new ArgumentException($"{nameof(filePath)} is null or an empty string", nameof(filePath));
                pluginInstance.Logger.LogError(ex, $"{ex}");
				return default;
			}

            // Get the filename without the extension.
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

            // Calculate the checksum hash for the given filePath and algorithm.
            byte[] checksum;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                checksum = GetChecksum<THashAlgorithm>(fs);

            // Iterate through the supported versions dictionary and compare the checksums.
            foreach (var version in supportedVersions)
            {
                if (checksum.SequenceEqual(version.Key))
                {
                    pluginInstance.Logger.LogInformation($"{fileNameWithoutExt} version detected: {supportedVersions[version.Key]}");
                    return supportedVersions[version.Key];
                }
            }

            // Write out the version hash information.
            string versionHashLogFilename = $"{fileNameWithoutExt}_VersionHash.log";
            string versionHash = GetByteArrayDeclarationString(checksum);
            using (StreamWriter sw = new StreamWriter(versionHashLogFilename, false, Encoding.UTF8))
                sw.WriteLine($"[{DateTime.GetCurrentUtcString()}] {fileNameWithoutExt}'s {typeof(THashAlgorithm).Name} checksum hash: {versionHash}");

            // The version is unknown or not supported
            pluginInstance.Logger.LogWarning($"Unknown version! Please submit the {versionHashLogFilename} file to the plugin authors {pluginInstance.Info.Author}.");

            return default;
		}

        /// <summary>
        /// Calculates the checksum of a file.
        /// </summary>
        /// <param name="stream">A stream to the content to calculate a checksum for.</param>
        /// <returns>The computed checksum as a byte array.</returns>
        public static byte[] GetChecksum<T>(Stream stream) where T : HashAlgorithm
        {
            using (HashAlgorithm hashFunc = HashAlgorithm.Create(typeof(T).Name)) // Create an instance of the hashing algorithm
                return hashFunc.ComputeHash(stream); // Compute the hash value of the file using the algorithm
        }

        /// <summary>
        /// Generates a string containing a C#-stylized byte array declaration representing the version information of the program.
        /// </summary>
        /// <param name="checksum">The byte array containing the version information.</param>
        /// <returns>A string containing a C#-stylized byte array declaration representing the version information of the program.</returns>
        public static string GetByteArrayDeclarationString(byte[] checksum)
        {
            StringBuilder sb = new($"new byte[{checksum.Length}] {{ ");
            for (int i = 0; i < checksum.Length; ++i) // Iterate over the byte array checksum and append each byte to the StringBuilder
                sb.AppendFormat("0x{0:X2}, ", checksum[i]);
            sb.Length -= 2; // Remove the extra trailing comma and space
            sb.Append(" };"); // Append the closing bracket and semicolon to complete the string
            return sb.ToString();
        }
    }
}
