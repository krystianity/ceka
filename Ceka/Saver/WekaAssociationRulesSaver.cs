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

namespace DM.Ceka.Saver
{
    /// <summary>
    /// wraps the result saver and adds weka-result-alike stringbuilding in constructors
    /// </summary>
    public class WekaAssociationRulesSaver : ResultSaver
    {
        /// <summary>
        /// trys to mimic the output of Weka's Associator Rule Results (humanreadable) - AprioriResult
        /// </summary>
        /// <param name="result"></param>
        public WekaAssociationRulesSaver(Ceka.Algorithms.Associaters.Apriori.NestedAprioriResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("=== Run information === \n\n");
            sb.Append("Scheme: Ceka.Algorithms.Associators.Apriori \n");
            sb.Append("Minimum Support Filter Active: ");
            sb.Append(result.SupportApplied);
            sb.Append("\n");
            sb.Append("Confidence Filter Active: ");
            sb.Append(result.ConfidenceApplied);
            sb.Append("\n");
            sb.Append("Relation: ");
            sb.Append(result.Relation);
            sb.Append("\n");
            sb.Append("Instances: ");
            sb.Append(result.TotalDatasets);
            sb.Append("\n");
            sb.Append("Attributes: ");
            sb.Append(result.Attributes.Count);
            sb.Append("\n");

            foreach (string s in result.Attributes)
            {
                sb.Append("\t");
                sb.Append(s);
                sb.Append("\n");
            }

            sb.Append("Attributes of Value: ");
            sb.Append(result.ItemTable.Count);
            sb.Append("\n");

            foreach (List<object> at in result.ItemTable)
            {
                sb.Append("\t");
                foreach (object o in at)
                {
                    sb.Append(o);
                    sb.Append(" ");
                }
                sb.Append("\n");
            }
            sb.Append("=== Associator model (full training set) ===\n\n\n");
            sb.Append("Apriori\n=======\n\n");
            sb.Append("Minimum support: ");
            sb.Append(result.MinimumSupport);
            sb.Append(" (");
            sb.Append((int)(result.TotalDatasets * result.MinimumSupport));
            sb.Append(" instances)\n");
            sb.Append("Minimum metric <confidence>: ");
            sb.Append(result.Confidence);
            sb.Append("\nNumber of cycles performed: ");
            sb.Append(result.CyclesRan);
            sb.Append("\n\nGenerated sets of large itemsets:\n\n");

            int lloc = 1;
            foreach (List<List<object>> ds in result.CycleResults)
            {
                sb.Append("Size of set of large itemsets L(");
                sb.Append(lloc);
                sb.Append("): ");
                sb.Append(ds.Count);
                sb.Append("\n\n");
                lloc++;
            }

            sb.Append("Best rules found:\n\n");

            int line = 1;
            for (int i = result.CycleResults.Count - 1; i >= 0; i--)
            {
                foreach (List<object> dsl in result.CycleResults[i])
                {
                    sb.Append(line);
                    sb.Append(". ");

                    for (int d = 0; d < dsl.Count - 2; d++)
                    {
                        sb.Append(dsl[d]);
                        sb.Append(" ");

                        if (d == dsl.Count -4)
                            sb.Append("==> ");
                    }

                    sb.Append(" conf:(");
                    sb.Append(float.Parse(dsl[dsl.Count - 1].ToString()) / 1000);
                    sb.Append(")\n");

                    line++;
                    //dsl[dsl.Count - 1] = confidence
                    //dsl[dsl.Count - 2] = support
                }
            }

            this.value = sb.ToString();
        }

    }
}