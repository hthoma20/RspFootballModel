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
            return new Model {
                Enums = ParseEnums(xmlDoc),
                Results = ParseResults(xmlDoc),
                Structs = ParseStructs(xmlDoc)
            };
        }

        private IEnumerable<EnumModel> ParseEnums(XmlDocument xmlDoc) {
            XmlNodeList enums = xmlDoc.GetElementsByTagName("Enum");

            return from enumNode in enums.Cast<XmlNode>()
                select ParseEnum(enumNode);
        }

        private EnumModel ParseEnum(XmlNode node) {
            string name = node.Attributes["name"].Value;
            
            XmlNodeList memberNodes = node.SelectNodes("member");
            IEnumerable<string> members = from memberNode in memberNodes.Cast<XmlNode>()
                select memberNode.InnerText;

            return new EnumModel {
                Name = name,
                Members = members
            };
        }

        private IEnumerable<StructModel> ParseStructs(XmlDocument xmlDoc) {
            XmlNodeList structs = xmlDoc.GetElementsByTagName("Struct");

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

        private IEnumerable<ResultModel> ParseResults(XmlDocument xmlDoc) {
            XmlNodeList results = xmlDoc.GetElementsByTagName("Result");

            return from resultNode in results.Cast<XmlNode>()
                select ParseResult(resultNode);
        }

        private ResultModel ParseResult(XmlNode node) {
            string name = node.Attributes["name"].Value;
            string tag = node.Attributes["tag"].Value;
            
            XmlNodeList memberNodes = node.SelectNodes("member");
            IEnumerable<TypedMember> members = from memberNode in memberNodes.Cast<XmlNode>()
                select ParseTypedMember(memberNode);

            return new ResultModel {
                Name = name,
                Tag = tag,
                Members = members
            };
        }

        private TypedMember ParseTypedMember(XmlNode node) {
            string name = node.SelectSingleNode("name").InnerText;
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
            }

            throw new ArgumentException("Unknown type node: " + node.Name);
        }
    }

    interface LanguageModelGenerator {
        void GenerateHeader(StreamWriter writer);
        void GenerateEnum(StreamWriter writer, EnumModel enumModel);
        void GenerateStruct(StreamWriter writer, StructModel structModel);
        void GenerateResult(StreamWriter writer, ResultModel resultModel);
        void GenerateAggregateResult(StreamWriter writer, IEnumerable<ResultModel> results);
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

            foreach (ResultModel resultModel in model.Results) {
                generator.GenerateResult(writer, resultModel);
                writer.WriteLine();
            }
            generator.GenerateAggregateResult(writer, model.Results);
        }

    }

}
