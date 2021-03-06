﻿using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Budget.API.Services.OFXClient
{
    public static class OFXTagCloser
    {
        // Converting from (pre-version 2.0) OFX to XML is just a matter of
        // adding closing tags of atom elements

        public static string CloseTags(string ofx)
        {
            // remove white space from between tags
            ofx = RemoveWhiteSpaceBetweenTags(ofx);

            // build new xml string
            StringBuilder xml = new StringBuilder();

            // initialize to first char of input OFX string
            xml.Append(ofx[0]);

            // last tag scanned
            string tag = "";

            // track when the index is inside a tag
            bool inTag = ofx[0] == '<' ? true : false;

            // track previous char
            char prevChar = ofx[0];

            // walk across the string and 
            // and place a closing tag whenever a '<' is found
            // that wasn't preceded by a '>'
            for (int i = 1; i < ofx.Length; ++i)
            {
                // Clear previous tag name when new tag openning found
                if (prevChar == '<')
                {
                    tag = "";
                    inTag = true;
                }

                // Add closing tag if it is not already there
                if (ofx[i] == '<' && prevChar != '>')
                {
                    string nextTag = ofx.Substring(i, Math.Min(tag.Length + 3, ofx.Length - i));
                    if (nextTag != "</" + tag + ">")
                    {
                        xml.Append("</" + tag + ">");
                    }
                   
                }

                // build XML string
                xml.Append(ofx[i]);

                // build up tag name
                if (inTag)
                {
                    if (ofx[i] == '>')
                    {
                        inTag = false;
                    }
                    else
                    {
                        tag += ofx[i].ToString();
                    }
                }

                // set prev char
                prevChar = ofx[i];
            }
            return xml.ToString();
        }

        private static string RemoveWhiteSpaceBetweenTags(string ofx)
        {
            // look for any white space framed by a closing ">" and openning "<" bracket 
            string pattern = ">\\s+<";
            // and replace with just a closing and openning "><"
            string replace = "><";
            // do regex replacement
            Regex regex = new Regex(pattern);
            string newOfx = regex.Replace(ofx, replace);
            //return new string
            return newOfx;
        }
    }
}