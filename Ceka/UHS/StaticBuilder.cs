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

/* this class contains static functions that were used throughout the UHS project at SSU in 2014/2015.
 * it need not be included in the built libraries, therefore the define block was commented out,
 * each function represents a step in which an arff dataset was prepared or mined during the project's
   progress - it was left in the solution as it might be of help to someone. */

//uncomment this if you want to include it in your builds
//#define UHS

#if UHS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using DM.Ceka.Saver;
using DM.Ceka.Database;
using System.Threading;

namespace DM.Ceka.UHS
{
    public static class StaticBuilder
    {
        public static void GetDefaultDumpOfAliveAndDead()
        {
            Helpers.Utils.Debug("Dumping default alive and dead ARFF files..");
            Stopwatch sw = new Stopwatch(); sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("uhs_patient_story".ToUpper(),
                new string[]{
                        "INITIAL_PRESENTATION",
                            "INT_1_PRES",
                                "INT_2_PRES",
                                        "STATUS"
                                            }, -1, -1, false, true);

            long em = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Gathered MySQL ArffInstance and stored total file in {0} ms.", em));

            new ArffSaver(uhsAi).saveInstance("uhs_arff_total");

            ArffInstance uhsAi2 = uhsAi.toCopy();

            long em2 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Copied ArffInstance in {0} ms.", (em2 - em)));

            uhsAi.removeDatasetsPerAttributeValue("STATUS", "Y");
            uhsAi.Relation = "uhs_arff_alive";

            long em3 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Separated Alive Set in {0} ms.", (em3 - em2)));

            uhsAi2.removeDatasetsPerAttributeValue("STATUS", "N");
            uhsAi2.Relation = "uhs_arff_dead";

            long em4 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Separated Dead Set in {0} ms.", (em4 - em3)));

            new ArffSaver(uhsAi).saveInstance("uhs_arff_alive");
            new ArffSaver(uhsAi2).saveInstance("uhs_arff_dead");

            long em5 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Stored both sets in files, took {0} ms.", (em5 - em4)));

            sql.close();

            Helpers.Utils.Debug(string.Format("Dumping done, took {0} ms.", sw.ElapsedMilliseconds));
        }



        public static void GetDefaultDumpOfComplexUHSSets()
        {
            Helpers.Utils.Debug("Dumping default (complex) Arff UHS files..");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("UHS_EXTENDED_STORIES_2",
                new string[]{
                        "AGE",
                            "STATUS",
                                "T_NONE",
                                        "T_HORMONE",
                                            "T_SURGERY",
                                                "T_PRI_CHEMO",
                                                    "T_ADJ_CHEMO",
                                                        "T_ADJ_RT",
                                                            "T_OOPH",
                                                                "T_PLASTIC",
                                                                    "T_HER"
                                            }, -1, -1, true, true);

            long em = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Gathered MySQL ArffInstance and stored total file in {0} ms.", em));

            new ArffSaver(uhsAi).saveInstance("uhs_ext_stories_evol1");

            Helpers.Utils.Debug(string.Format("Dumping done, tooks {0} ms.", sw.ElapsedMilliseconds));
        }

        public static void GetDefaultMultithreadedAprioriFlexJsonResultOfAliveAndDead()
        {
            Helpers.Utils.Debug("Getting threaded Apriori Flex Results of Alive and Dead datasets..");
            Stopwatch sw = new Stopwatch(); sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("uhs_patient_story".ToUpper(),
                new string[]{
                        "INITIAL_PRESENTATION",
                            "INT_1_PRES",
                                "INT_2_PRES",
                                        "STATUS"
                                            }, -1, -1, false, true);

            Helpers.Utils.Debug("Gathered MySQL ArffInstance in " + sw.ElapsedMilliseconds + "ms.");
            uhsAi.Relation = "uhs_arff_total";

            Thread t1 = new Thread(new ThreadStart(delegate()
            {
                ArffInstance uhsAi2 = uhsAi.toCopy();
                uhsAi2.removeDatasetsPerAttributeValue("STATUS", "Y");
                uhsAi2.Relation = "uhs_arff_alive";
                new Ceka.Algorithms.Associaters.Apriori(uhsAi2, 0.2f, 0.1f, "apriori_result_alive");
            }));

            Thread t2 = new Thread(new ThreadStart(delegate()
            {
                ArffInstance uhsAi3 = uhsAi.toCopy();
                uhsAi3.removeDatasetsPerAttributeValue("STATUS", "N");
                uhsAi3.Relation = "uhs_arff_dead";
                new Ceka.Algorithms.Associaters.Apriori(uhsAi3, 0.2f, 0.1f, "apriori_result_dead");
            }));

            t1.Start();
            t2.Start();

            new Ceka.Algorithms.Associaters.Apriori(uhsAi, 0.2f, 0.1f, "apriori_result_total");

            t1.Join();
            t2.Join();

            Helpers.Utils.Debug("Json Apriori Flex Result Dump done, took " + sw.ElapsedMilliseconds + "ms."); sw.Stop();
        }

