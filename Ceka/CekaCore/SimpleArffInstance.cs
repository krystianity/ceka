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
    /// simplified version of ArffInstance
    /// </summary>
    [Serializable]
    public class SimpleArffInstance
    {
        /// <summary>
        /// name of the ARFF file (dataset) = @relation
        /// </summary>
        public string Relation { set; get; }

        /// <summary>
        /// @attribute lines content, values are split in List-string-
        /// </summary>
        public Dictionary<String, List<string>> Attributes { set; get; }

        /// <summary>
        /// @data lines, (data instances)
        /// </summary>
        public List<List<string>> Datasets { set; get; }

        /// <summary>
        /// turns a complex Library and Story related ArffInstance into a way simpler Data Structure,
        /// which is easier to understand, and requires less memory space
        /// </summary>
        /// <param name="ai"></param>
        public SimpleArffInstance(ArffInstance ai)
        {
            this.Attributes = new Dictionary<string, List<string>>();
            this.Datasets = new List<List<string>>();

            this.Relation = ai.Relation;

            foreach (Story st in ai.Headers.Data)
            {
                this.Attributes.Add(st.Name, st.Data.ToList<string>());
            }

            foreach (Story st in ai.Datasets.Data)
            {
                this.Datasets.Add(st.Data.ToList<string>());
            }
        }

        /// <summary>
        /// turns a simple arff instance into the complex Library and Story related ArffInstance Type
        /// </summary>
        /// <returns></returns>
        public ArffInstance ToComplexInstance()
        {
            ArffInstance ai = new ArffInstance();

            ai.Headers = new Library();
            ai.Datasets = new Library();
            ai.Headers.Data = new List<Story>();
            ai.Datasets.Data = new List<Story>();

            ai.Relation = Relation;

            Story st = null;
            foreach (KeyValuePair<string, List<string>> story in this.Attributes)
            {
                st = new Story(story.Key, story.Value.ToArray<string>(), EArffTypes.ATTRIBUTE);
                ai.Headers.Data.Add(st);
            }

            foreach (List<string> set in this.Datasets)
            {
                st = new Story(null, set.ToArray<string>(), EArffTypes.DATA);
                ai.Datasets.Data.Add(st);
            }

            return ai;
        }

        /// <summary>
        /// returns the in-memory byte size of the simple arff instance
        /// </summary>
        /// <returns></returns>
        public int GetMemorySize()
        {
            return (int)(Helpers.Utils.GetObjectMemoryUsage(this) / 1024);
        }

    }
}