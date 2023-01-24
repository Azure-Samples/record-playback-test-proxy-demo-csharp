// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

namespace ParseDotEnvFile
{
    using System;
    using System.IO;

    public static class ParseDotEnvFile
    {
        public static void Load(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var splits = line.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

                if (splits.Length != 2)
                    continue;

                Environment.SetEnvironmentVariable(splits[0], splits[1]);
            }
        }
    }
}
