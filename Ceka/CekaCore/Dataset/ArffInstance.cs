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

namespace DM.Ceka
{
    /// <summary>
    /// represents an ARFF file in memory
    /// </summary>
    [Serializable]
    public class ArffInstance
    {
        /// <summary>
        /// name of the ARFFfile
        /// </summary>
        public string Relation { set; get; }

        /// <summary>
        /// all Storys that follow from an @attribute block
        /// </summary>
        public Library Headers { set; get; }

        /// <summary>
        /// all Storys that are present after the @data block of the ARFF file
        /// </summary>
        public Library Datasets { set; get; }

        /// <summary>
        /// empty constructor
        /// </summary>
        public ArffInstance()
        {
        }

        /// <summary>
        ///  empty arff instance, pass the relation name
        /// </summary>
        /// <param name="relation"></param>
        public ArffInstance(string relation) : this()
        {
            this.Relation = relation;
            this.Headers = new Library();
            this.Datasets = new Library();
        }

        /// <summary>
        /// fully set arff instance, used by ArffLoader
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="datasets"></param>
        /// <param name="skipIntCheck"></param>
        public ArffInstance(Library headers, Library datasets, bool skipIntCheck = false) : this()
        {
            if (headers == null || datasets == null)
                throw new CekaException("can not pass null for library values of an ArffInstance");

            this.Headers = headers;
            this.Datasets = datasets;

            Headers.CleanUp();
            Datasets.CleanUp();

            this.Relation = Headers.getRelationName(); //grab name out of it
            Headers.removeRelation(); //and delete it afterwards, we dont need it, as we want clean Libraries

            //running the int-check
            if (!skipIntCheck)
                this.integrityCheck();
        }

        /// <summary>
        /// uses the library's dataset-check-functions to validate the loaded arff file components
        /// </summary>
        public void integrityCheck()
        {
            //check if every dataset is made up of the same amount of values as the headers(attributes) define
            if (Datasets.integretyCheck(this.Headers))
                Helpers.Utils.Debug("integrety check passed.");
            else
                throw new CekaException("integrety check failed.");

            //check if every single dataset element is allowed by its correspondant attribute definition
            if (Datasets.deepIntegretyCheck(this.Headers))
                Helpers.Utils.Debug("deep data value scan passed.");
            else
                throw new CekaException("deep data value scan failed.");
        }

        /// <summary>
        /// adds an attribute, non recursive
        /// </summary>
        /// <param name="name"></param>
        /// <param name="span"></param>
        public void addAttribute(string name, string[] span)
        {
            Story s = new Story(name, span, EArffTypes.ATTRIBUTE);
            this.Headers.Add(s);
        }

        /// <summary>
        /// adds an numeric attribute, non recursive, "REAL, numeric"
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void addNumericAttribute(string name, string value)
        {
            Story s = new Story(name, null, EArffTypes.ATTRIBUTE);
            s.isNumeric = true;
            s.Value = value;
            this.Headers.Add(s);
        }

        /// <summary>
        /// changes an attributes values(, non recursive)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="span"></param>
        public void changeAttribute(string name, string[] span)
        {
            int i = this.getIndexOfAttribute(name);

            if (i == -1)
                throw new CekaException("The attribute you are looking for, couldnt be found!");

            this.Headers.Data[i].Values = span;
        }

        /// <summary>
        /// turns attribute type into numeric type "REAL, numeric"
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void turnAttributeIntoNumeric(string name, string value)
        {
            int i = this.getIndexOfAttribute(name);

            if (i == -1)
                throw new CekaException("The attribute you are looking for, couldnt be found!");

            this.Headers.Data[i].Value = value;
            this.Headers.Data[i].Values = null;
            this.Headers.Data[i].isNumeric = true;
        }

        /// <summary>
        /// changes the value of an numeric attribute "REAL, numeric"
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void changeNumericAttribute(string name, string value)
        {
            int i = this.getIndexOfAttribute(name);

            if (i == -1)
                throw new CekaException("The attribute you are looking for, couldnt be found!");

            this.Headers.Data[i].Value = value;
        }

