using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace TaiwuUtils
{
    /// <summary>
    /// 可序列化字典
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public HashSet<TKey> IgnoreKeys { get; } = new HashSet<TKey>();
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            if (reader.IsEmptyElement)
                return;

            reader.Read();
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Pair");

                reader.ReadStartElement("Key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("Val");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            foreach (var pair in this)
            {
                if (IgnoreKeys.Contains(pair.Key))
                    continue;

                writer.WriteStartElement("Pair");

                writer.WriteStartElement("Key");
                keySerializer.Serialize(writer, pair.Key);
                writer.WriteEndElement();

                writer.WriteStartElement("Val");
                valueSerializer.Serialize(writer, pair.Value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
    }
}