        public static void TestAprioriOnAliveSetFilter()
        {
            Helpers.Utils.Debug("Testing apriori on alive dataset..");
            Stopwatch sw = new Stopwatch(); sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("uhs_patient_story".ToUpper(),
                new string[]{
                        "INITIAL_PRESENTATION",
                            "INT_1_PRES",
                                "INT_2_PRES",
                                        "STATUS"
                                            }, -1, -1, false, true);

            long t = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug("Gathered MySQL ArffInstance in " + t + "ms.");
           
            uhsAi.removeDatasetsPerAttributeValue("STATUS", "Y");
            uhsAi.Relation = "uhs_arff_alive";

            Helpers.Utils.Debug("Cleaned ArffInstance in " + (sw.ElapsedMilliseconds - t) + "ms.");

            new Ceka.Algorithms.Associaters.Apriori(uhsAi, 0.2f, 0.1f, "apriori_result_alive");

            Helpers.Utils.Debug("Apriori test done, took " + sw.ElapsedMilliseconds + "ms."); sw.Stop();
        }

        public static void TestAprioriOnAliveSetNoFilter()
        {
            Helpers.Utils.Debug("Testing apriori on alive dataset..");
            Stopwatch sw = new Stopwatch(); sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("uhs_patient_story".ToUpper(),
                new string[]{
                        "INITIAL_PRESENTATION",
                            "INT_1_PRES",
                                "INT_2_PRES",
                                        "STATUS"
                                            }, -1, -1, false, true);

            long t = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug("Gathered MySQL ArffInstance in " + t + "ms.");

            uhsAi.removeDatasetsPerAttributeValue("STATUS", "Y");
            uhsAi.Relation = "uhs_arff_alive";

            Helpers.Utils.Debug("Cleaned ArffInstance in " + (sw.ElapsedMilliseconds - t) + "ms.");

            new Ceka.Algorithms.Associaters.Apriori(uhsAi, 0.2f, 0.1f, true, true, "apriori_result_alive", true);

            Helpers.Utils.Debug("Apriori test done, took " + sw.ElapsedMilliseconds + "ms."); sw.Stop();
        }

        public static void LoadArffFileAndRunApriori(string file = "test")
        {
            Helpers.Utils.Debug(string.Format("Running apriori on ARFF file {0}.arff..", file));
            Stopwatch sw = new Stopwatch(); sw.Start();

            Loader.ArffLoader al = new Loader.ArffLoader(file);
            al.loadArff();

            ArffInstance ai = al.getInstance();
            ai.Datasets.removeEmptyValueDatasets();

            long t = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Read & Parsed ARFF file in {0} ms.", t));

            new Ceka.Algorithms.Associaters.Apriori(ai, 0.5f, 0.5f, true, true, file + "_result", true);

            Helpers.Utils.Debug(string.Format("Finished, took {0} ms.", sw.ElapsedMilliseconds));
            sw.Stop();
        }

        public static void CompareSizeOfLoadedInstance(string file = "test")
        {
            Helpers.Utils.Debug("Running Memory Size Comparing-Test..");
            Stopwatch sw = new Stopwatch(); sw.Start();

            Loader.ArffLoader al = new Loader.ArffLoader(file);
            al.loadArff();

            ArffInstance ai = al.getInstance();

            long t = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Read & Parsed ARFF file in {0} ms.", t));

            SimpleArffInstance si = new SimpleArffInstance(ai);

            long t2 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Converting complex instance in {0} ms.", (t2 - t)));

            Helpers.Utils.Debug(string.Format("Simple Instance Size: {0} Kb.", si.GetMemorySize()));
            Helpers.Utils.Debug(string.Format("Complex Instance Size: {0} Kb.", ai.GetMemorySize()));

            Helpers.Utils.Debug(string.Format("Finished, took {0} ms.", sw.ElapsedMilliseconds));
        }