        /// <summary>
        /// removes an attribute, non recursive
        /// </summary>
        /// <param name="name"></param>
        public void removeAttribute(string name)
        {
            int i = this.getIndexOfAttribute(name);

            if(i == -1)
                throw new CekaException("the attribute " + name + " does not exist in this instance!");

            this.Headers.Data.RemoveAt(i);
        }

        /// <summary>
        /// gets the index of an attribute from its headers
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public int getIndexOfAttribute(string attribute)
        {
            int index = -1;
            foreach (Story s in this.Headers.Data)
            {
                index++;
                if (string.CompareOrdinal(s.Name, attribute) == 0)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// does not only remove the attribute from its headers, but also removes every dataset that does not contain the value
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public void removeDatasetsPerAttributeValue(string attribute, string value)
        {
            //get the index of the attribute of its values in the datasets
            int attributeIndex = getIndexOfAttribute(attribute);

            if (attributeIndex == -1)
                throw new CekaException("There is no index for the attribute you have passed: " + attribute);

            if (this.Headers.Data[attributeIndex].isNumeric)
                throw new CekaException("Can not run this on an attribute of numeric type!");

            //remove the attribute from the headers
            this.removeAttribute(attribute);

            Story s = null;
            for(int i = this.Datasets.Data.Count - 1; i >= 0; i--){
                //reverse loop because elements are deleted in it
                s = this.Datasets.Data[i];

                if (s.Data == null || s.Data.Length <= attributeIndex) //skip if there is something wrong with this story
                    continue;

                //check if the attribute's value is the same
                if (string.CompareOrdinal(s.Data[attributeIndex], value) == 0)
                {
                    //if the value of the attribute is wished for, keep the dataset but remove the status
                    //using linq to build a new array where the index of the attribute does not exist
                    s.Data = s.Data.Where((val, idx) => idx != attributeIndex).ToArray();
                }
                else
                {
                    //if the value of the attribute is not what was wished for, the whole dataset is dropped
                    this.Datasets.Data.Remove(s);
                }
            }
        }

        /// <summary>
        /// turns something like this: age {50, 20, 70, 40, 50, -20, 80, 75, 25} into something like this: age {-20-0, 1-21, 22-42, 43-63, 64-84}
        /// uses the whole dataset ('s values) for the range building and exchanges the total dataset values with the representative range values
        /// makes use of the DM.Ceka.Helpers.RangeBuilder class
        /// </summary>
        /// <param name="attribute_index">the index of the attribute that contains numeric values</param>
        /// <param name="rangeSteps">the steps that each range should cover, 100-200 = 10, 1000-2000 = 100, 10000-20000 = 1000 ..</param>
        public void rebuildAttributeValueByRange(int attribute_index, int rangeSteps = 10)
        {
            try { string str = this.Datasets.Data[0].Data[attribute_index];}
            catch(Exception ex){ throw new CekaException("There is something wrong with your dataset, cannot apply range builder..\n" + ex.Message); }

            int lowBound = 0;
            int highBound = 0;

            int[] carry = new int[this.Datasets.Data.Count]; //performance

            int range = 0;
            for (int i = 0; i < this.Datasets.Data.Count; i++)
            {
                if (int.TryParse(this.Datasets.Data[i].Data[attribute_index], out range))
                {
                    carry[i] = range;

                    if (lowBound > range){
                        lowBound = range;
                        continue; //performance
                    }

                    if (highBound < range)
                        highBound = range;
                }
                else
                {
                    throw new CekaException(string.Format("Cannot apply range builder, because parsing of a value in {0} failed at dataset index {1}!", attribute_index, i));
                }
            }
            Helpers.Utils.Debug(string.Format("Building ranges for attribute {0} - Bounds: {1}<->{2} for {3} Datasets.", attribute_index, lowBound, highBound, carry.Length));

            int boundSplit = ((0 - lowBound) + highBound) / 2; //performance
            List<Helpers.RangeBuilder> r = Helpers.RangeBuilder.BuildRange(lowBound, highBound, rangeSteps);
            //OVERRIDE DATASET VALUES WITH RANGE VALUE
            for (int i = 0; i < carry.Length; i++)
            {
                this.Datasets.Data[i].Data[attribute_index] = Helpers.RangeBuilder.ValueOf(r, carry[i], boundSplit);
            }
            //OVERRIDE POSSIBLE ATTRIBUTE VALUES WITH ALL RANGES
            this.Headers.Data[attribute_index].Values = Helpers.RangeBuilder.ConvertToSimpleRanges(r);
        }

        /// <summary>
        /// removes datasets that fit to a specified pattern, you can use "*" wildcards
        /// </summary>
        /// <param name="pattern"></param>
        public void deletePatternMatchingDatasets(List<string> pattern)
        {
            int count = this.Datasets.Data.Count;
            bool similiar = false;
            for(int x = this.Datasets.Data.Count - 1; x >= 0; x--){
                similiar = true;
                for(int i = 0; i < pattern.Count; i++){
                    if (pattern[i] == "*")
                        continue; //skip on wildcard

                    if(string.CompareOrdinal(pattern[i], this.Datasets.Data[x].Data[i]) != 0){
                        similiar = false;
                        break;
                    }
                }
                if (similiar)
                {
                    this.Datasets.Data.RemoveAt(x);
                }
            }
            int rcount = count - this.Datasets.Data.Count;
            Helpers.Utils.Debug(string.Format("Removed {0} matching datasets.", rcount));
        }

        /// <summary>
        /// removes all attribute values that are not used in the dataset (quad-for-loop: use with caution..)
        /// </summary>
        public void removeUnusedAttributeValues()
        {
            int uc = 0;
            bool throughBreaker = false;
            List<string> tempValueList = null;
            for (int a = 0; a < this.Headers.Data.Count; a++) //all attributes
            {
                if (this.Headers.Data[a].isNumeric)
                    continue; //skipp if this attribute has a numeric type

                tempValueList = new List<string>();
                for (int av = 0; av < this.Headers.Data[a].Values.Length; av++) //every possible value of an attribute
                {
                    throughBreaker = false; //if the attribute value is used break the whole dataset loop and
                    for (int d = 0; d < this.Datasets.Data.Count; d++) //all datasets
                    {
                        if (string.CompareOrdinal(this.Datasets.Data[d][a], this.Headers.Data[a][av]) == 0) //dv == a
                        {
                            //the value of this attributes value is similiar to the value of this datasets value
                            throughBreaker = true;
                            tempValueList.Add(this.Headers.Data[a][av]);
                            break;
                        } 
                    }
                    if (!throughBreaker)
                        uc++;
                }
                //override the attribute values, with the values that are really used in the dataset
                this.Headers.Data[a].Values = tempValueList.ToArray<string>();
            }

            Helpers.Utils.Debug(string.Format("Removed {0} @attribute.values that were not used in the dataset!", uc));
        }

        /// <summary>
        /// walks backwards through attributes values and counts the appearances in the dataset, removes dataset and attribute value if applies
        /// </summary>
        /// <param name="attributeIndex"></param>
        /// <param name="refine"></param>
        public void refineBackRangedAttribute(int attributeIndex, int refine = 2, int minAppearance = 5)
        {
            int rA = 0;
            int rD = 0;
            int apCount = -1;
            List<string> tl;
            List<int> tI = new List<int>();

            int vc = this.Headers.Data[attributeIndex].Values.Length;
            for (int i =  vc - 1; i >= (vc - refine); i--)
            {
                apCount = 0;
                tI.Clear();
                for (int d = 0; d < this.Datasets.Data.Count; d++)
                {
                    if (string.CompareOrdinal(this.Headers.Data[attributeIndex][i], this.Datasets.Data[d][attributeIndex]) == 0)
                    {
                        apCount++;
                        tI.Add(d);

                        if (apCount >= minAppearance)
                            break; //performance
                    }
                }

                if (apCount < minAppearance)
                {
                    rA++;
                    Helpers.Utils.Debug("Removing: " + this.Headers.Data[attributeIndex][i]);
                    //remove attribute's value
                    tl = this.Headers.Data[attributeIndex].Values.ToList<string>();
                    tl.RemoveAt(i);
                    this.Headers.Data[attributeIndex].Values = tl.ToArray<string>();
                    tl = null;

                    //remove all datasets that contain the value
                    for (int di = 0; di < tI.Count; di++)
                    {
                        rD++;
                        this.Datasets.Data.RemoveAt(tI[di]);
                    }
                }
            }

            Helpers.Utils.Debug(string.Format("Refined ranged attributes via backwalk. Removed: {0} Attributes, {1} Datasets.", rA, rD));
        }

        /// <summary>
        /// refines all non numeric/ranged attributes
        /// </summary>
        /// <param name="refine"></param>
        /// <param name="minAppearance"></param>
        public void refineBackAllRangedAttributes(int refine = 2, int minAppearance = 5)
        {
            for (int i = 0; i < this.Headers.Data.Count; i++)
            {
                if (this.Headers.Data[i].isNumeric)
                    continue; //skip on non ranged attributes

                this.refineBackRangedAttribute(i, refine, minAppearance);
            }
        }

        /// <summary>
        /// adds an additional dataset to the library
        /// </summary>
        /// <param name="data"></param>
        public void addDataset(string[] data)
        {
            Story s = new Story("", data, EArffTypes.DATA);
            this.Datasets.Add(s);
        }

        /// <summary>
        /// changing a dataset means, that one has to refer it, unique lineNumbers are not yet implemented
        /// </summary>
        /// <param name="lineNr"></param>
        /// <param name="data"></param>
        public void changeDataset(int lineNr, string[] data)
        {
            throw new CekaException("not implemented.");
        }

        /// <summary>
        /// deleting a dataset means, that one has to refer it, unique lineNumbers are not yet implemented
        /// </summary>
        /// <param name="lineNr"></param>
        public void removeDataset(int lineNr)
        {
            throw new CekaException("not implemented.");
        }

        /// <summary>
        /// adding an attribute to an existing arffinstance(file) means that all dataset stories have to be updated as well
        /// </summary>
        /// <param name="name"></param>
        /// <param name="span"></param>
        public void addAttributeWithRecursiveDatsetChanges(string name, string[] span)
        {
            throw new CekaException("not implemented.");
        }

        /// <summary>
        /// changing an attribute of an existing arffinstance(file) means that all dataset stories have to be updated as well
        /// </summary>
        /// <param name="name"></param>
        /// <param name="span"></param>
        public void changeAttributeWithRecursiveDatasetChanges(string name, string[] span)
        {
            throw new CekaException("not implemented.");
        }

        /// <summary>
        /// removing an attribute of an existing arffinstance(file) means that all dataset stories have to be updated as well
        /// </summary>
        /// <param name="name"></param>
        public void removeAttributeWithRecursiveDatasetChanges(string name)
        {
            throw new CekaException("not implemented.");
        }

        /// <summary>
        /// generates a copy of this instance
        /// </summary>
        /// <returns></returns>
        public ArffInstance toCopy()
        {
            //return Weka.ObjectCopier.Clone<ArffInstance>(this);

            //ArffInstance ni = new ArffInstance(this.Relation);
            //ni.Headers = this.Headers.toCopy();
            //ni.Datasets = this.Datasets.toCopy();
            //return ni;

            //serialising and deserialising the object to ensure a deep memory copy
            return JsonConvert.DeserializeObject<ArffInstance>(JsonConvert.SerializeObject(this));
        }

        /// <summary>
        /// returns the in-memory byte size of the this arffinstance
        /// </summary>
        /// <returns></returns>
        public int GetMemorySize()
        {
            return (int)(Helpers.Utils.GetObjectMemoryUsage(this) / 1024);
        }

    }
}