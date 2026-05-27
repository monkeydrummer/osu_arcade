// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

// See the LICENCE file in the repository root for full licence text.



using System.Collections.Generic;

using System.IO;



namespace osu.Game.Rulesets.PacketRun.Charts

{

    public enum PacketRunAudioResolveResult

    {

        NotFound,

        FoundOnDisk,

    }



    public static class PacketRunAudioPathResolver

    {

        private static readonly string[] audio_extensions = { ".ogg", ".mp3", ".wav", ".flac" };



        public static PacketRunAudioResolveResult TryResolve(string? audioFileReference, string songDirectory, string? chartFilePath, out string resolvedPath)

        {

            audioFileReference = PacketRunPathUtils.NormalizeReference(audioFileReference);

            songDirectory = PacketRunPathUtils.ToFullPath(songDirectory);



            foreach (string candidate in enumerateCandidates(audioFileReference, songDirectory, chartFilePath))

            {

                if (PacketRunPathUtils.FileExists(candidate))

                {

                    resolvedPath = PacketRunPathUtils.ToFullPath(candidate);

                    return PacketRunAudioResolveResult.FoundOnDisk;

                }

            }



            resolvedPath = getFallbackPath(audioFileReference, songDirectory);

            return PacketRunAudioResolveResult.NotFound;

        }



        public static string DescribeLookup(string? audioFileReference, string songDirectory, string? chartFilePath)

        {

            audioFileReference = PacketRunPathUtils.NormalizeReference(audioFileReference);

            songDirectory = PacketRunPathUtils.ToFullPath(songDirectory);



            if (!string.IsNullOrWhiteSpace(audioFileReference) && Path.IsPathRooted(audioFileReference))

            {

                return PacketRunPathUtils.ToFullPath(audioFileReference);

            }



            if (!string.IsNullOrWhiteSpace(audioFileReference))

            {

                return Path.Combine(songDirectory, audioFileReference);

            }



            return songDirectory;

        }



        private static IEnumerable<string> enumerateCandidates(string audioFileReference, string songDirectory, string? chartFilePath)

        {

            if (!string.IsNullOrWhiteSpace(audioFileReference))

            {

                yield return audioFileReference;



                if (!Path.IsPathRooted(audioFileReference))

                {

                    if (!string.IsNullOrEmpty(songDirectory))

                    {

                        yield return Path.Combine(songDirectory, audioFileReference);



                        string? caseMatch = PacketRunPathUtils.FindFileByName(songDirectory, audioFileReference);



                        if (caseMatch != null)

                        {

                            yield return caseMatch;

                        }

                    }



                    if (!string.IsNullOrEmpty(chartFilePath))

                    {

                        string? chartDirectory = Path.GetDirectoryName(chartFilePath);



                        if (!string.IsNullOrEmpty(chartDirectory))

                        {

                            yield return Path.Combine(chartDirectory, audioFileReference);



                            string? caseMatch = PacketRunPathUtils.FindFileByName(chartDirectory, audioFileReference);



                            if (caseMatch != null)

                            {

                                yield return caseMatch;

                            }

                        }

                    }

                }

            }



            if (!string.IsNullOrEmpty(songDirectory) && PacketRunPathUtils.DirectoryExists(songDirectory))

            {

                foreach (string extension in audio_extensions)

                {

                    foreach (string match in PacketRunPathUtils.EnumerateFiles(songDirectory, $"*{extension}"))

                    {

                        yield return match;

                    }

                }

            }

        }



        private static string getFallbackPath(string audioFileReference, string songDirectory)

        {

            if (!string.IsNullOrWhiteSpace(audioFileReference) && Path.IsPathRooted(audioFileReference))

            {

                return PacketRunPathUtils.ToFullPath(audioFileReference);

            }



            if (!string.IsNullOrWhiteSpace(audioFileReference) && !string.IsNullOrEmpty(songDirectory))

            {

                return Path.Combine(songDirectory, audioFileReference);

            }



            return audioFileReference ?? songDirectory;

        }

    }

}


