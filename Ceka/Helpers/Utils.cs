/*
Ceka - Data Mining Library in C Sharp
Copyright (C) 2015 Christian Fröhlingsdorf, ceka.5cf.de

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DM.Ceka.Helpers
{
    /// <summary>
    /// static helper class, contains functions and values that are globally required
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// turns use of Console.WriteLine() on and off -> works also for algorithm workflow debugging
        /// </summary>
        public static bool DEBUG = true;

        /// <summary>
        /// write debug messages to logfile
        /// </summary>
        public static bool LOG = true;

        /// <summary>
        /// logfile (activate Utils.LOG); default: "./ceka.log" (changes Common.LogFile.FILE)
        /// </summary>
        public static string LOGFILE
        {
            get
            {
                return Common.LogFile.FILE;
            }
            set
            {
                Common.LogFile.FILE = value;
            }
        }

        /// <summary>
        /// returns the current directory of the application that uses the assembly
        /// </summary>
        public static string CurrentDirectory
        {
            get
            {
                return Environment.CurrentDirectory;
            }
        }

        /// <summary>
        /// returns the memory usage of a managed object (in bytes), not 100% accurate but it will do..
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static long GetObjectMemoryUsage(object o)
        {
            long size = 0;
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, o);
                size = s.Length;
            }
            return size;
        }

        /// <summary>
        /// displays an import help
        /// </summary>
        public static void PrintArffImportHelp()
        {
            bool debugger = false;
            if (!DEBUG) //make sure import help is always shown on import fails
            {
                debugger = true;
                DEBUG = true;
            }

            Helpers.Utils.Debug("###############################################");
            Helpers.Utils.Debug("###############################################");
            Helpers.Utils.Debug("Arff File Import Help:");
            Helpers.Utils.Debug("You should check the ARFF File you are trying to import, if you have any trouble.");
            Helpers.Utils.Debug("A structure like this: @attribute example {'1', '2', '3'} is not going to work.");
            Helpers.Utils.Debug("You should convert it to this: @attribute 'example' {'1', '2', '3'}.");
            Helpers.Utils.Debug("Or stay like this: @attribute 'example' {1, 2, 3}");
            Helpers.Utils.Debug("###############################################");
            Helpers.Utils.Debug("Arff File Import Hints:");
            Helpers.Utils.Debug("Whitespace will be automatically removed, as well as ' ' embracements.");
            Helpers.Utils.Debug("'?' values will be marked and can be removed with 'Library.removeEmptyValueDatasets()'");
            Helpers.Utils.Debug("Do not try to use numeric attributes, give them a span or range in your dataset.");
            Helpers.Utils.Debug("###############################################");
            Helpers.Utils.Debug("###############################################");

            if (debugger)
                DEBUG = false;
        }

        /// <summary>
        /// returns an array of supported algorithms (for any kind of lib related function selection)
        /// </summary>
        public static string[] SupportedAlgorithms = new string[] { 
            "apriori",
            "fast-apriori", //multithreaded
            "svm",
            "cobweb"
        };

        /// <summary>
        /// returns the exploded single string version of SupportedAlgorithms
        /// </summary>
        public static string SupportedAlgorithmsExploded
        {
            get
            {
                return ArrayExplode(SupportedAlgorithms);
            }
        }

        /// <summary>
        /// returns an array of supported arff altering functionalities (for any kind of lib related function selection)
        /// </summary>
        public static string[] SupportedArffFunctions = new string[]{
            "removePerAttributeValue",
            "rebuildAttributeAsRanged",
            "removePatternMatchRows",
            "removeUnusedValues",
            "refineAllRangedAttributes",
            "getMemorySize"
        };

       /// <summary>
       /// same as SupportedArffFunctions, but these functions do not require parameters
       /// </summary>
        public static string[] SupportedArffFunctionsWOP = new string[]{
            "removeUnusedValues",
            "getMemorySize"
        };

        /// <summary>
        /// returns the exploded single string version of SupportedArffFunctions
        /// </summary>
        public static string SupportedArffFunctionExploded
        {
            get
            {
                return ArrayExplode(SupportedArffFunctions);
            }
        }

        /// <summary>
        /// used in the getters of exploded arrays to turn array elements into concatted string
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private static string ArrayExplode(string[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in array)
            {
                sb.Append(s);
                sb.Append(',');
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }

        /// <summary>
        /// main debug function of the lib (writes to log; if LOG == true)
        /// </summary>
        /// <param name="msg"></param>
        public static void Debug(string msg)
        {
            if (DEBUG)
                Console.WriteLine(msg);

            if (LOG)
            {
                try
                {
                    Common.LogFile.AutoWrite(msg);
                }
                catch (Exception ex)
                {
                    Helpers.Utils.Debug("Failed to write to Logfile! " + ex.Message);
                }
            }
        }

        /// <summary>
        /// nested static class that returns members of the assembly(lib)
        /// </summary>
        public static class StaticAssembly
        {
            /// <summary>
            /// this library
            /// </summary>
            public static readonly Assembly Reference = typeof(StaticAssembly).Assembly;

            /// <summary>
            /// version of this library
            /// </summary>
            public static readonly Version Version = Reference.GetName().Version;

            /// <summary>
            /// author of this library
            /// </summary>
            public static String Author
            {
                get
                {
                    object[] attributes = Reference.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                    if (attributes.Length > 0)
                        return ((AssemblyCompanyAttribute)attributes[0]).Company;
                    else
                        return "C. Froehlingsdorf - ceka@5cf.de";
                }
            }
        }
    }
}