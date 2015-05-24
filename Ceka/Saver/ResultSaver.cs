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
using System.Text;

namespace DM.Ceka.Saver
{
    /// <summary>
    /// abstract class for all algorithm related result saving (writing to file or printing to console)
    /// </summary>
    public class ResultSaver
    {
        /// <summary>
        /// result value as string
        /// </summary>
        protected string value;

        /// <summary>
        /// empty constructor 
        /// </summary>
        protected ResultSaver() { }

        /// <summary>
        /// saves the result (this.value) to file
        /// </summary>
        /// <param name="file"></param>
        public void SaveToFile(string file)
        {
            using (StreamWriter os = new StreamWriter(file))
            {
                os.Write(this.value);
            }
        }

        /// <summary>
        /// prints (saves) the result (this.value) to the console
        /// </summary>
        public void CLI()
        {
            Console.WriteLine(this.value);
        }
    }
}