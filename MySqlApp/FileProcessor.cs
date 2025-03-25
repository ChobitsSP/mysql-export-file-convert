using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MySqlApp
{
    public static class FileProcessor
    {
        public static void ProcessFile(string filePath, Encoding encoding, params ILineProcessor[] processors)
        {
            var lines = File.ReadAllLines(filePath, encoding);
            var processedLines = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string currentLine = lines[i];
                bool skipLine = false;

                foreach (var processor in processors)
                {
                    var result = processor.ProcessLine(currentLine, i, lines);
                    if (result.ShouldSkip)
                    {
                        skipLine = true;
                        i = result.NextLineIndex - 1; // -1 because the loop will increment i
                        break;
                    }
                    currentLine = result.ProcessedLine;
                }

                if (!skipLine)
                {
                    processedLines.Add(currentLine);
                }
            }

            File.WriteAllLines(filePath, processedLines, encoding);
        }
    }

    public interface ILineProcessor
    {
        ProcessResult ProcessLine(string line, int lineIndex, string[] allLines);
    }

    public class ProcessResult
    {
        public bool ShouldSkip { get; set; }
        public string ProcessedLine { get; set; }
        public int NextLineIndex { get; set; }

        public static ProcessResult Skip(int nextLineIndex) => new ProcessResult { ShouldSkip = true, NextLineIndex = nextLineIndex };
        public static ProcessResult Continue(string processedLine) => new ProcessResult { ShouldSkip = false, ProcessedLine = processedLine };
    }

    public class SetBlockProcessor : ILineProcessor
    {
        public ProcessResult ProcessLine(string line, int lineIndex, string[] allLines)
        {
            if (line.TrimStart().StartsWith("SET @"))
            {
                int endIndex = lineIndex;
                while (endIndex < allLines.Length && !allLines[endIndex].TrimEnd().EndsWith(";"))
                {
                    endIndex++;
                }
                return ProcessResult.Skip(endIndex + 1);
            }
            return ProcessResult.Continue(line);
        }
    }

    public class DatabaseNameReplacer : ILineProcessor
    {
        private readonly string _dbName;
        private readonly Regex _regex = new Regex(@"Database: [A-Za-z0-9_]+$");

        public DatabaseNameReplacer(string dbName)
        {
            _dbName = dbName;
        }

        public ProcessResult ProcessLine(string line, int lineIndex, string[] allLines)
        {
            if (lineIndex < 3 && line.StartsWith("--") && _regex.IsMatch(line))
            {
                return ProcessResult.Continue(_regex.Replace(line, $"Database: {_dbName}"));
            }
            return ProcessResult.Continue(line);
        }
    }
}