        public static void LoadArffFileAndRunAprioriWithWekaOutput(string file = "test")
        {
            Helpers.Utils.Debug(string.Format("Running apriori on ARFF file, with WEKA output, {0}.arff..", file));
            Stopwatch sw = new Stopwatch(); sw.Start();

            Loader.ArffLoader al = new Loader.ArffLoader(file);
            al.loadArff();

            ArffInstance ai = al.getInstance();
            ai.Datasets.removeEmptyValueDatasets();

            long t = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Read & Parsed ARFF file in {0} ms.", t));

            new Ceka.Algorithms.Associaters.Apriori(ai, 0.1f, 0.5f, true, true, AprioriSaveTypes.WEKA);

            Helpers.Utils.Debug(string.Format("Finished, took {0} ms.", sw.ElapsedMilliseconds));
            sw.Stop();
        }

        public static void GetDefaultDumpOfAliveAndDeadDeepClean()
        {
            Helpers.Utils.Debug("Dumping default alive and dead ARFF files..");
            Stopwatch sw = new Stopwatch(); sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("uhs_patient_story".ToUpper(),
                new string[]{
                        "INITIAL_PRESENTATION",
                            "INT_1_PRES",
                                "INT_2_PRES",
                                        "STATUS"
                                            }, -1, -1, false, true);

            long em = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Gathered MySQL ArffInstance in {0} ms.", em));

            List<string> pattern1 = new List<string>() { "Primary|breast|cancer|(and/or|DCIS)", "Primary|breast|cancer|(and/or|DCIS)", "UNDEFINED" };
            List<string> pattern2 = new List<string>() { "Primary|breast|cancer|(and/or|DCIS)", "UNDEFINED", "UNDEFINED" };

            uhsAi.deletePatternMatchingDatasets(pattern1);
            uhsAi.deletePatternMatchingDatasets(pattern2);

            long emx = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Pattern match deletion took {0} ms.", (emx - em)));

            new ArffSaver(uhsAi).saveInstance("uhs_arff_total");
            ArffInstance uhsAi2 = uhsAi.toCopy();

            long em2 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Copied ArffInstance (+save of total set) in {0} ms.", (em2 - emx)));

            uhsAi.removeDatasetsPerAttributeValue("STATUS", "Y");
            uhsAi.Relation = "uhs_arff_alive";

            long em3 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Separated Alive Set in {0} ms.", (em3 - em2)));

            uhsAi2.removeDatasetsPerAttributeValue("STATUS", "N");
            uhsAi2.Relation = "uhs_arff_dead";

            long em4 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Separated Dead Set in {0} ms.", (em4 - em3)));

            new ArffSaver(uhsAi).saveInstance("uhs_arff_alive");
            new ArffSaver(uhsAi2).saveInstance("uhs_arff_dead");

            long em5 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Stored both sets in files, took {0} ms.", (em5 - em4)));

            sql.close();

            Helpers.Utils.Debug(string.Format("Dumping done, took {0} ms.", sw.ElapsedMilliseconds));
        }

        public static void RunAprioriOnDeepCleanedUhsAliveDataset(float support, float confidence)
        {
            Helpers.Utils.Debug("Running Apriori on deep cleaned UHS alive dataset from DB..");
            Stopwatch sw = new Stopwatch(); sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("uhs_patient_story".ToUpper(),
                new string[]{
                        "INITIAL_PRESENTATION",
                            "INT_1_PRES",
                                "INT_2_PRES",
                                        "STATUS"
                                            }, -1, -1, false, true);

            long em = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Gathered MySQL ArffInstance in {0} ms.", em));

            uhsAi.removeDatasetsPerAttributeValue("STATUS", "N");

            List<string> pattern1 = new List<string>() { "Primary|breast|cancer|(and/or|DCIS)", "Primary|breast|cancer|(and/or|DCIS)", "*" };
            List<string> pattern2 = new List<string>() { "Primary|breast|cancer|(and/or|DCIS)", "UNDEFINED", "*" };
            List<string> pattern3 = new List<string>() { "Primary|breast|cancer|(and/or|DCIS)", "*", "UNDEFINED" };
            List<string> pattern4 = new List<string>() { "*", "UNDEFINED", "UNDEFINED" };

            uhsAi.deletePatternMatchingDatasets(pattern1);
            uhsAi.deletePatternMatchingDatasets(pattern2);
            uhsAi.deletePatternMatchingDatasets(pattern3);
            uhsAi.deletePatternMatchingDatasets(pattern4);
            uhsAi.Datasets.removeEmptyValueDatasets();

            long t = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Cleansed dataset in {0} ms.", t));

            new Ceka.Algorithms.Associaters.Apriori(uhsAi, support, confidence, true, true, AprioriSaveTypes.WEKA);

            Helpers.Utils.Debug(string.Format("Finished, took {0} ms.", sw.ElapsedMilliseconds));
            sw.Stop();
        }

