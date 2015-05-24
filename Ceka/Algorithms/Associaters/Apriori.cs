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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DM.Ceka.Algorithms.Associaters
{
    /// <summary>
    /// single threaded approach of building a fast C# Apriori on behalf of an Ceka.ArffInstance
    /// </summary>
    public sealed class Apriori : Associater
    {
        /// <summary>
        /// helps debugging in multithreaded usage
        /// </summary>
        static uint APRIORI_COUNT = 0;

        /// <summary>
        /// is set in constructor from "APRIORI_COUNT", helps in multithreaded usage
        /// </summary>
        private uint algorithm_id = 0;

        /// <summary>
        /// to measure exact performance results
        /// </summary>
        private Stopwatch sw = new Stopwatch();

        /// <summary>
        /// the ArffInstance is passed in the constructor
        /// </summary>
        private ArffInstance source;

        /// <summary>
        /// object to generate unique* uint hashes from strings
        /// </summary>
        private Common.MurmurHash2Unsafe hash;

        /// <summary>
        /// the amount of total datasets "@data"
        /// </summary>
        private int dataset_count = 0;

        /// <summary>
        /// the amount of attributes
        /// </summary>
        private int dataset_attribute_count = 0;

        /// <summary>
        /// the amount of total distinct items
        /// </summary>
        private int different_item_count = 0;

        /// <summary>
        /// is like "different_item_count" but used during the 2nd cycle as they build pairs
        /// </summary>
        private int different_pair_count = 0;

        /// <summary>
        /// itemhashs in 2D -> [row][column] = itemhash
        /// </summary>
        private uint[,] rep_dataset;

        /// <summary>
        /// count of total items -> [i][0] = itemhash, [i][1] = itemcount
        /// </summary>
        private uint[][] rep_items;

        /// <summary>
        /// second level of total items ->[i][0] = itemhash_1, [i][1] = itemhash_2, [i][2] = paircount
        /// </summary>
        private uint[][] rep_items_2nd;

        /// <summary>
        /// second level of total items with confidence ->[i][0] = itemhash_1, [i][1] = itemhash_2, [i][2] = paircount, [i][3] = confidence / 10 = %
        /// </summary>
        private uint[][] rep_items_2nd_conf;

        /// <summary>
        /// hold every next level N after 2 with pairs inclunding count and confidence
        /// -> [i][n - 3] = itemhash_1, [i][n - 2] = itemhash_2, .. [i][n-1] = itemhash_2 .. [i][n] = paircount, [i][n + 1] = confidence / 10 = %
        /// </summary>
        private uint[][] rep_items_n_conf;

        /// <summary>
        /// to calculate the confidence of the next cycle the count from the old cycle_result is required, its faster and cheaper to just store the rep_cycle_n->results
        /// </summary>
        private List<uint[][]> rep_n_list;

        /// <summary>
        /// minimum support for the found patterns
        /// </summary>
        private float threshold_min_support = 0.1f; // 10%

        /// <summary>
        /// confidence required for the found patterns
        /// </summary>
        private float threshold_confidence = 0.5f; // 80%

        /// <summary>
        /// a faster way to get the original string value from the inthashes
        /// </summary>
        private Dictionary<uint, string> inthashReturn;

        /// <summary>
        /// making the internal "(int)cycle" accessable, dont rely on this.. its set in "do_n_cycles_routine"
        /// </summary>
        private int cycles_made = -1;

        /// <summary>
        /// total execution time of "run_default_process"
        /// </summary>
        private long total_runtime = 0;

        /// <summary>
        /// just for result purpose
        /// </summary>
        private bool filterForSupport = true;

        /// <summary>
        /// just for result purpose
        /// </summary>
        private bool filterForConfidence = true;

        /// <summary>
        /// single constructor, executes the total algorithm using default thresholds
        /// </summary>
        /// <param name="ai"></param>
        public Apriori(ArffInstance ai) : base()
        { 
            this.algorithm_id = Apriori.APRIORI_COUNT;
            Apriori.APRIORI_COUNT++;
            
            source = ai;

            dataset_count = this.source.Datasets.Data.Count;
            dataset_attribute_count = this.source.Datasets.Data[0].Length();

            hash = new Common.MurmurHash2Unsafe();

            this.rep_n_list = new List<uint[][]>();
        }

        /// <summary>
        /// traditional apriori constructor, enabling configuration through confidence and support
        /// </summary>
        /// <param name="ai"></param>
        /// <param name="support"></param>
        /// <param name="confidence"></param>
        public Apriori(ArffInstance ai, float support, float confidence)
            : this(ai)
        {
            this.threshold_confidence = confidence;
            this.threshold_min_support = support;
        }

        /// <summary>
        /// traditional apriori constructor that writes the result straight to a json file
        /// </summary>
        /// <param name="ai"></param>
        /// <param name="support"></param>
        /// <param name="confidence"></param>
        /// <param name="file"></param>
        public Apriori(ArffInstance ai, float support, float confidence, string file)
            : this(ai, support, confidence)
        {
            this.run_default_process();
            new Ceka.Saver.SimpleJsonSaver(this.get_aobj_result(), true).SaveToFile(file + ".json");
        }

        /// <summary>
        /// traditional apriori constructor that writes the json result to a file but leaves the possibility to disable filters
        /// </summary>
        /// <param name="ai"></param>
        /// <param name="support"></param>
        /// <param name="confidence"></param>
        /// <param name="filterSupport"></param>
        /// <param name="filterConfidence"></param>
        /// <param name="file"></param>
        /// <param name="prettyJson"></param>
        public Apriori(ArffInstance ai, float support, float confidence, bool filterSupport, bool filterConfidence, string file, bool prettyJson)
            : this(ai, support, confidence)
        {
            this.filterForSupport = filterSupport;
            this.filterForConfidence = filterConfidence;

            this.run_default_process(filterSupport, filterConfidence);
            new Ceka.Saver.SimpleJsonSaver(this.get_aobj_result(), prettyJson).SaveToFile(file + ".json");
        }

        /// <summary>
        /// similar to the traditional constructor with filter options, but leaves options for different result saving
        /// </summary>
        /// <param name="ai"></param>
        /// <param name="support"></param>
        /// <param name="confidence"></param>
        /// <param name="filterSupport"></param>
        /// <param name="filterConfidence"></param>
        /// <param name="savt"></param>
        public Apriori(ArffInstance ai, float support, float confidence, bool filterSupport, bool filterConfidence, Saver.AprioriSaveTypes savt, bool cli = false, string outputFile = null) 
            : this(ai, support, confidence)
        {
            this.filterForSupport = filterSupport;
            this.filterForConfidence = filterConfidence;

            this.run_default_process(filterSupport, filterConfidence);

            string file = this.source.Relation + "_result";
            if (outputFile != null)
                file = outputFile;

            switch (savt)
            {
                case Saver.AprioriSaveTypes.JSON:
                    if (!cli)
                        new Saver.SimpleJsonSaver(this.get_aobj_result(), false).SaveToFile(file + ".json");
                    else
                        new Saver.SimpleJsonSaver(this.get_aobj_result(), false).CLI();
                    break;

                case Saver.AprioriSaveTypes.JSON_PRETTY:
                    if (!cli)
                        new Saver.SimpleJsonSaver(this.get_aobj_result(), true).SaveToFile(file + ".json");
                    else
                        new Saver.SimpleJsonSaver(this.get_aobj_result(), true).CLI();
                    break;

                case Saver.AprioriSaveTypes.WEKA:
                    if (!cli)
                        new Saver.WekaAssociationRulesSaver(this.get_aobj_result()).SaveToFile(file + ".ceka");
                    else
                        new Saver.WekaAssociationRulesSaver(this.get_aobj_result()).CLI();
                    break;

                case Saver.AprioriSaveTypes.NONE:
                    log("Apriori finished, doing nothing as SaveType is NONE.");
                    break;

                default:
                    log("Apriori finished, but SaveType is DEFAULT: " + savt.ToString());
                    break;
            }
        }

        /// <summary>
        /// simple public function, for basic constructor instances that do not execute "run_default_process"
        /// </summary>
        public void Run()
        {
            this.run_default_process();
        }

        /// <summary>
        /// runs the default apriori algorithm for n cycles after first and second cycle and filters for support and confidence
        /// </summary>
        private void run_default_process(bool filterSupport = true, bool filterConfidence = true)
        {
            Stopwatch swi = new Stopwatch();
            log(string.Format("Running Apriori on {0} Datasets and {1} Attributes of @{2}.", 
                dataset_count, dataset_attribute_count, this.source.Relation));
            swi.Start();

            turn_into_representative_ints(); //prepare dataset
            get_representative_header_items(); //prepare total items
            if (check_for_representative_anomalic_collisions())
            { //collision can occure in the murmur2 function, that would result in bad algo. results
                log("WARNING: Collisions found in int-hashes, this will result in bad algorithm result-sets!");
                throw new CekaException(
                    "MurmurHash2 generates collisions in the inthash-maps, please change the item set string values!");
            }

            /* splitted even further, for the most opened analysis */
            this.do_first_cycle_preparation(filterSupport);
            this.do_second_cycle_pair_building(filterSupport, filterConfidence);
            this.do_n_cycles_routine(filterSupport, filterConfidence);

            total_runtime = swi.ElapsedMilliseconds;
            log(string.Format("Finished Apriori after {0} ms.", total_runtime));
        }

        /// <summary>
        /// calls all necessary functions for the first cycle step of the algorithm
        /// </summary>
        private void do_first_cycle_preparation(bool filterSupport = true)
        {
            //first cycle: preparation
            calculate_simple_amount_of_items();

            if(filterSupport)
                filter_simple_amount_of_items_for_min_support();
        }

        /// <summary>
        /// calls all necessary functions for the second cycle step of the algorithm (first real cycle)
        /// you cant call this unless "do_first_cycle_preparation" already ran
        /// </summary>
        private void do_second_cycle_pair_building(bool filterSupport = true, bool filterConfidence = true)
        {
            //second cycle: first pair building and evaluation
            build_pair_list_for_second_cycle();
            run_second_cycle();

            if(filterSupport)
                filter_second_cycle_items();

            calculate_confidence_of_second_cycle_result();

            if(filterConfidence)
                filter_second_cycle_result_for_confidence();
        }

        /// <summary>
        /// calls all necessary functions for the "N" cycle step of the algorithm (cycle routine)
        /// you cant call this unless "do_first_cycle_preparation" and "do_second_cycle_pair_building" already ran
        /// </summary>
        private void do_n_cycles_routine(bool filterSupport = true, bool filterConfidence = true)
        {
            int cycle = 2; //start after 2nd cycle

            //n cycles: advanced pair building and evaluation routine
            while (cycle <= dataset_attribute_count) //we cant get more rule pairs than attributes
            {
                cycle++; //hop to the next cycle
                if (build_pair_list_for_n_cycle(cycle) <= 0)
                    break; //and we dont need to waste runtime if we cant find any more pairs

                run_n_cycle(cycle);

                if(filterSupport)
                    filter_n_cycle_items(cycle);

                calculate_confidence_of_n_cycle_result(cycle);

                if(filterConfidence)
                    filter_n_cycle_result_for_confidence(cycle);
            }

            this.cycles_made = cycle;

            /* its always the same routine:
             * 1.) build pairs from item(pair) combinations that are still left (different last item)
             * 2.) run the cycle and count the occurences of the new pairs in the dataset
             * optional:
             * o.) filter the occurences of the pairs for the minimum support given by user
             * o.) calculate the confidence for the items using the last itemset
             * o.) filter the confidences of the pairs for the confidence given by user
             * 
             * 3.) repeat
             */
        }

        /// <summary>
        /// runs through pre-calculated item pair confidences and filters the item pairs for the given confidence
        /// </summary>
        /// <param name="n"></param>
        private void filter_n_cycle_result_for_confidence(int n)
        {
            log(string.Format("Filtering for confidence on cycle {0}..", n));
            sw.Start();

            List<uint[]> filter = new List<uint[]>();

            uint arrangedConfidence = (uint)((double)threshold_confidence * 1000.0);

            for (int i = 0; i < different_pair_count; i++)
            {
                if (rep_items_n_conf[i][n + 1] >= arrangedConfidence)
                    filter.Add(rep_items_n_conf[i]);
            }

            this.rep_items_n_conf = filter.ToArray<uint[]>();
            this.different_pair_count = rep_items_n_conf.Length;

            log(string.Format("Filtering done, took {0} ticks for Confidence of {1}% -> {2} -> leaving: {3}",
                sw.ElapsedTicks, (threshold_confidence * 100), arrangedConfidence, different_pair_count));
            sw.Reset();
        }

        /// <summary>
        /// uses the cycle_result that was stored before, to calculate the confidence of the current cycle result pairs
        /// </summary>
        /// <param name="n"></param>
        private void calculate_confidence_of_n_cycle_result(int n)
        {
            log(string.Format("Calculating confidence on cycle {0}..", n));
            sw.Start();

            double tConf = 0.0;
            bool similar = false;

            /* Confidence=(count A => B / count A)  (* 1000 to store in an accurate uint) */

            for (int i = 0; i < different_pair_count; i++)
            {
                for (int ic = 0; ic < this.rep_n_list[this.rep_n_list.Count - 1].Length; ic++)
                { //run through the last stored item pair set 
                    similar = true;
                    for (int l = 0; l < n - 1; l++)
                    { //run through the single items in the pairs of the stored and current item pair list (excluding count and confidence)
                        if (rep_items_n_conf[i][l] != this.rep_n_list[this.rep_n_list.Count - 1][ic][l])
                            similar = false;
                    }

                    if (similar) //if the first 0 -> n - 1 int hashes (item elements) were the same, the pair is the same
                    {
                        tConf = ((double)rep_items_n_conf[i][n] /* pair count A => B => N => N */
                            / (double)rep_n_list[this.rep_n_list.Count - 1][ic][n - 1] /* item count of A => B => N */
                                * 1000.0 /* 1000 to make sure we dont loose accuraccy in the uint */ );

                        rep_items_n_conf[i][n + 1] = (uint)Math.Round(tConf);
                        break;
                    }
                }
            }

            log(string.Format("Calculation done, took {0} ticks.", sw.ElapsedTicks));
            sw.Reset();
        }

        /// <summary>
        /// filters the item pairs for the given minimum support
        /// </summary>
        /// <param name="n"></param>
        private void filter_n_cycle_items(int n)
        {
            log(string.Format("Filtering for minimum support on cycle {0}..", n));
            sw.Start();

            List<uint[]> filteredList = new List<uint[]>();

            //dataset_count adapted in "run_n_cycle"!!!
            uint totalSupportage = (uint)Math.Round((double)dataset_count * (double)threshold_min_support);

            /* I am just returning the calculation of the usual Support=(count A => B / transactions) way,
             * because this way I only have to calculate the support once and compare that to the count (transaction)
             * that is already stored with the items */

            for (int i = 0; i < different_pair_count; i++)
            {
                if (rep_items_n_conf[i][n] >= totalSupportage)
                    filteredList.Add(rep_items_n_conf[i]);
            }

            rep_items_n_conf = filteredList.ToArray<uint[]>();
            this.different_pair_count = this.rep_items_n_conf.Length;

            log(string.Format("Filtering done, took {0} ticks for Support of {1}% -> {2}/{3} -> leaving: {4}",
                sw.ElapsedTicks, (threshold_min_support * 100), totalSupportage, dataset_count,
                different_pair_count));
            sw.Reset();
        }

        /// <summary>
        /// runs a cycle "N", make sure to run "build_pair_list_for_n_cycle" before -> counts the possible pairs in the dataset
        /// </summary>
        /// <param name="n">number of cycles</param>
        private void run_n_cycle(int n)
        {
            log(string.Format("Running next cycle {0}..", n));
            sw.Start();

            List<uint> tempIndexes = new List<uint>();
            int colCount = 1;
           
            for (int i = 0; i < rep_items_n_conf.Length; i++) //for every pair combination -> P
            {
                for (int r = 0; r < dataset_count; r++) //run through all datasets -> D
                {
                    colCount = 1;

                    for (int c = 0; c < dataset_attribute_count; c++) //and through all items -> I
                    {
                       uint itemD = rep_dataset[r, c];
                       uint itemC = rep_items_n_conf[i][colCount - 1];

                       if (itemD == itemC)
                           colCount++;  
                    }

                    if (colCount == n)
                    {
                        rep_items_n_conf[i][n]++;

                        if (!tempIndexes.Contains((uint)r))
                            tempIndexes.Add((uint)r); //store the index of the row that contained a pair (faster check if index was added)
                    }
                }
            }

            //get rid of the datasets that do not contain any pairs
            uint[,] reDataset = new uint[tempIndexes.Count, dataset_attribute_count];
            for (int r = 0; r < tempIndexes.Count; r++)
                for (int c = 0; c < dataset_attribute_count; c++)
                    reDataset[r, c] = rep_dataset[r, c];

            //overwrite current dataset
            rep_dataset = reDataset;
            dataset_count = rep_dataset.Length / dataset_attribute_count;

            log(string.Format("Cycle run done, took {0} ms.",sw.ElapsedMilliseconds));
            sw.Reset();
        }

        /// <summary>
        /// probably the most important function for the advanced part of the algorithm, it finds possible item pair combinations
        /// that can be build from the current cycle's item pair set, it should also control the loop for the total upcoming generations of cycles
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private int build_pair_list_for_n_cycle(int n)
        {
            log(string.Format("Building pair list for cycle {0}..", n));
            sw.Start();

            if (n == 3) //we just ran the 2nd cycle
                rep_items_n_conf = rep_items_2nd_conf;

            //this should normally start off with n = 3
            //but the although the second cycle ran, the first ones does not count as it does not stack pairs up
            n--;

            /* we have to find item pairs, where just the last item differs, to combine them as
             * a new pair, with every cycle the pairs grow by a single element, meaning that on every
             * cycle the index of the last item increases but the amount of the combined item (pairs) that
             * have to be compared against all other combinations increases as well.. */

            List<uint[]> pairs = new List<uint[]>();
            uint[] pair = null;

            List<uint[]> newpairs = new List<uint[]>();
            uint[] newpair = null;

            for (uint i = 0; i < different_pair_count; i++) //run through all existing pairs
            {
                pair = new uint[n]; //(filled size is n - 1) but leave full n size so that [n-1] can be used for index

                for(int cn = 0; cn < n - 1; cn++){ //build pairs of the length until the last element index = size - 2
                    pair[cn] = rep_items_n_conf[i][cn];
                }

                pair[n - 1] = i; //store index
                uint[] indexes = get_indexes_of_first_pair_found(pairs, pair);
                
                pairs.Add(pair); //add it to the pairs, it doesnt matter if there is already an index in the pairs list or not, we need all possible combinations

                for (int ix = 0; ix < indexes.Length; ix++)
                {
                    //magic happens here

                    /*    we have a possible item pair
                        * with just the last index being different, a new pair for the this cycle can be
                        * build with just these items (pairs) */

                    newpair = new uint[(n + 1 + 2)]; //n + 1 = for the new item element, + 2 to leave space for count and confidence

                    for (int np = 0; np < n - 1; np++)
                    {
                        newpair[np] = pair[np]; //transfer the first item elements
                    }

                    newpair[n - 1] = rep_items_n_conf[indexes[ix]][n - 1]; //adds different element from the !!!first pair!!! that was found
                    newpair[n] = rep_items_n_conf[i][n - 1]; //adds the different element from the second pair that was found
                    newpair[n + 1] = 0; //count
                    newpair[n + 2] = 0; //confidence

                    //only add the new pair if we cant find same attributes in it
                    if(!check_new_pair_collisions(newpair, n))
                        newpairs.Add(newpair);
                }
            }

            //store the old item pair result, before overwriting it
            this.rep_n_list.Add(rep_items_n_conf);

            this.rep_items_n_conf = newpairs.ToArray<uint[]>();
            this.different_pair_count = rep_items_n_conf.Length;

            log(string.Format("Building pairs done, took {0} ms.", sw.ElapsedMilliseconds));
            sw.Reset();

            return different_pair_count; //when there are no more new pairs, this is going to be ZERO
        }

        /// <summary>
        /// checks all first attributes of items in a new itemsetpair and if one doubles, it returns false
        /// </summary>
        /// <param name="newpair"></param>
        /// <returns></returns>
        private bool check_new_pair_collisions(uint[] newpair, int n)
        {
            uint v = 0;
            List<uint> ah = new List<uint>();
            for (int i = 0; i < (n + 1); i++)
            {
                v = attribute_part_to_hash(newpair[i]);

                if (!ah.Contains(v))
                    ah.Add(v);
                else
                    return true; //collision
            }

            return false;
        }

        /// <summary>
        /// this function does not return the index of the in the pair list, it returns the index of the pair it collides with
        /// in the original list, this function is used by "build_pair_list_for_n_cycle"
        /// </summary>
        /// <param name="pairs"></param>
        /// <param name="pair"></param>
        /// <returns></returns>
        private uint[] get_indexes_of_first_pair_found(List<uint[]> pairs, uint[] pair)
        {
           /* there can be multiple index entries, so we cant just return the first value, that we find
            * in the orignal list, we have to return an array of values, so that all possible pairs
            * are build, otherwise we would drop of patterns that might me of importance */

            List<uint> indexList = new List<uint>();
            bool similar = false;

            for (int p = 0; p < pairs.Count; p++) //run through all pairs
            {
                similar = true;
                for (int i = 0; i < pairs[p].Length - 1; i++) //each pair in "pairs", must have same length as "pair" -> but the last element is the index => so: length - 1
                {
                    if (pairs[p][i] != pair[i])
                    {
                        similar = false; //if one of the elements is not the same, its just not the same pair
                        break;
                    }
                }
                if (similar) //if the pair was similiar, store the original index in the return list
                    indexList.Add(pairs[p][pairs[p].Length - 1]);
            }

            return indexList.ToArray<uint>();
        }

        /// <summary>
        /// clears the result list of the 2nd cycle with confidence "rep_items_2_conf" on behalf of the defined threshold confidence
        /// </summary>
        private void filter_second_cycle_result_for_confidence()
        {
            log("Filtering second cycle result for confidence..");
            sw.Start();

            List<uint[]> filter = new List<uint[]>();

            uint arrangedConfidence = (uint)((double)threshold_confidence * 1000.0);

            for (int i = 0; i < different_pair_count; i++)
            {
                if (rep_items_2nd_conf[i][3] >= arrangedConfidence)
                    filter.Add(rep_items_2nd_conf[i]);
            }

            this.rep_items_2nd_conf = filter.ToArray<uint[]>();
            this.different_pair_count = rep_items_2nd_conf.Length;

            log(string.Format("Filtering done, took {0} ticks for Confidence of {1}% -> {2} -> leaving: {3}",
                sw.ElapsedTicks, (threshold_confidence * 100), arrangedConfidence, different_pair_count));
            sw.Reset();
        }

        /// <summary>
        /// adds another element to the "rep_items_2nd" array and calculates the confidence of each pair
        /// </summary>
        private void calculate_confidence_of_second_cycle_result()
        {
            log("Calculating confidence of second cycle..");
            sw.Start();

            uint[][] incCr = new uint[different_pair_count][]; //[4]
            double tConf = 0.0;

            /* Confidence=(count A => B / count A)  (* 1000 to store in an accurate uint) */

            for (int i = 0; i < different_pair_count; i++)
            {
                //0-2 stay the same, as we just add the confidence
                incCr[i] = new uint[4];
                incCr[i][0] = rep_items_2nd[i][0];
                incCr[i][1] = rep_items_2nd[i][1];
                incCr[i][2] = rep_items_2nd[i][2];

                for(int ic = 0; ic < different_item_count; ic++)
                    if (rep_items[ic][0] == rep_items_2nd[ic][0])
                    {
                        tConf = ((double)rep_items_2nd[i][2] /* pair count A => B */ 
                            / (double)rep_items[ic][1] /* item count of A */ 
                                * 1000.0 /* 1000 to make sure we dont loose accuraccy in the uint */ );
                        incCr[i][3] = (uint)Math.Round(tConf);
                        break;
                    }
            }

            this.rep_items_2nd_conf = incCr;

            log(string.Format("Calculation done, took {0} ticks.", sw.ElapsedTicks));
            sw.Reset();
        }

        /// <summary>
        /// runs through the total itemlist "rep_items_2d" and kicks out pairs that are not in the minimum support range
        /// </summary>
        private void filter_second_cycle_items()
        {
            log("Cleaning second cycle itemset..");
            sw.Start();

            List<uint[]> filteredList = new List<uint[]>();

            //dataset_count adapted in "run_second_cycle"
            uint totalSupportage = (uint)Math.Round((double)dataset_count * (double)threshold_min_support);

            /* I am just returning the calculation of the usual Support=(count A => B / transactions) way,
             * because this way I only have to calculate the support once and compare that to the count (transaction)
             * that is already stored with the items */

            for (int i = 0; i < different_pair_count; i++)
            {
                if (rep_items_2nd[i][2] >= totalSupportage)
                    filteredList.Add(rep_items_2nd[i]);
            }

            rep_items_2nd = filteredList.ToArray<uint[]>();
            this.different_pair_count = this.rep_items_2nd.Length;

            log(string.Format("Filtering done, took {0} ticks for Support of {1}% -> {2}/{3} -> leaving: {4}",
                sw.ElapsedTicks, (threshold_min_support * 100), totalSupportage, dataset_count,
                different_pair_count));
            sw.Reset();
        }

        /// <summary>
        /// second cycle runs after "calculate_simple_amount_of_items()" and builds the first level pattern
        /// </summary>
        private void run_second_cycle()
        {
            log("Running second cycle..");
            sw.Start();

            List<uint> tempIndexes = new List<uint>();

            bool hi1 = false, hi2 = false;
            for(int i = 0; i < rep_items_2nd.Length; i++){ //run through all pairs
                for (int r = 0; r < dataset_count; r++) //all datasets rows
                {
                    hi1 = false; hi2 = false;

                    for (int c = 0; c < dataset_attribute_count; c++) //all dataset columns
                    {
                        if (rep_dataset[r, c] == rep_items_2nd[i][0])
                            hi1 = true;

                        if (rep_dataset[r, c] == rep_items_2nd[i][1])
                            hi2 = true;
                    }

                    if (hi1 && hi2) //if pair was present in this row increase its count
                    {
                        rep_items_2nd[i][2]++;

                        if (!tempIndexes.Contains((uint)r))
                            tempIndexes.Add((uint)r); //store the index of the row that contained a pair (faster check if index was added)
                    }
                }
            }

            //get rid of the datasets that do not contain any pairs
            uint[,] reDataset = new uint[tempIndexes.Count, dataset_attribute_count];
            for (int r = 0; r < tempIndexes.Count; r++)
                for (int c = 0; c < dataset_attribute_count; c++)
                    reDataset[r, c] = rep_dataset[r, c];

            //overwrite current dataset
            rep_dataset = reDataset;
            dataset_count = rep_dataset.Length / dataset_attribute_count;

            log(string.Format("Second cycle done, took {0} ms.", sw.ElapsedMilliseconds));
            sw.Reset();
        }

        /// <summary>
        /// builds a list of int[][3] including 
        /// a combination of all itemhash, itemhash pairs and a count in the dataset
        /// </summary>
        private void build_pair_list_for_second_cycle()
        {
            log("Building patterns for the second cycle..");
            sw.Start();

            List<uint[]> pairs = new List<uint[]>();
            bool pair_exists = false;

            for (int i = 0; i < different_item_count; i++)
            {
                for (int x = 0; x < different_item_count; x++)
                {
                    //make sure there are no itemsets build from the same attributes
                   if (attribute_part_to_hash(rep_items[i][0]) == attribute_part_to_hash(rep_items[x][0]))
                       continue;

                    pair_exists = false;
                    /* start on the first item, add every other to it, and all other combinations, but make sure none of them are equal not AB and BA likewise */
                    foreach (uint[] p in pairs)
                    {
                        if ((p[0] == rep_items[i][0] && p[1] == rep_items[x][0]) || (p[0] == rep_items[x][0] && p[1] == rep_items[i][0]) )
                        {
                            //in whatever order this item does already exist
                            pair_exists = true;
                            break;
                        }
                    }

                    if (pair_exists)
                        continue;
                    
                    pairs.Add(new uint[] { rep_items[i][0], rep_items[x][0], 0 });
                }
            }

            this.rep_items_2nd = pairs.ToArray<uint[]>();
            this.different_pair_count = rep_items_2nd.Length;

            log(string.Format("Done building patterns, took {0} ticks.", sw.ElapsedTicks));
            sw.Restart();
        }

        /// <summary>
        /// this is as fast as it gets, tried every possibility (except unsafe code) to get an int representative of just the first attribute part of the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private uint attribute_part_to_hash(uint item)
        {
            /*
            string str = inthashReturn[item];
            int ind = str.IndexOf('=');
            char[] ca = new char[ind + 1];

            for (int i = 0; i < ind + 1; i++)
            {
                ca[i] = str[i];
            }

            return get_representative_int_hash(new string(ca));
            */

            string str = inthashReturn[item];
            int ind = str.IndexOf('=');
            return get_representative_int_hash(str.Substring(0, ind + 1));
            
            /*
            string s = inthashReturn[item];
            StringBuilder b = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
                if (s[i] != '=')
                    b.Append(s[i]);
                else
                    break;

            return get_representative_int_hash(b.ToString());
            */
        }

        /// <summary>
        /// runs through every item in the dataset row by column, using the function "increase_rep_item_count_by_inthash()" to find and count the item in "rep_items"
        /// </summary>
        private void calculate_simple_amount_of_items()
        {
            log("Calculating amount of items, simple manner..");
            sw.Start();

            for (int r = 0; r < dataset_count; r++)
            {
                for (int c = 0; c < dataset_attribute_count; c++)
                {
                    increase_rep_item_count_by_inthash(rep_dataset[r, c]);
                }
            }

            log(string.Format("Calculation done, took {0} ticks.", sw.ElapsedTicks));
            sw.Reset();
        }

        /// <summary>
        /// used by "calculate_simple_amount_of_items" to find the inthash value in "rep_items" and increase it
        /// </summary>
        /// <param name="inthash"></param>
        private void increase_rep_item_count_by_inthash(uint inthash)
        {
            for (int i = 0; i < different_item_count; i++)
            {
                if (rep_items[i][0] == inthash)
                {
                    rep_items[i][1]++;
                    break;
                }
            }
        }

        /// <summary>
        /// runs through all "rep_items" and checks if a hashvalue exsits more then once
        /// </summary>
        /// <returns></returns>
        private bool check_for_representative_anomalic_collisions()
        {
            bool status = false;
            log("Checking for anomalic collisions in the generated int[][0]..");
            sw.Start();

            for (int i = 0; i < different_item_count; i++)
            {
                int item_count = 0;
                for (int j = 0; j < different_item_count; j++)
                {
                    if (rep_items[i][0] == rep_items[j][0] && i != j)
                    {
                        item_count++;
                        log(string.Format("Collision on {0} and {1}, index values: {2} and {3}!", rep_items[i][0], rep_items[j][0], i, j));
                    }
                }
                if (item_count != 0)
                    status = true;
            }

            log(string.Format("Check done, took {0} ticks, STATUS: {1}!", sw.ElapsedTicks, status));
            sw.Reset();

            return status;
        }

        /// <summary>
        /// builds the "rep_items" property using the ArffInstance's Headers and also sets the different_items_count
        /// </summary>
        private void get_representative_header_items()
        {
            log("Building header int[][]..");
            sw.Start();

            this.inthashReturn = new Dictionary<uint, string>(); //easier return

            List<uint[]> tempItems = new List<uint[]>(); //the total amount of items is unknown
            List<string> tempList = new List<string>(); //ensure string uniqueness

            for (int r = 0; r < dataset_attribute_count; r++)
            {
                for (int c = 0; c < source.Headers.Data[r].Length(); c++) //this can be different! -> .Data[r]
                {
                    //cant be sure if the inthash is unique, thats why a temp string list with the orignal value is used to check if the item is already stored
                    if(!tempList.Contains((source.Headers.Data[r][c]))){
                        //tempList.Add(source.Headers.Data[r][c]); v1
                        tempList.Add(this.source.Headers.Data[r].Name + '=' + source.Headers.Data[r][c]);
                        //uint hash = this.get_representative_int_hash(source.Headers.Data[r][c]); v1
                        uint hash = this.get_representative_int_hash(this.source.Headers.Data[r].Name + '=' + source.Headers.Data[r][c]);
                        tempItems.Add(new uint[2] { hash, 0 }); //startcount is 0 ofc 

                        //inthashReturn.Add(hash, source.Headers.Data[r][c]); v1
                        inthashReturn.Add(hash, this.source.Headers.Data[r].Name + '=' + source.Headers.Data[r][c]);
                    }
                }
            }

            this.different_item_count = tempItems.Count;
            this.rep_items = tempItems.ToArray<uint[]>();

            log(string.Format("Build done, took {0} ticks, Different Items: {1}.", sw.ElapsedTicks, different_item_count));
            sw.Reset();
        }

        /// <summary>
        /// builds the int[,] containing the representative ArffInstance Datasets but with a 60x faster data access
        /// </summary>
        private void turn_into_representative_ints()
        {
            log("Building int[,] of representative dataset values..");
            sw.Start();
            this.rep_dataset = new uint[dataset_count, dataset_attribute_count];

            //rows
            for (int r = 0; r < dataset_count; r++)
            {
                //columns
                for (int c = 0; c < dataset_attribute_count; c++)
                {
                    //this.rep_dataset[r, c] = this.get_representative_int_hash(this.source.Datasets.Data[r][c]); v1
                    this.rep_dataset[r, c] = this.get_representative_int_hash(this.source.Headers.Data[c].Name + '=' + this.source.Datasets.Data[r][c]);
                }
            }

            log(string.Format("Build done, took {0} ticks for {1} dataset values.", sw.ElapsedTicks, (dataset_count * dataset_attribute_count)));
            sw.Reset();
        }

        /// <summary>
        /// function that uses the hash object to generate a unique* uint from a (dataset_column) string
        /// </summary>
        /// <param name="dataset_column"></param>
        /// <returns></returns>
        private uint get_representative_int_hash(string dataset_column)
        {
            return hash.Hash(Encoding.UTF8.GetBytes(dataset_column));
        }

        /// <summary>
        /// runs through the total itemlist "rep_items" and kicks out items that are not in the minimum support range
        /// </summary>
        private void filter_simple_amount_of_items_for_min_support()
        {
            log("Filtering for min support..");
            sw.Start();

            List<uint[]> filteredList = new List<uint[]>();

            uint totalSupportage = (uint)Math.Round((double)dataset_count * (double)threshold_min_support);

            for (int i = 0; i < different_item_count; i++)
            {
                if (rep_items[i][1] >= totalSupportage)
                    filteredList.Add(rep_items[i]);
            }

            this.rep_items = filteredList.ToArray<uint[]>();
            this.different_item_count = this.rep_items.Length;

            log(string.Format("Filtering done, took {0} ticks for Support of {1}% -> {2}/{3} -> leaving: {4}",
                sw.ElapsedTicks, (threshold_min_support * 100), totalSupportage, dataset_count,
                different_item_count));
            sw.Reset();
        }

        /// <summary>
        /// builds a key array of support (int[][i=last] element) and maps the indexes to the key array, 
        /// while using Array.Sort(k, v); to sort and return the cycle result array
        /// </summary>
        /// <param name="cr"></param>
        /// <param name="sort_index">the index of the columns that is used as sort value, default = 3 works for 2nd cycle with confidence</param>
        /// <returns></returns>
        private uint[][] sort_cycle_result(uint[][] cr, int sort_index = 3)
        {
            uint[] keys = new uint[cr.Length];
            int[] values = new int[cr.Length];

            for (int i = 0; i < cr.Length; i++)
            {
                //keys[i] = cr[i][cr[i].Length - 1]; //latest element of inner array is the pair count
                keys[i] = cr[i][sort_index]; //with this version, the function can run on any cycle_result
                values[i] = i; //store index of cycle result array as value
            }

            List<uint[]> sortedCr = new List<uint[]>();
            Array.Sort(keys, values);
            Array.Reverse(values); //sort is smallest to highest
            //sort key array and use the value as bound sorted index array for the cycle result array
            for (int i = 0; i < values.Length; i++)
            {
                sortedCr.Add(cr[values[i]]);
            }

            return sortedCr.ToArray<uint[]>();
        }

        /// <summary>
        /// 2nd cycle result without confidence uint[][3]
        /// sorts the input and builds an List of representative string values for the collected inthash 
        /// pairs and their other values.., note: data_size is also the index of the first countable value
        /// </summary>
        /// <param name="cr"></param>
        /// <param name="data_size">e.g. data_size = 2 if int[][0] and int[][1] are inthashes</param>
        private List<List<object>> get_flexible_cycle_result(uint[][] cr, int data_size, int sort_index = -1)
        {
            //data_size is the index of the count at the same time; array.length - 1 -> can be..
            if (sort_index == -1)
                sort_index = data_size;
            
            cr = this.sort_cycle_result(cr, sort_index);

            List<List<object>> fxcs = new List<List<object>>();
            List<object> tls = null;
            for (int r = 0; r < cr.Length; r++)
            {
                tls = new List<object>();
                for (int c = 0; c < cr[r].Length; c++)
                {
                    if (c < data_size)
                        tls.Add(inthashReturn[cr[r][c]]); //the pair inthash value as its original string representative
                    else
                        tls.Add(cr[r][c].ToString()); //the pair count or confidence, or whatever comes after is just added as it is
                }
                fxcs.Add(tls);
            }

            /* example usage:
             * you want the results from the second cycle without confidence and sorted after item count:
             * this.debug_cycle_result(rep_items_2nd, 2);
             * you want the results form the second cycle with confidence and sorted after item count:
             * this.debug_cycle_result(rep_items_2nd_conf, 2, 2);
             * you want the results from the second cycle with confidence and sorted after confidence:
             * this.debug_cycle_result(rep_items_2nd_conf, 2, 3);
             * you want the results from the third cycle with the confidence and sorted after confidence:
             * this.debug_cycle_result(rept_items_3nd_conf, 3, 3);
             * ..
             */

            return fxcs;
        }

        /// <summary>
        /// simple function that logs a string message into the console(default)
        /// </summary>
        /// <param name="msg"></param>
        private void log(string msg)
        {
                Helpers.Utils.Debug("[" + this.algorithm_id + "] " + msg);
        }

        /// <summary>
        /// generates a result object (its nested in this class) using "get_flexible_cycle_result" the result object can be printed via json
        /// </summary>
        /// <returns></returns>
        private NestedAprioriResult get_aobj_result()
        {
            log("Generating AprioriResultObject..");
            sw.Start();

            NestedAprioriResult nar = new NestedAprioriResult();

            if (cycles_made == -1)
                throw new CekaException("you did not set cycles_made or you did not run through any cycles!");

            //set simple values
            nar.Relation = this.source.Relation;
            nar.CyclesRan = this.cycles_made;
            nar.ValueableDatasets = this.dataset_count;
            nar.TotalDatasets = this.source.Datasets.Data.Count;
            nar.MinimumSupport = this.threshold_min_support;
            nar.Confidence = this.threshold_confidence;
            nar.TotalExecutionTime = this.total_runtime;
            nar.AlgorithmId = this.algorithm_id;
            nar.SupportApplied = this.filterForSupport;
            nar.ConfidenceApplied = this.filterForConfidence;

            //remap cycle results
            nar.ItemTable = get_flexible_cycle_result(rep_items, 1, 1);

            nar.CycleResults = new List<List<List<object>>>();

            int c = 2;
            foreach (uint[][] map in this.rep_n_list)
            {
                nar.CycleResults.Add(get_flexible_cycle_result(map, c, c + 1));
                c++;
            }

            //add attributes
            nar.Attributes = new List<string>();
            foreach (Story st in this.source.Headers.Data)
            {
                nar.Attributes.Add(st.Name);
            }

            log(string.Format("ResultObject generated, took {0} ms.", sw.ElapsedMilliseconds));
            sw.Reset();

            return nar;
        }

        /// <summary>
        /// a nested class used to generate a pretty and accessable json object as result output for the algorithm
        /// </summary>
        public sealed class NestedAprioriResult
        {
            /// <summary>
            /// empty constructor
            /// </summary>
            public NestedAprioriResult() { } //JSON Default Constructor

            /// <summary>
            /// stores "rep_items"
            /// </summary>
            public List<List<object>> ItemTable { set; get; }

            /// <summary>
            /// stores "rep_n_list"
            /// </summary>
            public List<List<List<object>>> CycleResults { set; get; }

            /// <summary>
            /// stores "cycles_made"
            /// </summary>
            public int CyclesRan { set; get; }

            /// <summary>
            /// stores "dataset_count"
            /// </summary>
            public int ValueableDatasets { set; get; }

            /// <summary>
            /// stores "source.Datasets.Data.Count;"
            /// </summary>
            public int TotalDatasets { set; get; }

            /// <summary>
            /// stores "threshold_min_support"
            /// </summary>
            public float MinimumSupport { set; get; }

            /// <summary>
            /// stores "threshold_confidence"
            /// </summary>
            public float Confidence { set; get; }

            /// <summary>
            /// stores "total_runtime"
            /// </summary>
            public long TotalExecutionTime { set; get; }

            /// <summary>
            /// stores "algorithm_id"
            /// </summary>
            public uint AlgorithmId { set; get; }

            /// <summary>
            /// relation of the dataset (source)
            /// </summary>
            public string Relation { set; get; }

            /// <summary>
            /// list of attributes present in the dataset's header
            /// </summary>
            public List<string> Attributes { set; get; }

            /// <summary>
            /// was minimum support used to filter itemsets?
            /// </summary>
            public bool SupportApplied { set; get; }

            /// <summary>
            /// was confidence used to filter itemsets?
            /// </summary>
            public bool ConfidenceApplied { set; get; }
        }

    }
}