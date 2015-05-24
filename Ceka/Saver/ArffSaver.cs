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
using System.IO;

namespace DM.Ceka.Saver
{
    /// <summary>
    /// not a result-saver, this class is used to write arff instances (back) to files
    /// </summary>
    public class ArffSaver
    {
        /// <summary>
        /// should the saver override .arff files that already exist?, if this is false it will through an exception if so
        /// </summary>
        public static bool OVERRIDE = false;

        /// <summary>
        /// the arff instance that will be written to file
        /// </summary>
        public ArffInstance Instance { set; get; }

        /// <summary>
        /// writer for the file..
        /// </summary>
        public StreamWriter Writer { set; get; }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="instance">the arff instance you want to write to a file</param>
        public ArffSaver(ArffInstance instance)
        {
            this.Instance = instance;
        }

        /// <summary>
        /// writes the arff instance (given in constructor or via ArffSaver.Instance = value) to a file
        /// </summary>
        /// <param name="file">filename, without .arff extension!</param>
        /// <param name="comments">array of comments you might want to add to the arff file</param>
        public void saveInstance(string file, string[] comments = null)
        {
            string comfile = file + ArffFile.FILE_EXTENSION;

            if (File.Exists(comfile))
            {
                if(!OVERRIDE)
                    throw new CekaException("the file(" + comfile + ") already exists.");
                else
                {
                    File.Delete(comfile);
                    Helpers.Utils.Debug("File " + comfile + " already existed, it was deleted..");
                }
            }

            this.Writer = new StreamWriter(comfile);

            //easy reading = easy understanding
            writeComments(comments); //%..
            writeRelation(); //@relation ..
            writeHeaders(); //@attributes .. {..}
            writeDatasets(); //@data ..
            writeEnd(); //eof

            this.Writer.Close();
        }

        /// <summary>
        /// inner..writes comments to file
        /// </summary>
        /// <param name="comments"></param>
        private void writeComments(string[] comments)
        {
            this.Writer.WriteLine(ArffFile.ATT_IGNO + " created with Ceka v. " + ArffFile.CEKA_VERSION);
            this.Writer.WriteLine(ArffFile.ATT_IGNO + " on " + DateTime.Now.ToLongDateString());
            this.Writer.WriteLine(ArffFile.ATT_IGNO + " Author: " + ArffFile.CEKA_AUTHOR);
           
            if(comments != null)
                foreach (string s in comments)
                {
                    this.Writer.WriteLine(ArffFile.ATT_IGNO + " " + s);
                }

            this.Writer.WriteLine();
        }

        /// <summary>
        /// inner..writes relation to file
        /// </summary>
        private void writeRelation()
        {
            this.Writer.WriteLine(ArffFile.ARFF_RELATION + " " + this.Instance.Relation);
            this.Writer.WriteLine();
        }

        /// <summary>
        /// inner..writes @attributes to file
        /// </summary>
        private void writeHeaders()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Story s in this.Instance.Headers.Data)
            {
                sb.Append(ArffFile.ARFF_ATTRIBUTE);
                sb.Append(ArffFile.ARFF_SPACE); //start with "@attribute "

                if(s.Name.Contains(ArffFile.ARFF_SPACE)){
                    //contains white space, needs to be enclosed
                    sb.Append(ArffFile.ATT_NAME_COMB);
                    sb.Append(s.Name);
                    sb.Append(ArffFile.ATT_NAME_COMB);
                } else {
                    sb.Append(s.Name);
                }

                sb.Append(ArffFile.ARFF_SPACE); //continue with "@attribute 'name here' "

                if (!s.isNumeric)
                {
                    sb.Append(ArffFile.ATT_START);
                    for (int i = 0; i < s.Values.Length; i++)
                    {
                        sb.Append(s.Values[i]);

                        if (i != (s.Values.Length - 1))
                        {
                            sb.Append(ArffFile.STORY_DELIMITTER);
                            sb.Append(ArffFile.ARFF_SPACE);
                        }
                    }
                    sb.Append(ArffFile.ATT_END); //done with "@attribute 'name here' {el1, el2, el3}"
                }
                else
                {
                    sb.Append(s.Value);
                }

                this.Writer.WriteLine(sb.ToString());
                sb.Clear(); //write the line, clear buffer and up for the next attribute
            }

            this.Writer.WriteLine();
        }

        /// <summary>
        /// inner..writes @data instances to file
        /// </summary>
        private void writeDatasets()
        {
            this.Writer.WriteLine(ArffFile.ARFF_DATA);

            StringBuilder sb = new StringBuilder();
            foreach(Story s in this.Instance.Datasets.Data){

                for (int i = 0; i < s.Data.Length; i++)
                {
                    sb.Append(s.Data[i]);

                    if (i != (s.Data.Length - 1))
                        sb.Append(ArffFile.STORY_DELIMITTER);
                }

                this.Writer.WriteLine(sb.ToString());
                sb.Clear(); //write the line, clear buffer and up for the next dataset
            }

            this.Writer.WriteLine();
        }

        /// <summary>
        /// inner... writes EOF to file
        /// </summary>
        private void writeEnd()
        {
            this.Writer.WriteLine(ArffFile.ATT_IGNO + " " + ArffFile.CEKA_EOF);
        }

    }
}