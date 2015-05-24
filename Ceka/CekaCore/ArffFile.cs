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

namespace DM.Ceka
{
    /// <summary>
    /// static class that contains mostly constant values and delimiters for ARFF file parsing and creation
    /// </summary>
    public static class ArffFile
    {
        /// <summary>
        /// .arff
        /// </summary>
        public const string FILE_EXTENSION = ".arff";

        /// <summary>
        /// @relation
        /// </summary>
        public const string ARFF_RELATION = "@relation";

        /// <summary>
        /// @data
        /// </summary>
        public const string ARFF_DATA = "@data";

        /// <summary>
        /// @attribute
        /// </summary>
        public const string ARFF_ATTRIBUTE = "@attribute";


        /// <summary>
        /// @
        /// </summary>
        public const char ATT_PRE = '@';

        /// <summary>
        /// ,
        /// </summary>
        public const char STORY_DELIMITTER = ',';

        /// <summary>
        /// '
        /// </summary>
        public const char ATT_NAME_COMB = '\'';

        /// <summary>
        /// " "
        /// </summary>
        public const char ARFF_SPACE = ' ';

        /// <summary>
        /// {
        /// </summary>
        public const char ATT_START = '{';

        /// <summary>
        /// }
        /// </summary>
        public const char ATT_END = '}';

        /// <summary>
        /// %
        /// </summary>
        public const char ATT_IGNO = '%';

        /// <summary>
        /// ?
        /// </summary>
        public const char ATT_EMPTY_VALUE = '?';


        /// <summary>
        /// numeric
        /// </summary>
        public const string ATT_UNS = "numeric";

        /// <summary>
        /// real
        /// </summary>
        public const string ATT_UNS_2 = "real";

        /// <summary>
        /// UNDEFINED
        /// </summary>
        public const string ATT_UNDEFINED = "UNDEFINED";

        /// <summary>
        /// |
        /// </summary>
        public const char ATT_SPACE_EXCHANGE = '|';
       

        /// <summary>
        /// current version of ceka
        /// </summary>
        public static string CEKA_VERSION {
            get //updated in v.1.2.1
            {
                return Helpers.Utils.StaticAssembly.Version.ToString();
            }
        }

        /// <summary>
        /// author string of ceka
        /// </summary>
        public static string CEKA_AUTHOR {
            get 
            { //updated in v.1.2.1
                return Helpers.Utils.StaticAssembly.Author;
            }
        }

        /// <summary>
        /// eof description
        /// </summary>
        public const string CEKA_EOF = "EOF CEKA.ARFF";
    }
}