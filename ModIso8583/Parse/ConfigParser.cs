using System;
using System.IO;
using System.Xml;
using C5;
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

        private static int ParseType(string type)
        {
            if (type.Length % 2 == 1) type = "0" + type;
            if (type.Length != 4) return -1;
            return ((type[0] - 48) << 12) | ((type[1] - 48) << 8) | ((type[2] - 48) << 4) | (type[3] - 48);
        }
    }
}