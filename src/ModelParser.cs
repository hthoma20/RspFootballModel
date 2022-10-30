using System;

using System.Linq;
using System.Collections.Generic;
using System.Xml;

namespace Model {
    class ModelParser {
        public Model Parse(XmlDocument xmlDoc) {
            XmlNode model = xmlDoc.FirstChild;

            var constructs = from XmlNode node in model
                    select ParseConstruct(node);

            return new Model {
                Constructs = constructs
            };
        }

        private TopLevelConstruct ParseConstruct(XmlNode node) {
            switch (node.Name) {
                case "Enum":
                    return ParseEnum(node);
                case "Struct":
                    return ParseStruct(node);
                case "TaggedUnion":
                    return ParseUnion(node);
            }
            throw new ArgumentException("Unexpected top-level node " + node.Name);
        }

        private EnumModel ParseEnum(XmlNode node) {
            string name = node.Attributes["name"].Value;
            
            XmlNodeList memberNodes = node.SelectNodes("member");
            IEnumerable<string> members = from memberNode in memberNodes.Cast<XmlNode>()
                select memberNode.InnerText.Trim();

            return new EnumModel {
                Name = name,
                Members = members
            };
        }

        private StructModel ParseStruct(XmlNode node) {
            string name = node.Attributes["name"].Value;
            
            XmlNodeList memberNodes = node.SelectNodes("member");
            IEnumerable<TypedMember> members = from memberNode in memberNodes.Cast<XmlNode>()
                select ParseTypedMember(memberNode);

            return new StructModel {
                Name = name,
                Members = members
            };
        }

        private TaggedUnionModel ParseUnion(XmlNode node) {
            string name = node.Attributes["name"].Value;
            string tagKey = node.Attributes["tagKey"].Value;

            IEnumerable<StructModel> members = from structNode in node.SelectNodes("Struct").Cast<XmlNode>()
                select ParseUnionStruct(structNode, tagKey);

            return new TaggedUnionModel {
                Name = name,
                TagKey = tagKey,
                Members = members
            };
        }

        private StructModel ParseUnionStruct(XmlNode structNode, string tagKey) {
            string tag = structNode.Attributes["tag"].Value;
            
            StructModel structModel = ParseStruct(structNode);
            
            var members = new List<TypedMember>(structModel.Members);
            members.Insert(0, new TypedMember{
                Name = tagKey,
                Type = new TagType{ Value = tag }
            });

            structModel.Members = members;

            return structModel;
        }

        private TypedMember ParseTypedMember(XmlNode node) {
            string name = node.SelectSingleNode("name").InnerText.Trim();
            Type type = ParseType(node.SelectSingleNode("type").FirstChild);

            return new TypedMember {
                Name = name,
                Type = type
            };
        }

        private Type ParseType(XmlNode node) {
            switch (node.Name) {
                case "#text":
                    return new Identifier {
                        Name = node.Value
                    };
                case "int":
                    return new IntType();
                case "string":
                    return new StringType();
                case "boolean":
                    return new BooleanType();
                case "list":
                    return new ListType {
                        SubType = ParseType(node.FirstChild)
                    };
                case "optional":
                    return new OptionalType {
                        SubType = ParseType(node.FirstChild)
                    };
                case "playermap":
                    return new PlayerMap {
                        SubType = ParseType(node.FirstChild)
                    };
                case "serverdefault":
                    return new ServerDefault {
                        SubType = ParseType(node.SelectSingleNode("type").FirstChild),
                        DefaultValue = node.SelectSingleNode("default").InnerText.Trim()
                    };
            }

            throw new ArgumentException("Unknown type node: " + node.Name);
        }
    }
}