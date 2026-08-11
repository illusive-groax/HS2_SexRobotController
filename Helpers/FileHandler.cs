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

        internal static void WriteToFile(string animationName)
        {
            if (animationName == null || animationName == "")
                return;
            using StreamWriter writer = new(StringConstants.UNKNOWN_ANIMATIONS_FILE_PATH, append: true);
            writer.WriteLine(animationName);
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
