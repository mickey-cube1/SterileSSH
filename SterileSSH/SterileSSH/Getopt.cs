
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*
 * Getopt.cs
 * Copyright (c) 2017 micky-cube1.
 * This software is released under the MIT License.
 * Relicensed under The 3-Clause BSD license:
 */
/*
Copyright (c) 2017 micky-cube1 All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

  1. Redistributions of source code must retain the above copyright notice,
	 this list of conditions and the following disclaimer.

  2. Redistributions in binary form must reproduce the above copyright 
	 notice, this list of conditions and the following disclaimer in 
	 the documentation and/or other materials provided with the distribution.

  3. The names of the authors may not be used to endorse or promote products
	 derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
namespace SterileSSH
{
    class Getopt
    {
        public Int32 optind;
        public Char optopt;
        public String optarg;
        private Int32 place;

        public Getopt()
        {
            optind = 0;
            optopt = '?';
            optarg = null;
            place = -1;
        }
        public Int32 getopt(String[] argv, String ostr)
        {
            Int32 oli;

            if (place == -1)
            {
                place = 0;
                if (optind >= argv.Length || argv[optind][place++] != '-')
                {
                    place = -1;
                    return -1;
                }
                optopt = argv[optind][place++];
                if (optopt == '-' && argv[optind][place] == '\0')
                {
                    optind++;
                    place = -1;
                    return -1;
                }
                if (optopt == 0)
                {
                    place = -1;
                    if (ostr.IndexOf('-') == -1)
                    {
                        return -1;
                    }
                    optopt = '-';
                }
            }
            else
            {
                optopt = argv[optind][place++];
            }

            if (optopt == ':' || (oli = ostr.IndexOf(optopt)) == -1)
            {
                if (place >= argv[optind].Length)
                {
                    optind++;
                    place = -1;
                }
                return '?';
            }

            if (ostr.Length - 1 == oli || ostr[oli + 1] != ':')
            {
                /* no argument option */
                optarg = null;
                if (place >= argv[optind].Length)
                {
                    optind++;
                    place = -1;
                }

            }
            else
            {
                /* option with an argument */
                if (place < argv[optind].Length - 1)
                {
                    optarg = argv[optind].Substring(place);
                }
                else if (argv.Length > ++optind)
                {
                    optarg = argv[optind];
                }
                else
                {
                    place = -1;
                    if (ostr[0] == ':')
                    {
                        return ':';
                    }
                    else
                    {
                        return '?';
                    }
                }
                place = -1;
                optind++;
            }
            return optopt;
        }
    }
}
