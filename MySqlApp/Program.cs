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
                var dbname = Console.ReadLine();

                var files = Directory.GetFiles(dir);

                foreach (var file in files)
                {
                    ReplaceAllSet(file, dbname);
                }
            }
        }

        static void ReplaceAllSet(string filePath, string dbname)
        {
            var encode = Encoding.UTF8;
            var lines = File.ReadAllLines(filePath, encode);

            var removeIndex = new List<int>();

            for (var i = 0; i < lines.Length; i++)
            {
                var str = lines[i];
                if (Regex.IsMatch(str, @"^SET \@[^;]+;$"))
                {
                    lines[i] = string.Empty;
                    continue;
                }
                if (i < 3)
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