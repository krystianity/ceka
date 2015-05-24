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
    /// a story object is very ambigous it can hold any kind of "line-data", depending on the @"definition" in the file before, which can be later evaluated by its "this.type" property
    /// </summary>
    [Serializable]
    public class Story
    {
        /// <summary>
        /// name of the story (heading attribute)
        /// </summary>
        protected string name;

        /// <summary>
        /// value of the story (heading attribute), stores the numeric definition if isNumeric == true
        /// </summary>
        protected string value;

        /// <summary>
        /// most important property
        /// </summary>
        protected EArffTypes type;

        /// <summary>
        /// for (data)sets => @data
        /// </summary>
        protected string[] data = new string[0];

        /// <summary>
        /// for(header)sets => @attribute
        /// </summary>
        protected string[] values = new string[0];

        /// <summary>
        /// added on 28.02.2015 to support numeric attributes in arff files, indicates wheather this story is the definition of a numeric attribute or not
        /// </summary>
        public bool isNumeric { set; get; }

        /// <summary>
        /// simple constructor to build Storys
        /// </summary>
        public Story()
        {
            this.isNumeric = false;
        }

        /// <summary>
        /// main constructor that automatically takes care about parsing the arff line
        /// </summary>
        /// <param name="arffLine"></param>
        /// <param name="line"></param>
        public Story(string arffLine, int line)
        {
            this.isNumeric = false;
            this.arffFileLine = line;
            parseType(arffLine);
        }

        /// <summary>
        /// constructor for a faster init, that also enables to set the values of the dataset
        /// </summary>
        /// <param name="name"></param>
        /// <param name="set"></param>
        /// <param name="type"></param>
        public Story(string name, string[] set, EArffTypes type)
        {
            this.isNumeric = false;
            this.name = name;
            this.type = type;

            if (type == EArffTypes.ATTRIBUTE)
                this.values = set;
            else if (type == EArffTypes.DATA)
                this.data = set;
            else
                throw new CekaException("can not set any values or dataset(elements) for the type " + type.ToString());
        }

        /// <summary>
        /// this should be mainly used to access the data behind the story, because it does not require the understanding of the generated Story
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string this[int i]
        {
            get
            {
                if (this.type == EArffTypes.ATTRIBUTE)
                    return this.Values[i];
                else if (this.type == EArffTypes.DATA)
                    return this.Data[i];
                else
                    throw new CekaException("this Story is neither an Attribute nor a Dataset(element)!");
            }
            set
            {
                if (this.type == EArffTypes.ATTRIBUTE)
                    this.Values[i] = value;
                else if (this.type == EArffTypes.DATA)
                    this.Data[i] = value;
                else
                    throw new CekaException("this Story is neither an Attribute nor a Dataset(element)!");
            }
        }

        /*
        /// <summary>
        /// this is used to make the inner datasets available to foreach functions
        /// </summary>
        public IEnumerable<string> Enum
        {
            get
            {
                if (this.type == EArffTypes.ATTRIBUTE)
                    return this.Values;
                else if (this.type == EArffTypes.DATA)
                    return this.Data;
                else
                    throw new CekaException("this Story is neither an Attribute nor a Dataset(element)");
            }
            set
            {
                throw new CekaException("Enum of a Story can not bet set!");
            }
        } */

        
        /// <summary>
        /// to make sure Stories can be easily used in loops
        /// </summary>
        /// <returns></returns>
        public int Length()
        {
            /* using an accessor here failed pretty bad! */
            if (this.type == EArffTypes.ATTRIBUTE)
                return this.Values.Length;
            else if (this.type == EArffTypes.DATA)
                return this.Data.Length;
            else
                throw new CekaException("this Story is neither an Attribute nor a Dataset(element)!");
        }
         

        /// <summary>
        /// this will return the ARFF File Line Number (if the Story was loaded from an ARFF-File (line))
        /// </summary>
        public int arffFileLine { set; get; }

        /// <summary>
        /// these are the values of the definition of an attributes span "@attributes", made internal because this[] use is safer
        /// </summary>
        public string[] Values
        {
            get
            {
                /* if ((this.values == null || this.values.Length <= 0) && (this.data != null && this.data.Length > 0))
                    throw new CekaException("You are trying to use the Values of a Story but this Story is probably part of a Dataset, please use Data and not Values."); */

                return this.values;
            }
            set
            {
                //throw new CekaException("can not set the value of a story's values");
                this.values = value;
            }
        }

        /// <summary>
        /// these are the values of simple dataset "@data", made internal because this[] use is safer
        /// </summary>
        public string[] Data
        {
            get
            {
                /* if ((this.data == null || this.data.Length <= 0) && (this.values != null && this.values.Length > 0))
                    throw new CekaException("You are trying to use the Data of a Story but this Story is probably part of Headers, please use Values and not Data."); */

                /* the major problem with this class is, that every single property that has to be copied with ArffInstance.ToCopy() 
                 * needs to be public and accessable.. because of the Newtonsoft.Json call
                 * that makes it difficult to include Exceptions, like the one above - it will make it impossble to clone or store the object via json
                 * because Newtonsoft.Json will call all properties to store their value */

                return this.data;
            }
            set
            {
                //throw new CekaException("can not set the value of a story's datasets");
                this.data = value;
            }
        }

        /// <summary>
        /// returns the name of an header-attribute, if this line describes a header attribute it something like "this" {..}
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                //throw new CekaException("can not set value of a story's name");
                this.name = value;
            }
        }

        /// <summary>
        /// return the value of an header-attribute
        /// </summary>
        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        /// <summary>
        /// as every Story is read and parsed from an ARFF line, the Type of the Story describes the ultimate definition of the value that it holds
        /// </summary>
        public EArffTypes Type
        {
            get
            {
                return this.type;
            }
            set
            {
                //throw new CekaException("can not set the value of a story type");
                this.type = value;
            }
        }

        /// <summary>
        /// this function takes care of the definition of the ARFF File line (that is passed as parameter) and sets the value(s) and type of the Story
        /// </summary>
        /// <param name="al"></param>
        private void parseType(string al)
        {
            //making sure we sort out the empty or new lines first
            if (string.IsNullOrEmpty(al) || string.IsNullOrWhiteSpace(al) || al.Length <= 2)
            {
                this.value = "";
                this.type = EArffTypes.NONE;
                return;
            }

            //next up is a fast check if the story might be a higher type
            if (al[0].Equals(ArffFile.ATT_PRE))
            {
                //find out what kind of type it is
                string[] _data = al.Split(ArffFile.ARFF_SPACE);

                if (_data.Length <= 0)
                    throw new CekaException("this story (" + al + ") seems to be an attribute"
                        + " but there is something wrong with the whitespaces in your file.");

                //switch on behalf of the attribute's type
                //(just make sure everything is put in place here, the rest is done by the library later on)
                switch(_data[0])
                {
                    case ArffFile.ARFF_RELATION:
                        this.type = EArffTypes.RELATION;
                        //a relation just ends on the 2n index
                        this.name = _data[1];
                        this.value = "relation-type";
                        break;

                    case ArffFile.ARFF_ATTRIBUTE:
                        this.type = EArffTypes.ATTRIBUTE;
                        al = al.Replace(_data[0], "");

                        if (testForNumericContent(al))
                        {
                            //value indicates that this attribute is a numeric or REAL type, parse it!
                            parseNumeric(al);
                        }
                        else
                        {
                            //value and name have to be splitted
                            filterNameAndValue(al);
                            //to make sure we even drill down into the single elements of attributes values
                            furtherSplitOfValue();
                        }
                        break;

                    case ArffFile.ARFF_DATA:
                        this.type = EArffTypes.DATA_ATT; //beware of the difference between DATA_ATT and DATA!
                        //nothing to do here, its just the end of the header section == @data
                        this.value = "data-type";
                        this.name = "none";
                        break;

                    default:
                        throw new CekaException("this story (" + al + ") seemed to be alright, but the attribute "
                        + "type (" + _data[0] + ") is unknown.");
                }
            }
            else
            {
                //it is no higher type, its a clean data-line
                if (al.Contains(ArffFile.ARFF_SPACE))
                    throw new CekaException("this story (data) (" + al + ") is including whitespaces.");

                //simply add the data of the line to this story
                this.data = al.Split(ArffFile.STORY_DELIMITTER);

                if (this.data.Length <= 0)
                    throw new CekaException("this story (" + al + ") seems to be a " 
                    + "dataset, although its not using any '" + ArffFile.STORY_DELIMITTER + "'.");

                StringBuilder sb = null;
                //an remove any "/'" s (should be clean of whitespaces already)
                for (int i = 0; i < this.data.Length; i++)
                {
                    sb = new StringBuilder();

                    foreach (char c in data[i])
                    {
                        if (!c.Equals(ArffFile.ATT_NAME_COMB))
                            sb.Append(c);
                    }

                    data[i] = sb.ToString();
                }

                this.type = EArffTypes.DATA;
                this.value = "data-value";
            }
        }

        /// <summary>
        /// added on 28.02.2015 to support numeric values in ARFF files
        /// </summary>
        /// <param name="_value"></param>
        private bool testForNumericContent(string _value)
        {
            //_value is possibly something like this: "'power consumption' numeric" or "'power consumption REAL'"

            //having something like "'REAL power consumption' .." or "'numeric values' .." will cause this to interpret it wrong!
 
            if(_value.Contains(ArffFile.ATT_START))
                return false; //contains { -> it cannot be numeric or REAL

            string[] split = _value.Split(ArffFile.ARFF_SPACE);
            foreach (string s in split)
            {
                if (string.CompareOrdinal(s, ArffFile.ATT_UNS) == 0 || string.CompareOrdinal(s, ArffFile.ATT_UNS_2) == 0)
                {
                    //contains numeric or REAL as attribute description
                    return true;
                }
            }

            //this shouldnt happen!
            throw new CekaException("@attribute is neither a defined range nor a numeric or REAL value!");
        }

        /// <summary>
        /// added on 28.02.2015 to support numeric values in ARFF files
        /// </summary>
        /// <param name="_value"></param>
        private void parseNumeric(string _value)
        {
            //contains something like this: "'power consumption'" REAL, OR this "powerConsumption numeric"
            this.isNumeric = true;

            /* funny thing is, that the filterNameandValue function, actually applys for numeric attribute types as well! */
            filterNameAndValue(_value);
        }

        /// <summary>
        /// this function takes care of evaluating the values stored in an ARFF File line that contains an @attribute description "this" {..}
        /// </summary>
        /// <param name="_value"></param>
        private void filterNameAndValue(string _value)
        {
            //_value is possibly something like this: "'power consumption' {1, 2, 3}"

            //having something like " power consumption {'1', '2', '3'} will cause a crash! "

            if (_value.Contains(ArffFile.ATT_NAME_COMB))
            {
                //the name is possibly splitted by whitespace (enclosed by ')
                int sI = -1, eI = -1;
                char[] sa = _value.ToCharArray();
                for (int i = 0; i < sa.Length; i++)
                {
                    if (sa[i].Equals(ArffFile.ATT_NAME_COMB))
                    {
                        if (sI == -1)
                            sI = i;
                        else if (sI != -1 && eI == -1)
                            eI = i;
                        else
                            break; //we could also have something like " 'power consumption' {'1', '2', '3'} " that would cause a crash here
                            //throw new CekaException("splitting name whitespace, but there are more then 2 uses of: " + ArffFile.ATT_NAME_COMB + ", try to encapsulate all attribute names with " + ArffFile.ATT_NAME_COMB);
                    }
                }

                if (sI != -1 && eI == -1)
                    throw new CekaException("splitting name whitespace, found 1 but missing one of: " + ArffFile.ATT_NAME_COMB);

                //concreting the name from the counted span
                StringBuilder nb = new StringBuilder();
                for (int i = sI + 1; i < eI; i++)
                {
                    nb.Append(sa[i]);
                }

                this.name = nb.ToString();

                nb.Clear();
                for (int i = eI + 1; i < sa.Length; i++)
                {
                    nb.Append(sa[i]);
                }

                this.value = nb.ToString();
            }
            else
            {
                //just grabbing the name out of the string
                string[] _data = _value.Split(ArffFile.ARFF_SPACE);
                List<string> rd = new List<string>();

                foreach (string s in _data)
                    if (!string.IsNullOrWhiteSpace(s) && !string.IsNullOrEmpty(s))
                        rd.Add(s); //clean whitespaces or empty strings out of this thingcf

                _data = rd.ToArray<string>();

                if (_data.Length <= 1)
                    throw new CekaException("cant split the whitespace because there is non in: " + _value);

                this.name = _data[0];
                this.value = _value.Replace(_data[0], "");
               //this.value = data[1]; <-- this will not work, because there are more than one whitespace in the value
            }
        }

        /// <summary>
        /// this function works together with filterNameAndValue(string _value) and is used to parse an ARFF file line of @attribute type
        /// </summary>
        private void furtherSplitOfValue()
        {
            //this.value is possibly something like this: {power=base, power=peak}"

            if (string.IsNullOrEmpty(this.value) || string.IsNullOrWhiteSpace(this.value))
                throw new CekaException("further split is being called but this story's value has no value!");

            int sI = -1, eI = -1;
            char[] sa = this.value.ToCharArray();
            for (int i = 0; i < sa.Length; i++)
            {
                if (sa[i].Equals(ArffFile.ATT_START))
                {
                    if (sI == -1)
                        sI = i;
                    else
                        throw new CekaException("further splitting but " + ArffFile.ATT_START + " is used multiple times.");
                }

                if (sa[i].Equals(ArffFile.ATT_END))
                {
                    if (eI == -1)
                        eI = i;
                    else
                        throw new CekaException("further splitting but " + ArffFile.ATT_END + " is used multiple times.");
                }
            }

            if (sI == -1 || eI == -1)
                throw new CekaException("further splitting will fail due to missing "
            + "start(" + ArffFile.ATT_START + ")/end(" + ArffFile.ATT_END + ") descriptions for an attribute.");

            StringBuilder vb = new StringBuilder();
            for (int i = sI + 1; i < eI; i++)
            {
                if(!sa[i].Equals(ArffFile.ARFF_SPACE) && !sa[i].Equals(ArffFile.ATT_NAME_COMB)) //no whitespaces and no '
                    vb.Append(sa[i]);
            }

            string sb = vb.ToString();
            this.values = sb.Split(ArffFile.STORY_DELIMITTER);

            if (this.values.Length <= 0)
                throw new CekaException("further splitting ends with empty values on: " + this.value);
        }

        /// <summary>
        /// this function should only be called if the Story is a @data line and contains a dataset (it will return its size)
        /// </summary>
        /// <returns></returns>
        public int getDatasetSize()
        {
            if (this.type != EArffTypes.DATA)
                throw new CekaException("cannot gather the size of a dataset from a story that is not a type of DATA; its " + this.type.ToString());

            return this.data.Length;
        }

        /// <summary>
        /// this function can be used if a dirty version of a string containing all dataset values is needed
        /// </summary>
        /// <param name="delim"></param>
        /// <returns></returns>
        public string explodeWithDelimiter(char delim)
        {
            if (this.type != EArffTypes.DATA)
                throw new CekaException("cannot explode a Story that is not of Type=DATA!");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < this.Data.Length; i++)
            {
                sb.Append(this.Data[i]);

                if (i < this.Data.Length - 1)
                    sb.Append(delim);
            }

            return sb.ToString();
        }

        /// <summary>
        /// checks if this dataset contains '?' and is therefore possible irrelevant to algorithms that require clean and functioning datasets
        /// </summary>
        /// <returns></returns>
        public bool containsEmptyValue()
        {
            if (this.data == null || this.data.Length <= 0)
                return false;

            for (int i = 0; i < this.data.Length; i++)
            {
                if (string.CompareOrdinal(this.data[i], ArffFile.ATT_EMPTY_VALUE.ToString()) == 0)
                    return true;
            }

            return false;
        }

    }
}