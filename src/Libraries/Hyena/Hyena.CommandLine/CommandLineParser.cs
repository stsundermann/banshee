// 
// CommandLineParser.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hyena.CommandLine
{
    public class CommandLineParser
    {
        private int offset;
        private string enqueue_arg;
        private string [] arguments;
        private Dictionary<string, string> parsed_arguments = new Dictionary<string, string> ();
        private List<string> file_list = new List<string> ();
        
        public CommandLineParser () : this (null, Environment.GetCommandLineArgs (), 1)
        {
        }
        
        public CommandLineParser (string enqueueArgument) : this (enqueueArgument, Environment.GetCommandLineArgs (), 1)
        {
        }
        
        public CommandLineParser (string enqueueArgument, string [] arguments, int offset)
        {
            this.enqueue_arg = enqueueArgument;
            this.arguments = arguments;
            this.offset = offset;
            
            Parse ();
        }
        
        private void Parse ()
        {
            bool enqueue_mode = false;
            
            for (int i = offset; i < arguments.Length; i++) {
                if (enqueue_mode || !IsOption (arguments[i])) {
                    file_list.Add (arguments[i]);
                    continue;
                }
                
                string name = OptionName (arguments[i]);
                string value = String.Empty;
                
                if (name == enqueue_arg) {
                    enqueue_mode = true;
                    continue;
                }

                int eq_offset = name.IndexOf ('=');
                if (eq_offset > 1) {
                    value = name.Substring (eq_offset + 1);
                    name = name.Substring (0, eq_offset);
                } else if (i < arguments.Length - 1 && !IsOption (arguments[i + 1])) {
                    value = arguments[i + 1];
                    i++;
                }
                  
                if (parsed_arguments.ContainsKey (name)) {
                    parsed_arguments[name] = value;
                } else {
                    parsed_arguments.Add (name, value);
                }
            }
        }
        
        private bool IsOption (string argument)
        {
            return argument.Length > 2 && argument.Substring (0, 2) == "--";
        }
        
        private string OptionName (string argument)
        {
            return argument.Substring (2);
        }
        
        public bool Contains (string name)
        {
            return parsed_arguments.ContainsKey (name);
        }
        
        public string this[string name] {
            get { return Contains (name) ? parsed_arguments[name] : String.Empty; }
        }
        
        public ReadOnlyCollection<string> Files {
            get { return new ReadOnlyCollection<string> (file_list); }
        }
            
        public override string ToString ()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder ();

            builder.Append ("Parsed Arguments\n");
            foreach (KeyValuePair<string, string> argument in parsed_arguments) {
                builder.AppendFormat ("  {0} = [{1}]\n", argument.Key, argument.Value); 
            }
            
            builder.Append ("\nFile List\n");
            foreach (string file in file_list) {
                builder.AppendFormat ("{0}\n", file);
            }
            
            return builder.ToString ();
        }
    }
}
