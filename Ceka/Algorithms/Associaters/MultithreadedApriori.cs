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

namespace DM.Ceka.Algorithms.Associaters
{
    /// <summary>
    /// multithreaded version of the single threaded Apriori
    /// </summary>
    public sealed class MultithreadedApriori
    {
        /// <summary>
        /// empty constructor
        /// </summary>
        public MultithreadedApriori()
        {
           //i.d.
        }

        /// <summary>
        /// nested result object for the multithread apriori
        /// </summary>
        public class NestedFastAprioriResult
        {
            /// <summary>
            /// empty constructor
            /// </summary>
            public NestedFastAprioriResult()
            {
                //i.d.
            }
        }
    }
}