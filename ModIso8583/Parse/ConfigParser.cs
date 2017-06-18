﻿using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using C5;
using ModIso8583.Codecs;
using ModIso8583.Util;
using Serilog;
using Serilog.Events;
using Logger = Serilog.Core.Logger;
using System.Net.Http;
using System.Net;

namespace ModIso8583.Parse
{
    /// <summary>
    ///     This class is used to parse a XML configuration file and configure
    ///     a MessageFactory with the values from it.
    /// </summary>
    public static class ConfigParser
    {
        private static readonly Logger logger = new LoggerConfiguration().WriteTo.ColoredConsole().CreateLogger();

        /// <summary>
        /// Creates a message factory configured from the default file, which is n8583.xml
        /// located in the app domain base directory.
        /// </summary>
        /// <returns>The default.</returns>
        public static MessageFactory<IsoMessage> CreateDefault()
        {
            MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();
            ConfigureFromDefault(mfact);
            return mfact;
        }

        /// <summary>
        /// Creates a message factory from the file located at the specified URL
        /// </summary>
        /// <returns>The url of the config file</returns>
        /// <param name="url">URL.</param>
        public static async System.Threading.Tasks.Task<MessageFactory<IsoMessage>> CreateFromUrlAsync(Uri url)
        {
            MessageFactory<IsoMessage> mfact = new MessageFactory<IsoMessage>();
            await ConfigureFromUrlAsync(mfact, url);
            return mfact;
        }

        private static void ParseHeaders<T>(XmlNodeList nodes,
            MessageFactory<T> mfact) where T : IsoMessage
        {
            C5.ArrayList<XmlElement> refs = null;
            for (var i = 0; i < nodes.Count; i++)
            {
                var elem = (XmlElement)nodes.Item(i);
                if (elem != null)
                {
                    var type = ParseType(elem.GetAttribute("type"));
                    if (type == -1) throw new IOException($"Invalid type {elem.GetAttribute("type")} for ISO8583 header: ");
                    if (elem.ChildNodes == null || elem.ChildNodes.Count == 0)
                    {
                        if (elem.GetAttribute("ref") != null && !string.IsNullOrEmpty(elem.GetAttribute("ref")))
                        {
                            if (refs == null) refs = new ArrayList<XmlElement>(nodes.Count - i);
                            refs.Add(elem);
                        }
                        else { throw new IOException("Invalid ISO8583 header element"); }
                    }
                    else
                    {
                        var header = elem.ChildNodes.Item(0).Value;
                        var binHeader = "true".Equals(elem.GetAttribute("binary"));
                        if (logger.IsEnabled(LogEventLevel.Debug))
                        {
                            var binary = binHeader ? "binary" : string.Empty;

                            logger.Debug($"Adding {binary} ISO8583 header for type {elem.GetAttribute("type")} : {header}");
                        }
                        if (binHeader)
                            mfact.SetBinaryIsoHeader(type,
                                HexCodec.HexDecode(header));
                        else
                            mfact.SetIsoHeader(type,
                                header);
                    }
                }
            }
            if (refs == null) return;
            {
                foreach (var elem in refs)
                {
                    if (elem == null) continue;
                    var type = ParseType(elem.GetAttribute("type"));
                    if (type == -1) throw new IOException("Invalid type for ISO8583 header: " + elem.GetAttribute("type"));
                    if (elem.GetAttribute("ref") == null || elem.GetAttribute("ref").IsEmpty()) continue;
                    var t2 = ParseType(elem.GetAttribute("ref"));
                    if (t2 == -1) throw new IOException("Invalid type reference " + elem.GetAttribute("ref") + " for ISO8583 header " + type);
                    var h = mfact.GetIsoHeader(t2);
                    if (h == null) throw new ArgumentException("Header def " + type + " refers to nonexistent header " + t2);
                    if (logger.IsEnabled(LogEventLevel.Debug))
                        logger.Debug("Adding ISO8583 header for type {Type}: {H} (copied from {Ref})",
                            elem.GetAttribute("type"),
                            h,
                            elem.GetAttribute("ref"));
                    mfact.SetIsoHeader(type,
                        h);
                }
            }
        }

