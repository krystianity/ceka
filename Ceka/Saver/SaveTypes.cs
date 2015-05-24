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

namespace DM.Ceka.Saver
{
    /// <summary>
    /// possible save types for the apriori algorithm (used for fast-apriori as well)
    /// </summary>
    public enum AprioriSaveTypes
    {
        /// <summary>
        /// default
        /// </summary>
        NONE = 0,

        /// <summary>
        /// JSON standard
        /// </summary>
        JSON = 1,

        /// <summary>
        /// JSON standard, human-readable (expanded)
        /// </summary>
        JSON_PRETTY = 2,

        /// <summary>
        /// imitates WEKA's association rules miner result
        /// </summary>
        WEKA = 3
    }

    /// <summary>
    /// possible save types for the supporting vector machine algorithm
    /// </summary>
    public enum SVMSaveTypes
    {
        NONE = 0
    }

    /// <summary>
    /// possible save types for the cobweb algorithm
    /// </summary>
    public enum CobwebSaveTypes
    {
        NONE = 0
    }

    /// <summary>
    /// possible save types for the kmeans algorithm
    /// </summary>
    public enum KMeansSaveTypes
    {
        NONE = 0
    }
}