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

/* this class contains function calls that were used throughout the UHS project at SSU in 2014/2015.
 * it need not be included in the built libraries, therefore the define block was commented out,
 * each function represents a step in which an arff dataset was prepared or mined during the project's
   progress - it was left in the solution as it might be of help to someone. */

//uncomment this if you want to include it in your builds
//#define UHSP

#if UHSP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using DM.Ceka.UHS;

namespace DM
{
    class Program
    {
        static void Main()
        {
            DM.Ceka.Saver.ArffSaver.OVERRIDE = true;

            //Weka.PortStatus.print_port_status_of_all_classes();

            //Ceka.UHS.StaticBuilder.GetDefaultDumpOfAliveAndDead();
            //Ceka.UHS.StaticBuilder.GetDefaultMultithreadedAprioriFlexJsonResultOfAliveAndDead();
            //Ceka.UHS.StaticBuilder.TestAprioriOnAliveSetFilter();
            //Ceka.UHS.StaticBuilder.TestAprioriOnAliveSetNoFilter();
            //Ceka.UHS.StaticBuilder.LoadArffFileAndRunApriori();
            //Ceka.UHS.StaticBuilder.CompareSizeOfLoadedInstance();
            //Ceka.UHS.StaticBuilder.LoadArffFileAndRunAprioriWithWekaOutput();
            //Ceka.UHS.StaticBuilder.GetDefaultDumpOfComplexUHSSets();
            //Ceka.UHS.StaticBuilder.GetDefaultDumpOfAliveAndDeadDeepClean();
            //Ceka.UHS.StaticBuilder.LoadArffFileAndRunAprioriWithWekaOutput();
            //StaticBuilder.RunAprioriOnDeepCleanedUhsAliveDataset(0.2f, 0.2f);

            //25.02.2015
            //StaticBuilder.GetDefaultDumpOfAliveDeepClean();
            //StaticBuilder.GetDumpOfComplexUHSClassificationMaster();

            //01.03.2015
            //StaticBuilder.Evol3_BuildAprioriInstance();
            //StaticBuilder.Evol3_BuildClassifierInstance();
            //StaticBuilder.Evol4_BuildClassifierInstance();

            //06.05.2015
            //HashComparison.run_int_vs_string_comparison(10, 5, 50000, 90);

            Console.ReadKey();
        }
    }
}
#endif