        private static void ParseTemplates<T>(XmlNodeList nodes,
            MessageFactory<T> mfact) where T : IsoMessage
        {
            ArrayList<XmlElement> subs = null;
            for (var i = 0; i < nodes.Count; i++)
            {
                var elem = (XmlElement)nodes.Item(i);
                if (elem == null) continue;
                var type = ParseType(elem.GetAttribute("type"));
                if (type == -1) throw new IOException("Invalid ISO8583 type for template: " + elem.GetAttribute("type"));
                if (elem.GetAttribute("extends") != null && !elem.GetAttribute("extends").IsEmpty())
                {
                    if (subs == null) subs = new ArrayList<XmlElement>(nodes.Count - i);
                    subs.Add(elem);
                    continue;
                }
                var m = (T)new IsoMessage();
                m.Type = type;
                m.Encoding = mfact.Encoding;
                var fields = elem.GetElementsByTagName("field");
                for (var j = 0; j < fields.Count; j++)
                {
                    var f = (XmlElement)fields.Item(j);
                    if (f.ParentNode != elem) continue;
                    var num = int.Parse(f.GetAttribute("num"));
                    var v = GetTemplateField(f,
                        mfact,
                        true);
                    if (v != null) v.Encoding = mfact.Encoding;
                    m.SetField(num,
                        v);
                }
                mfact.AddMessageTemplate(m);
            }
        }

        private static int ParseType(string type)
        {
            if (type.Length % 2 == 1) type = "0" + type;
            if (type.Length != 4) return -1;
            return ((type[0] - 48) << 12) | ((type[1] - 48) << 8) | ((type[2] - 48) << 4) | (type[3] - 48);
        }

        /// <summary>
        /// Creates an IsoValue from the XML definition in a message template.
        /// If it's for a toplevel field and the message factory has a codec for this field,
        /// that codec is assigned to that field. For nested fields, a CompositeField is
        /// created and populated.
        /// </summary>
        /// <returns>The template field.</returns>
        /// <param name="f">xml element</param>
        /// <param name="mfact">message factory</param>
        /// <param name="toplevel">If set to <c>true</c> toplevel.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        private static IsoValue GetTemplateField<T>(XmlElement f,
            MessageFactory<T> mfact,
            bool toplevel) where T : IsoMessage
        {
            var num = int.Parse(f.GetAttribute("num"));
            var typedef = f.GetAttribute("type");
            if ("exclude".Equals(typedef)) return null;
            var length = 0;
            if (f.GetAttribute("length").Length > 0) length = int.Parse(f.GetAttribute("length"));
            var itype = Enumm.Parse<IsoType>(typedef);
            var subs = f.GetElementsByTagName("field");
            if (subs != null && subs.Count > 0)
            {
                var cf = new CompositeField();
                for (var j = 0; j < subs.Count; j++)
                {
                    var sub = (XmlElement)subs.Item(j);
                    if (sub != null && sub.ParentNode != f) continue;
                    var sv = GetTemplateField(sub,
                        mfact,
                        false);
                    if (sv == null) continue;
                    sv.Encoding = mfact.Encoding;
                    cf.AddValue(sv);
                }
                Debug.Assert(itype != null,
                    "itype != null");
                return itype.Value.NeedsLength() ? new IsoValue(itype.Value,
                    cf,
                    length,
                    cf) : new IsoValue(itype.Value,
                    cf,
                    cf);
            }
            var v = f.ChildNodes.Count == 0 ? string.Empty : f.ChildNodes.Item(0).Value;
            var customField = toplevel ? mfact.GetCustomField(num) : null;
            if (customField != null)
            {
                Debug.Assert(itype != null,
                    "itype != null");
                return itype.Value.NeedsLength() ? new IsoValue(itype.Value,
                    customField.DecodeField(v),
                    length,
                    customField) : new IsoValue(itype.Value,
                    customField.DecodeField(v),
                    customField);
            }
            Debug.Assert(itype != null,
                "itype != null");
            return itype.Value.NeedsLength() ? new IsoValue(itype.Value,
                v,
                length) : new IsoValue(itype.Value,
                v);
        }

