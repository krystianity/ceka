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
using System.Linq;
using System.Text;

using System.IO;

namespace DM.Ceka.Common
{
    /// <summary>
    /// static logfile class, logs debug messages if enabled in helpers
    /// </summary>
    public static class LogFile
    {
        /// <summary>
        /// file to write the log to
        /// </summary>
        public static string FILE = "ceka.log";

        /// <summary>
        /// writes a log-message to the logfile
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="w"></param>
        public static void Log(string msg, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", msg);
            w.WriteLine("-------------------------------");
        }

        /// <summary>
        /// creates streamwriter for a single log-entry (..and writes to log)
        /// </summary>
        /// <param name="msg"></param>
        public static void AutoWrite(string msg){
            using (StreamWriter w = File.AppendText(FILE)){
                Log(msg, w);
            }
        }

    }
}