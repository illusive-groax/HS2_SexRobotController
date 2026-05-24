using System;
using System.IO;

namespace HS2_SexRobotController
{
    internal sealed class FileHandler
    {        

        private static bool fileExists()
        {
            return File.Exists(StringConstants.ANIMATION_FILE_PATH);
        }

        private static void createAnimationPositionsFile()
        {
            var dictionary = BoneAnimationDefiner.animationFemaleTargetDictionary;
            using (StreamWriter writer = new StreamWriter(StringConstants.ANIMATION_FILE_PATH))
                foreach (var entry in dictionary)
                    writer.WriteLine("{0}, {1}", entry.Key, entry.Value);
        }

        internal static void readPositionsFromFile()
        {
            try
            {
                string[] parts;
                string line, key;
                BoneAnimationDefiner.FemaleTargetType value;                

                if (!fileExists())
                    createAnimationPositionsFile();

                StreamReader reader = new StreamReader(StringConstants.ANIMATION_FILE_PATH);
                while ((line = reader.ReadLine()) != null)
                {
                    // split the line into key and value
                    parts  = line.Split(',');
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
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