        private static FieldParseInfo GetParser<T>(XmlElement f,
            MessageFactory<T> mfact) where T : IsoMessage
        {
            var itype = Enumm.Parse<IsoType>(f.GetAttribute("type"));
            var length = 0;
            if (f.GetAttribute("length").Length > 0) length = int.Parse(f.GetAttribute("length"));
            Debug.Assert(itype != null,
                "itype != null");
            var fpi = FieldParseInfo.GetInstance(itype.Value,
                length,
                mfact.Encoding);
            var subs = f.GetElementsByTagName("field");
            if (subs != null && subs.Count > 0)
            {
                var combo = new CompositeField();
                for (var i = 0; i < subs.Count; i++)
                {
                    var sf = (XmlElement)subs.Item(i);
                    Debug.Assert(sf != null,
                        "sf != null");
                    if (sf.ParentNode == f)
                        combo.AddParser(GetParser(sf,
                            mfact));
                }
                fpi.Decoder = combo;
            }
            return fpi;
        }

        private static void ParseGuides<T>(XmlNodeList nodes,
            MessageFactory<T> mfact) where T : IsoMessage
        {
            ArrayList<XmlElement> subs = null;
            HashDictionary<int, HashDictionary<int, FieldParseInfo>> guides = new HashDictionary<int, HashDictionary<int, FieldParseInfo>>();
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlElement elem = (XmlElement)nodes.Item(i);
                if (elem != null)
                {
                    int type = ParseType(elem.GetAttribute("type"));
                    if (type == -1)
                    {
                        throw new IOException("Invalid ISO8583 type for parse guide: " + elem.GetAttribute("type"));
                    }
                    if (elem.GetAttribute("extends") != null && !elem.GetAttribute("extends").IsEmpty())
                    {
                        if (subs == null)
                        {
                            subs = new ArrayList<XmlElement>(nodes.Count - i);
                        }
                        subs.Add(elem);
                        continue;
                    }
                    HashDictionary<int, FieldParseInfo> parseMap = new HashDictionary<int, FieldParseInfo>();
                    XmlNodeList fields = elem.GetElementsByTagName("field");
                    for (int j = 0; j < fields.Count; j++)
                    {
                        XmlElement f = (XmlElement)fields.Item(j);
                        if (f != null && f.ParentNode == elem)
                        {
                            int num = int.Parse(f.GetAttribute("num"));
                            parseMap.Add(num, GetParser(f, mfact));
                        }
                    }

                    mfact.SetParseMap(type, parseMap);
                    guides.Add(type, parseMap);
                }
            }
            if (subs != null)
            {
                foreach (XmlElement elem in subs)
                {
                    int type = ParseType(elem.GetAttribute("type"));
                    int @ref = ParseType(elem.GetAttribute("extends"));
                    if (@ref == -1)
                    {
                        throw new ArgumentException("Message template "
                                + elem.GetAttribute("type") + " extends invalid template "
                                + elem.GetAttribute("extends"));
                    }
                    HashDictionary<int, FieldParseInfo> parent = guides[@ref];
                    if (parent == null)
                    {
                        throw new ArgumentException("Parsing guide "
                                + elem.GetAttribute("type") + " extends nonexistent guide "
                                + elem.GetAttribute("extends"));
                    }
                    HashDictionary<int, FieldParseInfo> child = new HashDictionary<int, FieldParseInfo>();
                    child.AddAll(parent);
                    ArrayList<XmlElement> fields = GetDirectChildrenByTagName(elem, "field");
                    foreach (XmlElement f in fields)
                    {
                        int num = int.Parse(f.GetAttribute("num"));
                        string typedef = f.GetAttribute("type");
                        if ("exclude".Equals(typedef))
                        {
                            child.Remove(num);
                        }
                        else
                        {
                            child.Add(num, GetParser(f, mfact));
                        }
                    }
                    mfact.SetParseMap(type, child);
                    guides.Add(type, child);
                }
            }
        }

        private static ArrayList<XmlElement> GetDirectChildrenByTagName(XmlElement elem, string tagName)
        {
            ArrayList<XmlElement> childElementsByTagName = new ArrayList<XmlElement>();
            var childNodes = elem.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                if (childNodes.Item(i).NodeType == XmlNodeType.Element)
                {
                    XmlElement childElem = (XmlElement)childNodes.Item(i);
                    if (childElem.Name.Equals(tagName))
                    {
                        childElementsByTagName.Add(childElem);
                    }
                }
            }
            return childElementsByTagName;
        }

        /// <summary>
        /// Reads the XML from the stream and configures the message factory with its values.
        /// </summary>
        /// <returns></returns>
        /// <param name="mfact">The message factory to be configured with the values read from the XML.</param>
        /// <param name="source">The InputSource containing the XML configuration</param>
        /// <typeparam name="T"></typeparam>
        private static void Parse<T>(MessageFactory<T> mfact, Stream source) where T : IsoMessage
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(source);
                XmlElement root = xmlDoc.DocumentElement;
                ParseHeaders(root.GetElementsByTagName("header"), mfact);
                ParseTemplates(root.GetElementsByTagName("template"), mfact);
                //Read the parsing guides
                ParseGuides(root.GetElementsByTagName("parse"), mfact);
            }
            catch (Exception e)
            {
                logger.Error($"ISO8583 Cannot parse XML configuration {e.ToString()}");
                return;
            }
        }

        /// <summary>
        /// Configures a MessageFactory using the configuration file at the path specified (will be searched
        /// from the application domain
        /// </summary>
        /// <param name="mfact">The message factory to be configured with the values read from the XML.</param>
        /// <param name="path">Path.</param>
        /// <typeparam name="T"></typeparam>
        public static void ConfigureFromClasspathConfig<T>(MessageFactory<T> mfact, string path) where T : IsoMessage
        {
            try
            {
                string f = AppDomain.CurrentDomain.BaseDirectory + path;
                using (FileStream fsSource = new FileStream(f, FileMode.Open, FileAccess.Read))
                {
                    logger.Debug("ISO8583 Parsing config from classpath file {Path}", path);
                    Parse(mfact, fsSource);
                }
            }
            catch (FileNotFoundException)
            {
                logger.Warning("ISO8583 File not found in classpath: {Path}", path);
            }
        }

        /// <summary>
        /// This method attempts to open a stream from the XML configuration in the specified URL and
        /// configure the message factory from that config.
        /// </summary>
        /// <param name="mfact">The message factory to be configured with the values read from the XML.</param>
        /// <param name="url">The URL of the config file</param>
        /// <typeparam name="T"></typeparam>
        public static async System.Threading.Tasks.Task ConfigureFromUrlAsync<T>(MessageFactory<T> mfact, Uri url) where T : IsoMessage
        {
            try
            {
                HttpClient client = new HttpClient();
                using (client)
                {
                    var stream = await client.GetStreamAsync(url);
                    logger.Debug("ISO8583 Parsing config from classpath file {Path}", url.ToString());
                    Parse(mfact, stream);
                }
            }
            catch (Exception e)
            {
                logger.Warning("ISO8583 File not found in classpath: {Path}", url.ToString());
                throw e;
            }
        }

        /// <summary>
        /// Configures a MessageFactory using the default configuration file n8583.xml.
        /// This is useful if you have a MessageFactory created
        /// </summary>
        /// <param name="mfact">The message factory to be configured with the values read from the XML.</param>
        /// <typeparam name="T"></typeparam>
        public static void ConfigureFromDefault<T>(MessageFactory<T> mfact) where T : IsoMessage
        {
            string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "n8583.xml");
            if (!File.Exists(configFile))
            {
                logger.Warning("ISO8583 config file n8583.xml not found!");
                throw new FileNotFoundException("n8583.xml not found!");
            }
            try
            {
                using (FileStream fsSource = new FileStream(configFile, FileMode.Open, FileAccess.Read))
                {
                    logger.Debug("ISO8583 Parsing config from classpath file {Path}", configFile);
                    Parse(mfact, fsSource);
                }
            }
            catch (Exception e)
            {
                logger.Error("Error while parsing the config file" + e.ToString());
                throw e;
            }
        }
    }
}