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

using Newtonsoft.Json;
using System.IO;

namespace DM.Ceka.Saver
{
    /// <summary>
    /// wraps the result saver and adds JSON serialisation in constructors
    /// </summary>
    public class SimpleJsonSaver : ResultSaver
    {
        //could have used this for any other algorithm result-set, but the typed constructors feel cleaner
        /// <summary>
        /// save any object
        /// </summary>
        /// <param name="res"></param>
        /// <param name="pretty"></param>
        public SimpleJsonSaver(object res, bool pretty = false)
        {
            if (!pretty)
                this.value = JsonConvert.SerializeObject(res);
            else
                this.value = JsonConvert.SerializeObject(res, Formatting.Indented);
        }

        /// <summary>
        /// save flexible apriori result buffer (List-List-string--)
        /// </summary>
        public SimpleJsonSaver(List<List<string>> flex_apriori_result_buffer, bool pretty = false)
        {
            if (!pretty)
                this.value = JsonConvert.SerializeObject(flex_apriori_result_buffer);
            else
                this.value = JsonConvert.SerializeObject(flex_apriori_result_buffer, Formatting.Indented);
        }

        /// <summary>
        /// save strict result object to JSON file (AprioriResult)
        /// </summary>
        /// <param name="res"></param>
        /// <param name="pretty"></param>
        public SimpleJsonSaver(Ceka.Algorithms.Associaters.Apriori.NestedAprioriResult res, bool pretty = false)
        {
            if (!pretty)
                this.value = JsonConvert.SerializeObject(res);
            else
                this.value = JsonConvert.SerializeObject(res, Formatting.Indented);
        }

        /// <summary>
        /// save strict result object to JSON file (MultiAprioriResult)
        /// </summary>
        /// <param name="res"></param>
        /// <param name="pretty"></param>
        public SimpleJsonSaver(Ceka.Algorithms.Associaters.MultithreadedApriori.NestedFastAprioriResult res, bool pretty = false)
        {
            if (!pretty)
                this.value = JsonConvert.SerializeObject(res);
            else
                this.value = JsonConvert.SerializeObject(res, Formatting.Indented);
        }

        /// <summary>
        /// save strict result object to JSON file (SVMResult)
        /// </summary>
        /// <param name="res"></param>
        /// <param name="pretty"></param>
        public SimpleJsonSaver(Ceka.Algorithms.Classifiers.SupportVectorMachine.NestedSVMResult res, bool pretty = false)
        {
            if (!pretty)
                this.value = JsonConvert.SerializeObject(res);
            else
                this.value = JsonConvert.SerializeObject(res, Formatting.Indented);
        }

        /// <summary>
        /// save strict result object to JSON file (CobwebResult)
        /// </summary>
        /// <param name="res"></param>
        /// <param name="pretty"></param>
        public SimpleJsonSaver(Ceka.Algorithms.Clusterers.Cobweb.NestedCobwebResult res, bool pretty = false)
        {
            if (!pretty)
                this.value = JsonConvert.SerializeObject(res);
            else
                this.value = JsonConvert.SerializeObject(res, Formatting.Indented);
        }

        /// <summary>
        /// save strict result object to JSON file (KMeansResult)
        /// </summary>
        /// <param name="res"></param>
        /// <param name="pretty"></param>
        public SimpleJsonSaver(Ceka.Algorithms.Clusterers.KMeans.NestedKMeansResult res, bool pretty = false)
        {
            if (!pretty)
                this.value = JsonConvert.SerializeObject(res);
            else
                this.value = JsonConvert.SerializeObject(res, Formatting.Indented);
        }

    }
}