﻿using System;

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
                (new TypescriptGenerator(), "./generated/gameModel.ts")
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
            return new Model {
                Enums = ParseEnums(model),
                Structs = ParseStructs(model),
                Unions = ParseUnions(model)
            };
        }

        private IEnumerable<EnumModel> ParseEnums(XmlNode xmlNode) {
            XmlNodeList enums = xmlNode.SelectNodes("Enum");

            return from enumNode in enums.Cast<XmlNode>()
                select ParseEnum(enumNode);
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

        private IEnumerable<StructModel> ParseStructs(XmlNode xmlNode) {
            XmlNodeList structs = xmlNode.SelectNodes("Struct");

            return from structNode in structs.Cast<XmlNode>()
                select ParseStruct(structNode);
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

        private IEnumerable<TaggedUnionModel> ParseUnions(XmlNode xmlNode) {
            XmlNodeList unions = xmlNode.SelectNodes("TaggedUnion");

            return from unionNode in unions.Cast<XmlNode>()
                select ParseUnion(unionNode);
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

            foreach (EnumModel enumModel in model.Enums) {
                generator.GenerateEnum(writer, enumModel);
                writer.WriteLine();
            }

            foreach (StructModel structModel in model.Structs) {
                generator.GenerateStruct(writer, structModel);
                writer.WriteLine();
            }

            foreach (TaggedUnionModel unionModel in model.Unions) {
                foreach (StructModel structModel in unionModel.Members) {
                    generator.GenerateStruct(writer, structModel);
                    writer.WriteLine();
                }
                generator.GenerateTaggedUnion(writer, unionModel);
                writer.WriteLine();
            }
        }

    }

}
