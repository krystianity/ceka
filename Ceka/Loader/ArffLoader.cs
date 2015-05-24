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

namespace DM.Ceka.Loader
{
    /// <summary>
    /// loads and arff file into cekas arff instance in-memory model
    /// </summary>
    public class ArffLoader : AbstractLoader
    {
        /// <summary>
        /// attributes library that will be filled and passed to the arff instance
        /// </summary>
        protected Library lib_h;

        /// <summary>
        /// dataset library that will be filled and passed to the arff instance
        /// </summary>
        protected Library lib_d;
        
        /// <summary>
        /// empty constructor
        /// </summary>
        public ArffLoader()
            : base()
        {
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="file">file to be loaded, .arff is not needed!</param>
        public ArffLoader(string file)
            : base(file)
        {
        }

        /// <summary>
        /// returns the ILoader content (read file as string)
        /// </summary>
        public override string Content
        {
            get
            {
                return base.content;
            }
            set
            {
                throw new CekaException("you can not set the Content of an ArffLoader");
            }
        }

        /// <summary>
        /// returns the file reader
        /// </summary>
        public override StreamReader Reader
        {
            get
            {
                return base.reader;
            }
            set
            {
                base.reader = value;
            }
        }

        /// <summary>
        /// returns the dataset library
        /// </summary>
        public override Library Dataset 
        {
            get
            {
                return this.lib_d;
            }
            set
            {
                throw new CekaException("you can not set the Library of an ArffLoader");
            }
        }

        /// <summary>
        /// retunrs the attributes library
        /// </summary>
        public override Library Headers
        {
            get
            {
                return this.lib_h;
            }
            set
            {
                throw new CekaException("you can not set the Library of an ArffLoader");
            }
        }

        /// <summary>
        /// loads the (in constructor provided) file
        /// </summary>
        public override void loadArff()
        {
            Helpers.Utils.Debug("reading file: " + this.file + " into library.");

            if (string.IsNullOrEmpty(this.file))
                throw new CekaException("you have to specify a filepath");

            this.lib_d = new Library();
            this.lib_h = new Library();
            bool headers = true;
            int i = 0;

            try
            {
                if (!File.Exists(this.file + ArffFile.FILE_EXTENSION))
                    throw new CekaException("file(" + (this.file + ArffFile.FILE_EXTENSION) + ") does not exist!");

                base.reader = new StreamReader(this.file + ArffFile.FILE_EXTENSION);

                string line;
                Story st;
                while ((line = base.reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line) && !line[0].Equals(ArffFile.ATT_IGNO)) //make sure this line is no file comment
                    {
                        if (line.Contains(ArffFile.ATT_UNS)) //@attribute region-centroid-col numeric is not allowed!
                            throw new CekaException("the attribute description 'numeric' is not supported in Ceka!, define a span or range of your attribute values.");

                        st = new Story(line, i);

                        if (st.Type == EArffTypes.DATA_ATT)
                        {
                            //done with reading headers
                            headers = false;
                            Helpers.Utils.Debug("headers done, " + i + " attributes gone into library");
                        }

                        //splitting the librarys right from the start
                        if (!headers)
                            this.lib_d.Add(st);
                        else
                            this.lib_h.Add(st);
                    }
                    i++;
                }

                base.reader.Close();
                Helpers.Utils.Debug("reading done, " + i + " stories gone into library");
            }
            catch (Exception ex)
            {
                Helpers.Utils.Debug("failed to load file " + this.file + ": " + ex.Message);

                this.lib_h = new Library();
                this.lib_d = new Library();

                Helpers.Utils.PrintArffImportHelp();
            }
        }
         
        /// <summary>
        /// returns the arff instance that was read during loadArff()
        /// </summary>
        /// <returns></returns>
        public ArffInstance getInstance()
        {
            return new ArffInstance(this.lib_h, this.lib_d);
        }

        /// <summary>
        /// returns the arff instance that was read during loadArff() in a simpler memory model
        /// </summary>
        /// <returns></returns>
        public SimpleArffInstance getSimpleInstance()
        {
            return new SimpleArffInstance(new ArffInstance(this.lib_h, this.lib_d));
        }

    }
}