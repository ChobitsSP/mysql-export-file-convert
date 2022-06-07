using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
            var text = File.ReadAllText(filePath, encode);
            var reg = new Regex(@"[\r\n]SET \@[^;]+;");
            text = reg.Replace(text, m => string.Empty);

            var reg2 = new Regex(@"Database: ([^\r\n]+)");

            text = reg2.Replace(text, m =>
            {
                if (m.Index == 0) return "Database: " + dbname;
                return m.Value;
            });

            File.WriteAllText(filePath, text, encode);
        }
    }
}