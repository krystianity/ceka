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

namespace DM.Ceka.Loader
{
    /// <summary>
    /// abstract wrapper for the loader interface
    /// </summary>
    public abstract class AbstractLoader : ILoader
    {
        /// <summary>
        /// empty constructor
        /// </summary>
        public AbstractLoader()
        {
        }

        /// <summary>
        /// file to be loaded
        /// </summary>
        /// <param name="file"></param>
        public AbstractLoader(string file)
        {
            this.file = file;
        }

        protected string file;
        protected StreamReader reader;
        protected string content;

        public abstract string Content { set; get; }
        public abstract StreamReader Reader { set; get; }

        public abstract Library Dataset { set; get; }
        public abstract Library Headers { set; get; }

        public abstract void loadArff();
    }
}