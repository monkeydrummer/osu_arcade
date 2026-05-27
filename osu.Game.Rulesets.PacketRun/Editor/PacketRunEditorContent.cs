// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

// See the LICENCE file in the repository root for full licence text.



using System;

using System.Collections.Generic;

using System.IO;

using osu.Game.Rulesets.PacketRun.Charts;



namespace osu.Game.Rulesets.PacketRun.Editor

{

    public static class PacketRunEditorContent

    {

        public static string GetContentRoot()

        {

            string? songsRoot = findContentSongsRoot();

            return songsRoot ?? PacketRunPathUtils.ToFullPath(Path.Combine(AppContext.BaseDirectory, "PacketRun.Content", "songs"));

        }



        public static string GetContentDirectory()

        {

            string songsRoot = GetContentRoot();

            string? parent = Path.GetDirectoryName(songsRoot);



            if (!string.IsNullOrEmpty(parent) && PacketRunPathUtils.DirectoryExists(parent))

            {

                return parent;

            }



            return PacketRunPathUtils.ToFullPath(Path.Combine(AppContext.BaseDirectory, "PacketRun.Content"));

        }



        private static string? findContentSongsRoot()

        {

            foreach (string root in getSearchRoots())

            {

                string candidate = Path.Combine(root, "PacketRun.Content", "songs");



                if (PacketRunPathUtils.DirectoryExists(candidate))

                {

                    return PacketRunPathUtils.ToFullPath(candidate);

                }

            }



            return null;

        }



        private static IEnumerable<string> getSearchRoots()

        {

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);



            foreach (string? path in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })

            {

                if (string.IsNullOrWhiteSpace(path))

                {

                    continue;

                }



                string fullPath = PacketRunPathUtils.ToFullPath(path);



                if (seen.Add(fullPath))

                {

                    yield return fullPath;

                }

            }



            string? dir = AppContext.BaseDirectory;



            for (int i = 0; i < 10 && dir != null; i++)

            {

                string fullPath = PacketRunPathUtils.ToFullPath(dir);



                if (seen.Add(fullPath))

                {

                    yield return fullPath;

                }



                dir = Path.GetDirectoryName(dir);

            }

        }

    }

}


