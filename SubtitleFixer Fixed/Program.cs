using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubtitleFixer_Fixed
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }
        public static void FixerMain(string FilePath)
        {
            string[] file = File.ReadAllLines(FilePath);
            file = Check(file);
            string FileName = GetOnlyFileName(FilePath);
            file = SplitAndRemoveDuplicates(file);
            File.WriteAllLines(FileName + "-Fixed.srt", file);
        }

        static string GetEndLine(string line)
        {
            int index = line.IndexOf('?');
            if (index == -1)
            {
                index = line.IndexOf('!');
                if (index == -1)
                {
                    index = line.IndexOf('.');
                }
            }
            return index == -1 ? line : line.Substring(index + 1);
        }

        static string[] Check(string[] file)
        {
            List<string> tempList = new List<string>();
            string temp = "";

            for (int i = 0; i < file.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(file[i]))
                {
                    if (char.IsDigit(file[i][0]))
                    {
                        if (!string.IsNullOrWhiteSpace(temp))
                        {
                            tempList.Add(temp);
                            temp = "";
                        }
                        if (!tempList.Contains(file[i]))
                        {
                            tempList.Add(file[i]);
                        }
                    }
                    else if (char.IsLetter(file[i][0]))
                    {
                        if (!string.IsNullOrWhiteSpace(temp))
                        {
                            temp += "\n" + file[i];
                        }
                        else
                        {
                            temp = file[i];
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(temp))
                        {
                            tempList.Add(temp);
                            temp = "";
                        }
                        tempList.Add(file[i]);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        tempList.Add(temp);
                        temp = "";
                    }
                    tempList.Add("");
                }
            }

            if (!string.IsNullOrWhiteSpace(temp))
            {
                tempList.Add(temp);
            }

            return tempList.ToArray();
        }
        static string GetOnlyFileName(string fileName)
        {
            return fileName.Substring(0, fileName.IndexOf('.'));
        }

        static string[] SplitAndRemoveDuplicates(string[] file)
        {
            List<string> result = new List<string>();
            HashSet<string> seenSubtitles = new HashSet<string>();
            int currentIndex = 0;

            for (int i = 0; i < file.Length; i++)
            {
                if (int.TryParse(file[i], out int subtitleNumber))
                {
                    currentIndex = subtitleNumber;
                    if (i + 2 < file.Length)
                    {
                        string timestamp = file[i + 1];
                        string text = file[i + 2];
                        string[] sentences = SplitIntoSentences(text);

                        for (int j = 0; j < sentences.Length; j++)
                        {
                            if (!string.IsNullOrWhiteSpace(sentences[j]))
                            {
                                string newSubtitleKey = $"{timestamp},{sentences[j].Trim()}";
                                if (!seenSubtitles.Contains(newSubtitleKey))
                                {
                                    result.Add(currentIndex.ToString());
                                    result.Add(timestamp);
                                    result.Add(sentences[j].Trim());
                                    result.Add(""); // Add single empty line after each subtitle
                                    seenSubtitles.Add(newSubtitleKey);
                                    currentIndex++;
                                }
                            }
                        }
                        i += 2; // Skip the timestamp and text lines
                    }
                }
            }

            // Remove the last empty line if it exists
            if (result.Count > 0 && string.IsNullOrWhiteSpace(result[result.Count - 1]))
            {
                result.RemoveAt(result.Count - 1);
            }

            return result.ToArray();
        }

        static string[] SplitIntoSentences(string text)
        {
            return text.Split(new[] { ". ", "? ", "! " }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim() + (s.EndsWith(".") || s.EndsWith("?") || s.EndsWith("!") ? "" : "."))
                      .ToArray();
        }
    }
}
