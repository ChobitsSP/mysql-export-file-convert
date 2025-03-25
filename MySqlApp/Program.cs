using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using MySqlApp;
using System.Xml.Linq;

namespace TruckCrm.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("请输入文件夹路径: ");
                var dir = Console.ReadLine();

                Console.WriteLine("请输入数据库名称: ");
                var dbName = Console.ReadLine();

                var files = Directory.GetFiles(dir);

                foreach (var filePath in files)
                {
                    FileProcessor.ProcessFile(
                        filePath,
                        Encoding.UTF8,
                        new SetBlockProcessor(),
                        new DatabaseNameReplacer(dbName)
                        );
                    // ReplaceAllSet(file, dbname);
                }
            }
        }

        static void ReplaceAllSet(string filePath, string dbname)
        {
            var encode = Encoding.UTF8;
            var lines = File.ReadAllLines(filePath, encode);

            int? setIndex = null;

            for (var i = 0; i < lines.Length; i++)
            {
                var str = lines[i];

                if (setIndex.HasValue)
                {
                    if (str.EndsWith(";"))
                    {
                        for (var j = setIndex.Value; j <= i; j++)
                        {
                            lines[j] = string.Empty;
                        }
                        setIndex = null;
                    }
                    continue;
                }

                if (str.StartsWith("SET @"))
                {
                    if (str.EndsWith(";"))
                    {
                        lines[i] = string.Empty;
                        continue;
                    }
                    else
                    {
                        setIndex = i;
                    }
                }

                if (i < 3 && str.StartsWith("--"))
                {
                    var reg = new Regex("Database: [A-Za-z0-9_]+$");

                    if (reg.IsMatch(str))
                    {
                        lines[i] = reg.Replace(str, m => "Database: " + dbname);
                    }
                }
            }

            File.WriteAllLines(filePath, lines, encode);
        }
    }
}