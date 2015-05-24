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
using System.Diagnostics;

using DM.Ceka.Common;

namespace DM.Ceka.UHS
{
    /// <summary>
    /// containts functions to demonstrate uint hash performance
    /// </summary>
    public static class HashComparison
    {
        /// <summary>
        /// this function can estimate the amount of time that is likely saved due to uint hash comparison by the apriori algorithm on a specific dataset
        /// </summary>
        /// <param name="attributes">number of attributes in the dataset header; @attribute a1 {..}</param>
        /// <param name="attribute_values">median of attribute value count in the attributes; @attribute .. { val1, val2, val3, .. }</param>
        /// <param name="dataset_columns">@data row (instances) count of the dataset</param>
        /// <param name="percentage">percentage (5%-85%) of item-pair checks that might be avoided by the algorithm (speeds it up)</param>
        /// <returns>[0] = calculated comparisons, [1] = uint comparison time, [2] = string comparison time (times are milliseconds) </returns>
        public static long[] run_int_vs_string_comparison(int attributes = 10, int attribute_values = 5, int dataset_columns = 50000, int percentage = 30)
        {
            //calculate the number of comparisons from the scenario
            long compare_num = (long)((double)(((attributes * attribute_values) * (attributes * attribute_values) / 2) * dataset_columns * attributes) * ((double)percentage / 100));

            string _s1 = "association rules mining";
            string _s2 = "apriori data mining algorithm";
            string _s3 = "algorithm mining data apriori";
            MurmurHash2Simple hash = new MurmurHash2Simple();
            uint _i1 = get_representative_int_hash(hash, _s1);
            uint _i2 = get_representative_int_hash(hash, _s2);
            uint _i3 = get_representative_int_hash(hash, _s3);

            long uints = -1;
            long strings = -1;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            long do_something_1 = 0;
            long do_something_2 = 0;

            //comparing hashes
            for (long i = 0; i < compare_num; i++)
            {
                if (i % 2 == 0)
                {
                    if (_s1 == _s2) //doing the same thing will be optimized by the compiler..
                    {
                        do_something_1++;
                    }
                }
                else
                {
                    if (_s1 == _s3) //therefore another one here
                    {
                        do_something_2++;
                    }
                }
            }

            strings = sw.ElapsedMilliseconds;
            do_something_1 = 0;
            do_something_2 = 0;
            sw.Restart();

            //comparing strings
            for (long i = 0; i < compare_num; i++)
            {
                if (i % 2 == 0)
                {
                    if (_i1 == _i2) //doing the same thing will be optimized by the compiler..
                    {
                        do_something_1++;
                    }
                }
                else
                {
                    if (_i1 == _i3) //therefore another one here
                    {
                        do_something_2++;
                    }
                }
            }

            uints = sw.ElapsedMilliseconds;

            Helpers.Utils.Debug(string.Format("Compared {0} times; uints took {1} ms, strings took {2} ms.", compare_num, uints, strings));

            return new long[] { compare_num, uints, strings};
        }

        /// <summary>
        /// turns string into representative (murmur2) uint hash
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="dataset_column"></param>
        /// <returns></returns>
        public static uint get_representative_int_hash(MurmurHash2Simple hash, string dataset_column)
        {
            return hash.Hash(Encoding.UTF8.GetBytes(dataset_column));
        }
    }
}