        public static void GetDefaultDumpOfAliveDeepClean()
        {
            Helpers.Utils.Debug("Running Apriori on deep cleaned UHS alive dataset from DB..");
            Stopwatch sw = new Stopwatch(); sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("uhs_patient_story".ToUpper(),
                new string[]{
                        "INITIAL_PRESENTATION",
                            "INT_1_PRES",
                                "INT_2_PRES",
                                        "STATUS"
                                            }, -1, -1, false, true);

            long em = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Gathered MySQL ArffInstance in {0} ms.", em));

            uhsAi.removeDatasetsPerAttributeValue("STATUS", "N");

            List<string> pattern1 = new List<string>() { "Primary|breast|cancer|(and/or|DCIS)", "Primary|breast|cancer|(and/or|DCIS)", "*" };
            List<string> pattern2 = new List<string>() { "Primary|breast|cancer|(and/or|DCIS)", "UNDEFINED", "*" };
            List<string> pattern3 = new List<string>() { "Primary|breast|cancer|(and/or|DCIS)", "*", "UNDEFINED" };
            List<string> pattern4 = new List<string>() { "*", "UNDEFINED", "UNDEFINED" };

            uhsAi.deletePatternMatchingDatasets(pattern1);
            uhsAi.deletePatternMatchingDatasets(pattern2);
            uhsAi.deletePatternMatchingDatasets(pattern3);
            uhsAi.deletePatternMatchingDatasets(pattern4);
            uhsAi.Datasets.removeEmptyValueDatasets();

            long t = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Cleansed dataset in {0} ms.", t));

            new ArffSaver(uhsAi).saveInstance("uhs_clean_alive");

            Helpers.Utils.Debug(string.Format("Finished, took {0} ms.", sw.ElapsedMilliseconds));
            sw.Stop();
        }

        public static void GetDumpOfComplexUHSClassificationMaster()
        {
            Helpers.Utils.Debug("Dumping (complex) ARFF UHS for classifiers..");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("UHS_EXTENDED_STORIES_2",
                new string[]{
                "MNTHS_TO_1",
                    "MNTHS_TO_2",
                        "AGE",
                            "STATUS",
                                "T_NONE",
                                        "T_HORMONE",
                                            "T_SURGERY",
                                                "T_PRI_CHEMO",
                                                    "T_ADJ_CHEMO",
                                                        "T_ADJ_RT",
                                                            "T_OOPH",
                                                                "T_PLASTIC",
                                                                    "T_HER"
                                            }, -1, -1, false, false);

            long em = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Gathered MySQL ArffInstance in {0} ms.", em));

            uhsAi.rebuildAttributeValueByRange(0, 120);
            uhsAi.rebuildAttributeValueByRange(1, 120);
            uhsAi.rebuildAttributeValueByRange(2, 16);

            uhsAi.deletePatternMatchingDatasets(new List<string> { "[-19<->101]" });
            uhsAi.deletePatternMatchingDatasets(new List<string> { "*", "[-67<->53]" });

            uhsAi.removeUnusedAttributeValues();

            long em2 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Prepared classifier set in {0} ms.", (em2 - em)));

            new ArffSaver(uhsAi).saveInstance("uhs_ext_story_classifier_evol2");

            Helpers.Utils.Debug(string.Format("Dumping done, tooks {0} ms.", sw.ElapsedMilliseconds));
        }

