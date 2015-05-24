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

namespace DM.Ceka.Helpers
{
    /// <summary>
    /// this class is used to build ranges from numeric @attribute's data instances
    /// it turns something like: @attribute example { 122, 133, 144, 155, 210, 220, 230 }
    /// into this: @attribute example { [120-160], [210-230] }
    /// you dont have to call it from here, you can just call "ArffInstance.rebuildAttributeValueByRange(..)"
    /// </summary>
    public class RangeBuilder
    {
        /// <summary>
        /// [
        /// </summary>
        public const char ENOPEN = '[';

        /// <summary>
        /// /<->/
        /// </summary>
        public const string DELIM = "<->";

        /// <summary>
        /// ]
        /// </summary>
        public const char ENCLOSE = ']';

        /// <summary>
        /// lower boundary of the ranges
        /// </summary>
        public int lowerBound { set; get; }

        /// <summary>
        /// upper boundary of the ranges
        /// </summary>
        public int upperBound { set; get; }

        /// <summary>
        /// stringbuilde used to build the range
        /// </summary>
        private StringBuilder sb;

        /// <summary>
        /// single constructor
        /// </summary>
        /// <param name="l">lower range boundary</param>
        /// <param name="u">upper range boundary</param>
        public RangeBuilder(int l, int u)
        {
            this.lowerBound = l;
            this.upperBound = u;
            sb = new StringBuilder();
        }

        /// <summary>
        /// checks if the value is in bounds of this range
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool InRange(int value)
        {
            if (value <= upperBound && value >= lowerBound)
                return true;
            else
                return false;
        }

        /// <summary>
        /// returns a representative string of this range
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            sb.Clear();
            sb.Append(ENOPEN);
            sb.Append(lowerBound);
            sb.Append(DELIM);
            sb.Append(upperBound);
            sb.Append(ENCLOSE);
            
            return sb.ToString();
        }

        /// <summary>
        /// finds and returns the representative range string in a range collection from a value
        /// </summary>
        /// <param name="r"></param>
        /// <param name="value"></param>
        /// <param name="rangeSplit"></param>
        /// <returns></returns>
        public static string ValueOf(List<RangeBuilder> r, int value,  int rangeSplit = 0)
        {
            if(value < rangeSplit) //performance
                for (int i = 0; i < r.Count; i++)
                {
                    if (r[i].InRange(value))
                        return r[i].ToString();
                }
            else
                for (int i = r.Count - 1; i >= 0; i--)
                {
                    if (r[i].InRange(value))
                        return r[i].ToString();
                }

            throw new CekaException(string.Format("Couldnt find range for {0} in rangebuilder collection {1}..", value, r.Count));
        }

        /// <summary>
        /// builds a range collection from two bounds and steps in between
        /// </summary>
        /// <param name="lowBound"></param>
        /// <param name="highBound"></param>
        /// <param name="rangeSteps"></param>
        /// <returns></returns>
        public static List<RangeBuilder> BuildRange(int lowBound, int highBound, int rangeSteps)
        {
            bool lowNegative = false;
            if (lowBound < 0)
            {
                lowBound *= -1;
                lowNegative = true;
            }

            if ((lowBound / rangeSteps) > (2 * rangeSteps))
                throw new CekaException("You should reconsider your rangeStep choice! " + (lowBound / rangeSteps) + " ranges yet alone from lowerBounds is too much!");

            if ((highBound / rangeSteps + rangeSteps) > (2 * rangeSteps)) // +rangeSteps because we add it below via +(i * 1)
                throw new CekaException("You should reconsider your rangeStep choice! " + (highBound / rangeSteps) + " ranges yet alone from upperBounds is too much!");

            List<RangeBuilder> r = new List<RangeBuilder>();

            int l = 0;
            int u = 0;
            RangeBuilder rb = null;
            if(lowNegative)
                lowBound *= - 1;
            int xv = lowBound;
            int circle = 0;
            while (xv < highBound)
            {
                l = xv + circle;
                xv += rangeSteps;
                u = xv + circle;

                rb = new Helpers.RangeBuilder(l, u);
                r.Add(rb);
                circle++;
            }

            Helpers.Utils.Debug(string.Format("Built {0} ranges!", r.Count));

            return r;
        }

        /// <summary>
        /// turns range collection into string array containing all ranges (used for arff.attribute.values exchange)
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static string[] ConvertToSimpleRanges(List<RangeBuilder> r)
        {
            if (r == null || r.Count <= 0)
                throw new CekaException("The range collection is empty!");

            string[] sa = new string[r.Count];
            for (int i = 0; i < r.Count; i++)
                sa[i] = r[i].ToString();

            return sa;
        }
    }
}