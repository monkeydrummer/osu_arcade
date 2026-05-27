// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

// See the LICENCE file in the repository root for full licence text.



using System;

using System.Collections.Generic;

using System.IO;

using System.Runtime.InteropServices;



namespace osu.Game.Rulesets.PacketRun.Charts

{

    internal static class PacketRunPathUtils

    {

        public static string NormalizeReference(string? path)

        {

            if (string.IsNullOrWhiteSpace(path))

            {

                return string.Empty;

            }



            return path.Trim().Trim('"').Trim();

        }



        public static string ToFullPath(string path)

        {

            if (string.IsNullOrWhiteSpace(path))

            {

                return path;

            }



            try

            {

                return Path.GetFullPath(path);

            }

            catch

            {

                return path;

            }

        }



        public static string ToExtendedPathIfNeeded(string path)

        {

            string fullPath = ToFullPath(path);



            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))

            {

                return fullPath;

            }



            if (fullPath.StartsWith(@"\\?\", StringComparison.Ordinal))

            {

                return fullPath;

            }



            if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))

            {

                return fullPath.Length >= 240 ? @"\\?\UNC\" + fullPath.Substring(2) : fullPath;

            }



            return fullPath.Length >= 240 ? @"\\?\" + fullPath : fullPath;

        }



        public static bool FileExists(string path)

        {

            if (string.IsNullOrWhiteSpace(path))

            {

                return false;

            }



            if (File.Exists(path))

            {

                return true;

            }



            string extended = ToExtendedPathIfNeeded(path);

            return extended != path && File.Exists(extended);

        }



        public static bool DirectoryExists(string path)

        {

            if (string.IsNullOrWhiteSpace(path))

            {

                return false;

            }



            if (Directory.Exists(path))

            {

                return true;

            }



            string extended = ToExtendedPathIfNeeded(path);

            return extended != path && Directory.Exists(extended);

        }



        public static IEnumerable<string> EnumerateFiles(string directory, string searchPattern)

        {

            if (!DirectoryExists(directory))

            {

                yield break;

            }



            foreach (string path in enumerateFilesInternal(directory, searchPattern))

            {

                yield return path;

            }



            string extendedDirectory = ToExtendedPathIfNeeded(directory);



            if (extendedDirectory == directory)

            {

                yield break;

            }



            foreach (string path in enumerateFilesInternal(extendedDirectory, searchPattern))

            {

                yield return path;

            }

        }



        public static string? FindFileByName(string directory, string fileName)

        {

            if (string.IsNullOrWhiteSpace(fileName) || !DirectoryExists(directory))

            {

                return null;

            }



            foreach (string path in EnumerateFiles(directory, "*"))

            {

                if (string.Equals(Path.GetFileName(path), fileName, StringComparison.OrdinalIgnoreCase))

                {

                    return ToFullPath(path);

                }

            }



            return null;

        }



        private static IEnumerable<string> enumerateFilesInternal(string directory, string searchPattern)
        {
            try
            {
                return Directory.EnumerateFiles(directory, searchPattern);
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

    }

}