        public static void Evol3_BuildAprioriInstance()
        {
            Helpers.Utils.Debug("Evol3 Apriori..");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToInstance("UHS_EXTENDED_STORIES_2",
                new string[]{
                "MNTHS_TO_1",
                    "MNTHS_TO_2",
                        "AGE",
                            "STATUS",
                                "T_NONE",
                                        "T_HORMONE",
                                            "T_SURGERY",
                                                "T_PRI_CHEMO",
                                                    "T_ADJ_CHEMO",
                                                        "T_ADJ_RT",
                                                            "T_OOPH",
                                                                "T_PLASTIC",
                                                                    "T_HER"
                                            }, -1, -1, false, false);

            long em = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Gathered MySQL ArffInstance {0} ms.", em));

            uhsAi.integrityCheck();

            uhsAi.rebuildAttributeValueByRange(0, 120);
            uhsAi.rebuildAttributeValueByRange(1, 120);
            uhsAi.rebuildAttributeValueByRange(2, 16);

            uhsAi.deletePatternMatchingDatasets(new List<string> { "[-19<->101]" });
            uhsAi.deletePatternMatchingDatasets(new List<string> { "*", "[-67<->53]" });

            uhsAi.removeUnusedAttributeValues();
            uhsAi.Relation = "uhs_ext_story_apriori_evol3";

            long em2 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Prepared apriori set in {0} ms.", (em2 - em)));

            new ArffSaver(uhsAi).saveInstance("uhs_ext_story_apriori_evol3");

            Helpers.Utils.Debug(string.Format("Evol done, tooks {0} ms.", sw.ElapsedMilliseconds));
        }

        public static void Evol3_BuildClassifierInstance()
        {
            Helpers.Utils.Debug("Evol3 Classifier..");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToValidatedInstance("UHS_EXTENDED_STORIES_2",
                new string[]{
                "MNTHS_TO_1",
                    "MNTHS_TO_2",
                        "AGE",
                            "STATUS",
                                "T_NONE",
                                        "T_HORMONE",
                                            "T_SURGERY",
                                                "T_PRI_CHEMO",
                                                    "T_ADJ_CHEMO",
                                                        "T_ADJ_RT",
                                                            "T_OOPH",
                                                                "T_PLASTIC",
                                                                    "T_HER"
                                            }, new string[]{
                                            "MNTHS_TO_1",
                                                "MNTHS_TO_2",
                                                    "AGE"
                                                }, -1, -1, false, false);

            long em = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Gathered MySQL ArffInstance + preparing.. in {0} ms.", em));

            uhsAi.deletePatternMatchingDatasets(new List<string>(){
                "*", "0"
            });
            uhsAi.Relation = "uhs_ext_story_classifier_evol3";

            long em2 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Prepared classifier set in {0} ms.", (em2 - em)));

            new ArffSaver(uhsAi).saveInstance("uhs_ext_story_classifier_evol3");

            Helpers.Utils.Debug(string.Format("Evol done, tooks {0} ms.", sw.ElapsedMilliseconds));
        }

        public static void Evol4_BuildClassifierInstance()
        {
            Helpers.Utils.Debug("Evol4 Classifier..");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string mysqlConStr = "SERVER=localhost;" +
                            "DATABASE=uhs;" +
                            "UID=root;" +
                            "PASSWORD=pascal;";

            CekaMySQL sql = new CekaMySQL(mysqlConStr);

            ArffInstance uhsAi = sql.tableToValidatedInstance("UHS_EXTENDED_STORIES_2",
                new string[]{
                "MNTHS_TO_1",
                    "MNTHS_TO_2",
                        "AGE",
                            "STATUS",
                                "T_NONE",
                                        "T_HORMONE",
                                            "T_SURGERY",
                                                "T_PRI_CHEMO",
                                                    "T_ADJ_CHEMO",
                                                        "T_ADJ_RT",
                                                            "T_OOPH",
                                                                "T_PLASTIC",
                                                                    "T_HER"
                                            }, new string[]{
                                            "MNTHS_TO_1",
                                                "MNTHS_TO_2"
                                                }, -1, -1, false, false);

            long em = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Gathered MySQL ArffInstance + preparing.. in {0} ms.", em));

            uhsAi.deletePatternMatchingDatasets(new List<string>(){
                "*", "0"
            });

            int ageIndex = uhsAi.getIndexOfAttribute("AGE");

            uhsAi.rebuildAttributeValueByRange(ageIndex, 29);

            uhsAi.refineBackRangedAttribute(ageIndex, 3, 5);

            uhsAi.Relation = "uhs_ext_story_classifier_evol4";

            long em2 = sw.ElapsedMilliseconds;
            Helpers.Utils.Debug(string.Format("Prepared classifier set in {0} ms.", (em2 - em)));

            new ArffSaver(uhsAi).saveInstance("uhs_ext_story_classifier_evol4");

            Helpers.Utils.Debug(string.Format("Evol done, tooks {0} ms.", sw.ElapsedMilliseconds));
        }

    }
}
#endif