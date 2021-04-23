﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter the location of file to read stock data from");
            var fileName = @"c:\Users\varun13\Downloads\23-04-2020-TO-22-04-2021BAJFINANCEALLN.csv";//Console.ReadLine();
            if (File.Exists(fileName))
            {
                var gridT = new GridTester(fileName);
                gridT.Run();
            }
            else
            {
                Console.WriteLine("File not found. Terminating");
                Console.ReadLine();
            }
        }

        
    }
}
