﻿/*
 * Copyright © 2017 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conditions
{
    public class ConditionVariables
    {
        private Dictionary<string, string> values = new Dictionary<string, string>();

        public ConditionVariables()
        {
        }

        public ConditionVariables(ConditionVariables other)
        {
            values = new Dictionary<string, string>(other.values);
        }

        public ConditionVariables(ConditionVariables other, ConditionVariables other2)      // other can be null, other2 must not be
        {
            if ( other == null )
                values = new Dictionary<string, string>(other2.values);
            else
            {
                values = new Dictionary<string, string>(other.values);
                Add(other2);
            }
        }

        public ConditionVariables(ConditionVariables other, ConditionVariables other2, ConditionVariables other3)
        {
            values = new Dictionary<string, string>(other.values);
            Add(other2);
            Add(other3);
        }

        public ConditionVariables(string s, FromMode fm)     //v=1,v=2 no brackets
        {
            FromString(s, fm);
        }

        public ConditionVariables(string s, string value)     
        {
            values[s] = value;
        }

        public ConditionVariables(string[] s) // name,value,name,value..
        {
            System.Diagnostics.Debug.Assert(s.Length % 2 == 0);
            for (int i = 0; i < s.Length; i+=2)
                values[s[i]] = s[i+1];
        }

        public string this[string s] { get { return values[s]; } set { values[s] = value; } }       // can be set NULL

        public int Count { get { return values.Count; } }

        public IEnumerable<string> NameEnumuerable { get { return values.Keys; } }
        public List<string> NameList { get { return values.Keys.ToList(); } }

        public bool Exists(string s) { return values.ContainsKey(s); }

        public void Clear() { values.Clear(); }

        public void Delete(string name)
        {
            if (values.ContainsKey(name))
                values.Remove(name);
        }

        public int GetInt(string name, int def = 0)     // get or default
        {
            int i;
            if (values.ContainsKey(name) && values[name].InvariantParse(out i))
                return i;
            else
                return def;
        }

        public string GetString(string name, string def = null, bool checklen = false)      // optional check length
        {
            if (values.ContainsKey(name))
            {
                if ( !checklen || values[name].Length>0 )
                    return values[name];
            }

            return def;
        }

        public void SetOrRemove(bool add, string name,  string value)     // Set it, or remove it
        {
            if (add)
                values[name] = value;
            else
                values.Remove(name);
        }

        // Print vars, if altops is passed in, you can output using alternate operators

        public string ToString(Dictionary<string, string> altops = null, string pad = "", bool bracket = false , string separ = "," , bool quoteit = true, string prefix = "")
        {
            string s = "";
            foreach (KeyValuePair<string, string> v in values)
            {
                if (s.Length > 0)
                    s += separ;

                string vs = (quoteit) ? v.Value.QuoteString(comma: true, bracket: bracket) : v.Value;

                if ( altops == null )
                    s += prefix + v.Key + pad + "=" + pad + vs;
                else
                {
                    System.Diagnostics.Debug.Assert(altops.ContainsKey(v.Key));
                    s += prefix + v.Key + pad + altops[v.Key] + pad + vs;
                }
            }

            return s;
        }

        public enum FromMode { SingleEntry, MultiEntryComma, MultiEntryCommaBracketEnds };

        public bool FromString(string s, FromMode fm)    // string, not bracketed.
        {
            BaseUtils.StringParser p = new BaseUtils.StringParser(s);
            return FromString(p, fm);
        }

        // FromMode controls where its stopped. 
        // namelimit limits allowable names
        // fixnamecase means make sure its in Titlecase
        // altops enables operators other than = to be used (set/let only) 

        public bool FromString(BaseUtils.StringParser p, FromMode fm, Dictionary<string,string> altops = null )
        {
            Dictionary<string, string> newvars = new Dictionary<string, string>();

            while (!p.IsEOL)
            {
                string varname = p.NextQuotedWord( "= ");

                if (varname == null)
                    return false;

                if (altops!=null)            // with extended ops, the ops are returned in the altops function, one per variable found
                {                           // used only with let and set..
                    if (varname.EndsWith("$+"))
                    {
                        varname = varname.Substring(0, varname.Length - 2);
                        altops[varname] = "$+=";
                    }
                    else if (varname.EndsWith("$"))
                    {
                        varname = varname.Substring(0, varname.Length - 1);
                        altops[varname] = "$=";
                    }
                    else if (varname.EndsWith("+"))
                    {
                        varname = varname.Substring(0, varname.Length - 1);
                        altops[varname] = "+=";
                    }
                    else
                    {                                           
                        altops[varname] = "=";              // varname is good, it ended with a = or space, default is =

                        bool dollar = p.IsCharMoveOn('$'); // check for varname space $+
                        bool add = p.IsCharMoveOn('+');

                        if (dollar && add)
                            altops[varname] = "$+=";
                        else if ( dollar )
                            altops[varname] = "$=";
                        else if ( add )
                            altops[varname] = "+=";
                    }
                }

                if (!p.IsCharMoveOn('='))
                    return false;

                string value = p.NextQuotedWord((fm == FromMode.SingleEntry) ? " " : (fm == FromMode.MultiEntryComma) ? ", " : ",) ");

                if (value == null)
                    return false;

                newvars[varname] = value;

                if (fm == FromMode.MultiEntryCommaBracketEnds && p.PeekChar() == ')')        // bracket, stop don't remove.. outer bit wants to check its there..
                {
                    values = newvars;
                    return true;
                }
                else if (fm == FromMode.SingleEntry && !p.IsEOL)        // single entry, must be eol now
                    return false;
                else if (!p.IsEOL && !p.IsCharMoveOn(','))   // if not EOL, but not comma, incorrectly formed list
                    return false;
            }

            values = newvars;
            return true;
        }

        public JArray ToJSONObject()
        {
            JArray jf = new JArray();

            foreach (KeyValuePair<string, string> v in values)
            {
                JObject j1 = new JObject();
                j1["var"] = v.Key;
                j1["value"] = v.Value;
                jf.Add(j1);
            }

            return jf;
        }

        public void FromJSONObject(JArray jf)
        {
            foreach (JObject jo in jf)
            {
                values[(string)jo["var"]] = (string)jo["value"];
            }
        }

        static public string flagRunAtRefresh = "RunAtRefresh;";            // ACTION DATA Flags, stored with action program name in events to configure it

        public string ToActionDataString(string flag)           // helpers to encode action data..
        {
            if (flag.Length > 0 && values.Count > 0)
                return flag + "," + ToString();
            else if (values.Count > 0)
                return ToString();
            else
                return flag;
        }

        public void FromActionDataString(string ad, out string flag)        // helpers to encode action data..
        {
            if (ad.IndexOf('=') == -1)      // no equals, no variables, all flags
            {
                flag = ad;
            }
            else
            {
                int comma = ad.IndexOf(',');

                if (comma == -1)      // no comma, no flags, all vars
                {
                    flag = "";
                    FromString(ad, ConditionVariables.FromMode.MultiEntryComma);
                }
                else
                {
                    flag = ad.Substring(0, comma);
                    FromString(ad.Substring(comma + 1), ConditionVariables.FromMode.MultiEntryComma);
                }
            }
        }

        public void Add(List<ConditionVariables> varlist)
        {
            foreach (ConditionVariables d in varlist)
            {
                if (d != null)
                {
                    foreach (KeyValuePair<string, string> v in d.values)   // plus event vars
                        values[v.Key] = v.Value;
                }
            }
        }

        public void Add(ConditionVariables d)
        {
            if (d != null)
            {
                foreach (KeyValuePair<string, string> v in d.values)   // plus event vars
                    values[v.Key] = v.Value;
            }
        }

        public ConditionVariables FilterVars(string filter)
        {
            int wildcard = filter.IndexOf('*');
            if (wildcard >= 0)
                filter = filter.Substring(0, wildcard);

            ConditionVariables ret = new ConditionVariables();

            foreach (KeyValuePair<string, string> k in values)
            {
                if ((wildcard >= 0 && k.Key.StartsWith(filter)) || k.Key.Equals(filter))
                    ret[k.Key] = k.Value;
            }

            return ret;
        }

        // all variables, expand out thru macro expander.  does not alter these ones
        public ConditionVariables ExpandAll(ConditionFunctions e, ConditionVariables vars, out string errlist)
        {
            errlist = null;

            ConditionVariables exp = new ConditionVariables();

            foreach( KeyValuePair<string,string> k in values)
            {
                if (e.ExpandString(values[k.Key], out errlist) == ConditionFunctions.ExpandResult.Failed)
                    return null;

                exp[k.Key] = errlist;
            }

            errlist = null;
            return exp;
        }

        public string AddToVar(string name, int add, int initial)       // DOES NOT set anything..
        {
            if (values.ContainsKey(name))
            {
                int i;
                if (values[name].InvariantParse(out i))
                {
                    return (i + add).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            return initial.ToString();
        }

        #region Object values to this class

        public bool GetValuesIndicated(Object o)                                            // get the ones set up in the class
        {
            Type jtype = o.GetType();

            foreach ( string k in values.Keys.ToList())
            {
                System.Reflection.PropertyInfo pi = jtype.GetProperty(k);
                if ( pi != null )
                {
                    System.Reflection.MethodInfo getter = pi.GetGetMethod();
                    AddDataOfType(getter.Invoke(o, null), pi.PropertyType, k, 0);
                }
                else
                {
                    System.Reflection.FieldInfo fi = jtype.GetField(k);
                    if ( fi != null )
                    {
                        AddDataOfType(fi.GetValue(o), fi.FieldType, k, 0);
                    }
                }
            }

            return true;
        }


        public void AddPropertiesFieldsOfClass( Object o, string prefix , Type[] propexcluded , int maxdepth )      // get all data in the class
        {
            Type jtype = o.GetType();

            foreach (System.Reflection.PropertyInfo pi in jtype.GetProperties())
            {
                if (pi.GetIndexParameters().GetLength(0) == 0 && (propexcluded == null || !propexcluded.Contains(pi.PropertyType)))      // only properties with zero parameters are called
                {
                    string name = prefix + pi.Name;
                    System.Reflection.MethodInfo getter = pi.GetGetMethod();
                    AddDataOfType(getter.Invoke(o, null), pi.PropertyType, name, maxdepth);
                }
            }

            foreach (System.Reflection.FieldInfo fi in jtype.GetFields())
            {
                string name = prefix + fi.Name;
                AddDataOfType(fi.GetValue(o), fi.FieldType, name, maxdepth);
            }
        }

        public void AddDataOfType(Object o, Type rettype, string name, int depth )
        {
            if (depth < 0 )      // 0, list, class, object, .. limit depth
                return;

            System.Globalization.CultureInfo ct = System.Globalization.CultureInfo.InvariantCulture;

            try // just to make sure a strange type does not barfe it
            {
                if (rettype.UnderlyingSystemType.FullName.Contains("System.Collections.Generic.Dictionary"))
                {
                    if (o == null)
                        values[name + "Count"] = "0";                           // we always get a NameCount so we can tell..
                    else
                    {
                        var data = (System.Collections.IDictionary)o;           // lovely to work out

                        values[name + "Count"] = data.Count.ToString(ct);       // purposely not putting a _ to distinguish it from the entries

                        foreach (Object k in data.Keys)
                        {
                            if (k is string)
                            {
                                Object v = data[k as string];
                                AddDataOfType(v, v.GetType(), name + "_" + (string)k, depth-1);
                            }
                        }
                    }
                }
                else if (rettype.UnderlyingSystemType.FullName.Contains("System.Collections.Generic.List"))
                {
                    if (o == null)
                        values[name + "Count"] = "0";                           // we always get a NameCount so we can tell..
                    else
                    {
                        var data = (System.Collections.IList)o;           // lovely to work out

                        values[name + "Count"] = data.Count.ToString(ct);       // purposely not putting a _ to distinguish it from the entries

                        for (int i = 0; i < data.Count; i++)
                        { 
                            AddDataOfType(data[i], data[i].GetType(), name + "[" + (i + 1).ToString(ct) + "]" , depth-1);
                        }
                    }
                }
                else if (rettype.IsArray)
                {
                    if (o == null)
                        values[name + "_Length"] = "0";                         // always get a length
                    else
                    {
                        Object[] array = (Object[])o;

                        values[name + "_Length"] = array.Length.ToString(ct);
                        for (int i = 0; i < array.Length; i++)
                        {
                            AddDataOfType(array[i], array[i].GetType(), name + "[" + i.ToString(ct) + "]" , depth-1);
                    }
                    }
                }
                else if (o == null)
                {
                    values[name] = "";
                }
                else if (o is string)     // string is a class, so intercept first
                {
                    values[name] = o as string;
                }
                else if (rettype.IsClass)
                {
                    foreach (System.Reflection.PropertyInfo pi in rettype.GetProperties())
                    {
                        if (pi.GetIndexParameters().GetLength(0) == 0 && pi.PropertyType.IsPublic)      // only properties with zero parameters are called
                        {
                            System.Reflection.MethodInfo getter = pi.GetGetMethod();
                            AddDataOfType(getter.Invoke(o, null), pi.PropertyType, name + "_" + pi.Name , depth-1);
                        }
                    }

                    foreach (System.Reflection.FieldInfo fi in rettype.GetFields())
                    {
                        AddDataOfType(fi.GetValue(o), fi.FieldType, name + "_" + fi.Name, depth-1);
                    }
                }
                else if (o is bool)
                {
                    values[name] = ((bool)o) ? "1" : "0";
                }
                else if (!rettype.IsGenericType || rettype.GetGenericTypeDefinition() != typeof(Nullable<>))
                {
                    var v = Convert.ChangeType(o, rettype);

                    if (v is Double)
                        values[name] = ((double)v).ToString(ct);
                    else if (v is int)
                        values[name] = ((int)v).ToString(ct);
                    else if (v is long)
                        values[name] = ((long)v).ToString(ct);
                    else if (v is DateTime)
                        values[name] = ((DateTime)v).ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-us"));
                    else
                        values[name] = v.ToString();
                }
                else
                {                                                               // generic, get value type
                    System.Reflection.PropertyInfo pvalue = rettype.GetProperty("Value");   // value is second property of a nullable class

                    Type nulltype = pvalue.PropertyType;    // its type and value are found..
                    var value = pvalue.GetValue(o);
                    AddDataOfType(value, nulltype, name, depth-1);         // recurse to decode it
                }
            }
            catch { }
        }

        #endregion
    }
}
