using System.IO;
using UnityEngine;

namespace Utils
{
    //! TODO @16 Apr 2024 Fix an issue with file path being default will result in invalid file
    public static class FileUtils
    {
        /// <summary>
        /// Wrapper function to read text from given file name and path.
        /// </summary>
        /// <param name="fileName">File to read. Includes extension</param>
        /// <param name="filePath">File path that file belongs in</param>
        /// <returns>The text in file as string</returns>
        public static string ReadTextFile(string fileName, string filePath = "")
        {
            return File.ReadAllText(filePath + fileName);
        }

        public static void WriteFile(string content, string fileName, string filePath = "")
        {
            File.WriteAllText(filePath + fileName, content);
        }

        /// <summary>
        /// Writes given string content to file.
        /// https://learn.microsoft.com/en-us/dotnet/api/system.io.streamwriter.writeasync?view=net-7.0
        /// </summary>
        /// <param name="content">Content to write to file</param>
        /// <param name="fileName">Name of file to write to. Includes extension</param>
        /// <param name="filePath">Path of file</param>
        public static async void WriteFileAsync(string content, string fileName, string filePath = "")
        {
            Debug.Log("Writing to file async...");

            /*using(StreamWriter outputFile = new StreamWriter(filePath))
            {
                try
                {
                    await outputFile.WriteAsync(content);
                }
                catch 
                { 
                    Debug.LogError("Write to file failed! ");
                }
            }*/

            await File.WriteAllTextAsync(filePath + fileName, content);

            Debug.Log("Async file write has completed.");
        }
    }
}