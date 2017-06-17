using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using C5;
using ModIso8583.Codecs;
using ModIso8583.Util;
using Serilog;
using Serilog.Events;
using Logger = Serilog.Core.Logger;

namespace ModIso8583.Parse
{
    /// <summary>
    ///     This class is used to parse a XML configuration file and configure
    ///     a MessageFactory with the values from it.
    /// </summary>
    public static class ConfigParser
    {
        private static readonly Logger logger = new LoggerConfiguration().WriteTo.ColoredConsole().CreateLogger();

        public static MessageFactory<IsoMessage> createDefault() { throw new NotImplementedException(); }

        private static void ParseHeaders<T>(XmlNodeList nodes,
            MessageFactory<T> mfact) where T : IsoMessage
        {
            C5.ArrayList<XmlElement> refs = null;
            for (var i = 0; i < nodes.Count; i++)
            {
                var elem = (XmlElement) nodes.Item(i);
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
                var elem = (XmlElement) nodes.Item(i);
                if (elem == null) continue;
                var type = ParseType(elem.GetAttribute("type"));
                if (type == -1) throw new IOException("Invalid ISO8583 type for template: " + elem.GetAttribute("type"));
                if (elem.GetAttribute("extends") != null && !elem.GetAttribute("extends").IsEmpty())
                {
                    if (subs == null) subs = new ArrayList<XmlElement>(nodes.Count - i);
                    subs.Add(elem);
                    continue;
                }
                var m = (T) new IsoMessage();
                m.Type = type;
                m.Encoding = mfact.Encoding;
                var fields = elem.GetElementsByTagName("field");
                for (var j = 0; j < fields.Count; j++)
                {
                    var f = (XmlElement) fields.Item(j);
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
                    var sub = (XmlElement) subs.Item(j);
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
                    var sf = (XmlElement) subs.Item(i);
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
    }
}