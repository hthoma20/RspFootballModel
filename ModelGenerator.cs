using System;

using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml;

namespace Model
{
    class Runner
    {
        static void Main(string[] args)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("model.xml");

            ModelParser parser = new ModelParser();

            Model model = parser.Parse(xmlDoc);

            (LanguageModelGenerator, string)[] outputConfigurations = {
                (new PydanticGenerator(), "./generated/rspmodel.py"),
                (new TypescriptGenerator(), "./generated/rspModel.ts")
            };

            foreach ((LanguageModelGenerator languageGenerator, string dest) in outputConfigurations) {
                ModelGenerator generator = new ModelGenerator(languageGenerator);
                using StreamWriter writer = new StreamWriter(dest);
                generator.GenerateModel(writer, model);
            }
        }
    }

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

    interface LanguageModelGenerator {
        void GenerateHeader(StreamWriter writer);
        void GenerateEnum(StreamWriter writer, EnumModel enumModel);
        void GenerateStruct(StreamWriter writer, StructModel structModel);
        void GenerateTaggedUnion(StreamWriter writed, TaggedUnionModel unionModel);
    }

    class ModelGenerator {
        private LanguageModelGenerator generator;

        public ModelGenerator(LanguageModelGenerator generator) {
            this.generator = generator;
        }

        public void GenerateModel(StreamWriter writer, Model model) {

            generator.GenerateHeader(writer);
            writer.WriteLine();

            foreach (TopLevelConstruct construct in model.Constructs) {
                switch (construct) {
                    case EnumModel enumModel:
                        generator.GenerateEnum(writer, enumModel);
                        break;
                    case StructModel structModel:
                        generator.GenerateStruct(writer, structModel);
                        break;
                    case TaggedUnionModel unionModel:
                        foreach (StructModel structModel in unionModel.Members) {
                            generator.GenerateStruct(writer, structModel);
                            writer.WriteLine();
                        }
                        generator.GenerateTaggedUnion(writer, unionModel);
                        break;
                    default:
                        throw new ArgumentException("Unexpected construct " + construct);
                }
                writer.WriteLine();
            }
        }

    }

}
