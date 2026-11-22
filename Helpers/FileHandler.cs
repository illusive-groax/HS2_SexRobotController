using System;
using System.IO;

namespace HS2_SexRobotController.Helpers
{
    internal sealed class FileHandler
    {

        private static bool FileExists()
        {
            return File.Exists(StringConstants.ANIMATION_FILE_PATH);
        }

        private static void CreateAnimationPositionsFile()
        {
            var dictionary = BoneAnimationDefiner.animationFemaleTargetDictionary;
            using StreamWriter writer = new(StringConstants.ANIMATION_FILE_PATH);
            foreach (var entry in dictionary)
                writer.WriteLine("{0}, {1}", entry.Key, entry.Value);
        }

        private static void WriteToFile(string fileName, string data)
        {
            if (data == null || data == "")
                return;
            using StreamWriter writer = new(fileName, append: true);
            writer.WriteLine(data);
        }

        internal static void WriteToDebugLog(string debugEntry)
        {
            WriteToFile(StringConstants.DEBUGGING_LOG_FILE_PATH, debugEntry);
        }

        internal static void WriteUnknownAnimationsToFile(string animationName)
        {
            WriteToFile(StringConstants.UNKNOWN_ANIMATIONS_FILE_PATH, animationName);
        }

        internal static void ReadAnimationsFromFile()
        {
            string[] parts;
            string line, key;
            BoneAnimationDefiner.FemaleTargetType value;

            if (!FileExists())
                CreateAnimationPositionsFile();

            StreamReader reader = new(StringConstants.ANIMATION_FILE_PATH);
            while ((line = reader.ReadLine()) != null)
            {
                // split the line into key and value
                parts = line.Split(',');
                if (parts.Length == 2)
                {
                    key = parts[0].Trim();
                    value = (BoneAnimationDefiner.FemaleTargetType)Enum.Parse(typeof(BoneAnimationDefiner.FemaleTargetType), parts[1].Trim());
                    // verify it doesn't already exists in the dictionary
                    if (!BoneAnimationDefiner.animationFemaleTargetDictionary.ContainsKey(key))
                    {
                        BoneAnimationDefiner.animationFemaleTargetDictionary.Add(key, value);
                    }
                }
            }
        }

    }
}
