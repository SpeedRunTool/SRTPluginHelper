using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SRTPluginHelper
{
	public static class Versions
	{
		/// <summary>
		/// Generates a version log file containing a C# byte array declaration representing the version information of a game.
		/// </summary>
		/// <param name="logger">The logger instance for logging informational messages.</param>
		/// <param name="authors">A string containing all authors of the plugin.</param>
		/// <param name="cs">The byte array containing the version information.</param>
		/// <param name="gameName">The name of the game.</param>
		private static void OutputVersionString<PluginProducer>(ILogger<PluginProducer> logger, string authors, byte[] cs, string gameName)
		{
			// Create a StringBuilder instance to build the version string
			StringBuilder sb = new StringBuilder($"private static readonly byte[] {gameName.ToLowerInvariant()}??_00000000 = new byte[{cs.Length}] {{ ");

			// Iterate over the byte array 'cs' and append each byte to the StringBuilder
			for (int i = 0; i < cs.Length; i++)
				sb.AppendFormat("0x{0:X2}, ", cs[i]);

			// Remove the extra trailing comma and space
			sb.Length -= 2;

			// Append the closing bracket and semicolon to complete the version string
			sb.Append(" };");

			// Create a log file name based on the lowercased 'gameName'
			string filename = $"{gameName.ToLowerInvariant()}_version.log";

			// Log an informational message indicating the author of the plugin and the log file name
			logger.LogInformation($"Please message {authors} with the {filename} file.");

			// Open the log file for writing and write the version string to it
			using (StreamWriter writer = new StreamWriter(filename))
				writer.WriteLine(sb.ToString());
		}

		/// <summary>
		/// Detects the version of a game based on its file checksum and a dictionary of supported versions.
		/// </summary>
		/// <typeparam name="PluginProducer">The type representing the game plugin producer.</typeparam>
		/// <typeparam name="VersionType">The type representing the game version.</typeparam>
		/// <param name="logger">The logger instance for logging informational, error, and warning messages.</param>
		/// <param name="filePath">The path to the game file.</param>
		/// <param name="authors">A string containing all authors of the plugin.</param>
		/// <param name="_supportedVersions">A dictionary containing byte array checksums and their corresponding versions.</param>
		/// <returns>The detected version of the game, or null if the version is unknown or not supported.</returns>
		public static VersionType? DetectVersion<PluginProducer, VersionType>(ILogger<PluginProducer> logger, string? filePath, string authors, Dictionary<byte[], VersionType> _supportedVersions)
		{
			// Check if the file path is null, empty, or contains only whitespace
			if (string.IsNullOrWhiteSpace(filePath))
			{
				logger.LogError($"Unknown Version: string.IsNullOrWhiteSpace({nameof(filePath)}) returned true.");
				return default;
			}

			byte[] checksum;
			// Calculate the SHA256 checksum of the game file
			using (SHA256 hashFunc = SHA256.Create())
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
				checksum = hashFunc.ComputeHash(fs);

			// Iterate through the supported versions dictionary and compare the checksums
			foreach (var version in _supportedVersions)
			{
				if (checksum.SequenceEqual(version.Key))
					return _supportedVersions[version.Key];
			}

			// The version is unknown or not supported
			logger.LogWarning("Unknown Version");
			// Output the version checksum in a log file for further analysis
			OutputVersionString(logger, authors, checksum, "re4r");
			return default;
		}
	}
}
