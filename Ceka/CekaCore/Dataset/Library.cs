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
    /// represents an @attribute- or @data instance collection from an ARFF dataset
    /// </summary>
    [Serializable]
    public class Library
    {
        /// <summary>
        /// this class just wraps around this list
        /// </summary>
        protected List<Story> content;

        /// <summary>
        /// empty constructor, set content via Library.Data = value;
        /// </summary>
        public Library()
        {
            this.content = new List<Story>();
        }

        /// <summary>
        /// the content (@attribute or @data dataset instances / rows)
        /// </summary>
        public List<Story> Data
        {
            get
            {
                return this.content;
            }
            set
            {
                this.content = value;
            }
        }

        /// <summary>
        /// amount of attributes or dataset instances
        /// </summary>
        /// <returns></returns>
        public int headerSize()
        {
            return this.content.Count;
        }

        /// <summary>
        /// add an attribute or instance to the library (represented as Story)
        /// </summary>
        /// <param name="s"></param>
        public void Add(Story s)
        {
            this.content.Add(s);
        }

        /// <summary>
        /// remove an attribute or instance from the library, do not use this for data instances!
        /// </summary>
        /// <param name="s"></param>
        public void Remove(Story s)
        {
            if (s.Type == EArffTypes.DATA)
                throw new CekaException("please do not use this function for @data instances!");

            this.content.Remove(s);
        }

        /// <summary>
        /// removes unused attribute types that were created during parsing an arff file
        /// </summary>
        public void CleanUp()
        {
            bool removed = false;

            foreach (Story s in this.content)
            {
                if (s.Type == EArffTypes.NONE || s.Type == EArffTypes.DATA_ATT)
                {
                    this.content.Remove(s);
                    removed = true;
                    break;
                }
            }

            if (removed) //recurse
                this.CleanUp();
        }

        /// <summary>
        /// checking if the size of a single story-dataset differs from the size of the defined headers
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public bool integretyCheck(Library header)
        {
            int hs = header.headerSize();

            foreach (Story s in this.content)
            {
                if (s.getDatasetSize() != hs)
                {
                    Helpers.Utils.Debug("size (" + s.getDatasetSize() + ") of dataset l." + s.arffFileLine + " differs from header size (" + hs + ").");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// checking if the values of each single story-dataset element are allowed as they have to be defined in the headers
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public bool deepIntegretyCheck(Library header)
        {
            int sti = 0;
            //run through all stories
            foreach (Story st in this.content)
            {
                //run through every single story-dataset element
                for (int i = 0; i < st.Data.Length; i++)
                {
                    //check if the appropriat header attribute definition contains the value of the dataset:

                    if (header.content[i].isNumeric)
                        continue; //skip this entry because it belongs to an numeric attribute type

                    if(!header.content[i].Values.Contains(st.Data[i]))
                    {
                        if (string.CompareOrdinal(st.Data[i], ArffFile.ATT_EMPTY_VALUE.ToString()) == 0)
                        {
                            Helpers.Utils.Debug(String.Format("ignored row {0} column {1} as the value was empty={2}.", sti, i, ArffFile.ATT_EMPTY_VALUE));
                            continue;
                        }
                        Helpers.Utils.Debug("dataset(" + i + ") element (" + st.Data[i] + ") does not fit to the defined range of attribute (" + header.content[i].Name + ").");
                        return false;
                    }
                }

                sti++;
            }

            return true;
        }

        /// <summary>
        /// run this only on datasets! (a library that contains datasets only)
        /// </summary>
        public void removeEmptyValueDatasets()
        {
            for (int i = this.content.Count - 1; i >= 0; i--)
            {
                if (this.content[i].containsEmptyValue())
                {
                    this.content.Remove(this.content[i]);
                    Helpers.Utils.Debug(string.Format("Removed Dataset {0} as of empty values.", i));
                }
            }
        }

        /// <summary>
        /// it will find the @relation story and return its name value
        /// run this only on attributes! (a library that contains attributes only)
        /// </summary>
        /// <returns></returns>
        public string getRelationName()
        {
            foreach (Story s in this.content)
            {
                if (s.Type == EArffTypes.RELATION)
                {
                    return s.Name;
                }
            }

            throw new CekaException("trying to get relation from a headers library but can not find a story with relation type");
        }

        /// <summary>
        /// removes the @relation story from the library
        /// run this only on attributes! (a library that contains attributes only)
        /// </summary>
        public void removeRelation()
        {
            foreach (Story s in this.content)
            {
                if (s.Type == EArffTypes.RELATION)
                {
                    this.content.Remove(s);
                    break;
                }
            }
        }

        /// <summary>
        /// returns a deep copy of this library
        /// </summary>
        /// <returns></returns>
        public Library toCopy()
        {
            Library nl = new Library();
            Story[] cc = new Story[this.content.Count];
            this.content.CopyTo(cc);
            nl.content = new List<Story>(cc);
            return nl;
        }

    }
}