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

/* requires orcales mysql connector:
 * https://dev.mysql.com/downloads/connector/net/
 */
using MySql.Data.MySqlClient;

namespace DM.Ceka.Database
{
    /// <summary>
    /// class that turns mysql tables into arff instances
    /// </summary>
    public class CekaMySQL
    {
        /// <summary>
        /// database connection string
        /// </summary>
        private string conStr;

        /// <summary>
        /// first database connection
        /// </summary>
        private MySqlConnection connection;

        /// <summary>
        /// second database connection
        /// </summary>
        private MySqlConnection connection2; //the arff instance generation requires two database connections

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="conStr">mysql connection string</param>
        public CekaMySQL(string conStr)
        {
            this.conStr = conStr;
            this.connection = new MySqlConnection(this.conStr);
            this.connection2 = new MySqlConnection(this.conStr);
            connection.Open();
            connection2.Open();
        }

        /// <summary>
        /// simple query (with result-set) wrapper
        /// </summary>
        /// <param name="query"></param>
        /// <param name="con2"></param>
        /// <returns></returns>
        public MySqlDataReader query(string query, bool con2 = false)
        {
            MySqlCommand command = new MySqlCommand();

            if (!con2)
                command.Connection = connection;
            else
                command.Connection = connection2;

            command.CommandText = query;
            return command.ExecuteReader();
        }

        /// <summary>
        /// simple query (without) result-set wrapper
        /// </summary>
        /// <param name="query"></param>
        /// <param name="con2"></param>
        public int noQuery(string query, bool con2 = false)
        {
            MySqlCommand command = new MySqlCommand();

            if (!con2)
                command.Connection = connection;
            else
                command.Connection = connection2;

            command.CommandText = query;
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// turns table structur into attributes and table rows (content) into data rows
        /// </summary>
        /// <param name="table">the mysql database table</param>
        /// <param name="table_col">a list of columns names that are to be read into the arff instance</param>
        /// <param name="startIndex">table row start index; use -1 for no limit</param>
        /// <param name="endIndex">table row end index; use -1 for no limit</param>
        /// <param name="firstUndefined">can the first column of the passed column array be NULL</param>
        /// <param name="secondUndefined">can the second column of the passed column array be NULL</param>
        /// <returns>returns the generated arff instance</returns>
        public ArffInstance tableToInstance(string table, string[] table_col, int startIndex = -1, int endIndex = -1, bool firstUndefined = false, bool secondUndefined = false)
        {
            if (table_col == null || table_col.Length < 2)
                throw new CekaException("can not create an ArffInstance from a Table with less then 2 columns.");

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");

            for(int i = 0; i < table_col.Length; i++)
            {
                sb.Append(table_col[i]);

                if(i != (table_col.Length - 1))
                    sb.Append(", ");
            }

            sb.Append(" FROM ");
            sb.Append(table);
            if (!firstUndefined)
            {
                sb.Append(" WHERE ");
                sb.Append(table_col[0]);
                if (!secondUndefined)
                {
                    sb.Append(" IS NOT NULL AND ");
                    sb.Append(table_col[1]);
                    sb.Append(" IS NOT NULL");
                }
                else
                {
                    sb.Append(" IS NOT NULL");
                }
            }

            MySqlDataReader mr = query(sb.ToString());

            ArffInstance ai = new ArffInstance("ceka_" + table);

            foreach (string s in table_col) //setup attributes from table columns
                ai.addAttribute(s, this.getDistinctOccurencesInColumn(table, s));

            //gather data from select resultset into a library dataset
            int c = 0;
            string[] sa = new string[table_col.Length];
            while (mr.Read())
            {
                if ((startIndex == -1 || c >= startIndex) && (endIndex == -1 || c <= endIndex)) //make sure to run in limes
                {
                    for (int i = 0; i < mr.FieldCount; i++)
                    {
                        sa[i] = mr.GetValue(i).ToString();

                        if (string.IsNullOrWhiteSpace(sa[i]) || string.IsNullOrEmpty(sa[i]))
                            sa[i] = ArffFile.ATT_UNDEFINED; //make sure to give these a value at all times
                        else if (sa[i].Contains(ArffFile.ARFF_SPACE)){ //also make sure there a no whitespaces at all times
                            sa[i] = sa[i].Replace(ArffFile.ARFF_SPACE, ArffFile.ATT_SPACE_EXCHANGE);

                            if (string.IsNullOrEmpty(sa[i]) || string.IsNullOrWhiteSpace(sa[i]))
                            {
                                sa[i] = ArffFile.ATT_UNDEFINED;
                            }
                        }
                    }

                    ai.addDataset(sa);
                    sa = new string[table_col.Length];
                }
                c++;
            }

            mr.Close();

            return ai;
        }

        /// <summary>
        /// select attributes possible values from all table rows -> @attribute { val1, val2, val3 ... }
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public string[] getDistinctOccurencesInColumn(string table, string column)
        {
            List<string> ocs = new List<string>();
            ocs.Add(ArffFile.ATT_UNDEFINED);

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT DISTINCT ");
            sb.Append(column);
            sb.Append(" FROM ");
            sb.Append(table);
            MySqlDataReader mr = query(sb.ToString(), true);

            string f = "";
            while (mr.Read())
            {
                f = mr.GetValue(0).ToString();

                if (string.IsNullOrWhiteSpace(f))
                    continue; //bug fix!

                if (f.Contains(ArffFile.ARFF_SPACE)) //also make sure there a no whitespaces at all times
                    f = f.Replace(ArffFile.ARFF_SPACE, ArffFile.ATT_SPACE_EXCHANGE);

                ocs.Add(f);
            }

            mr.Close();

            return ocs.ToArray<string>();
        }

        /// <summary>
        /// same as tableToInstance(), but it adds validity checking
        /// </summary>
        /// <param name="table"></param>
        /// <param name="table_col"></param>
        /// <param name="numeric"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="firstUndefined"></param>
        /// <param name="secondUndefined"></param>
        /// <returns></returns>
        public ArffInstance tableToValidatedInstance(string table, string[] table_col, string[] numeric, int startIndex = -1, int endIndex = -1, bool firstUndefined = false, bool secondUndefined = false)
        {
            ArffInstance ai = this.tableToInstance(table, table_col, startIndex, endIndex, firstUndefined, secondUndefined);

            if (numeric != null)
            {
                foreach (string an in numeric)
                    ai.turnAttributeIntoNumeric(an, "numeric");
            }

            ai.Headers.CleanUp();
            ai.removeUnusedAttributeValues();
            ai.integrityCheck();
            
            return ai;
        }

        /// <summary>
        /// closes both database connections
        /// </summary>
        public void close()
        {
            connection.Close();
            connection2.Close();
        }

    }
}