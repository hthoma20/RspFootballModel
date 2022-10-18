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
                Enums = ParseEnums(xmlDoc)
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
    }

    interface LanguageModelGenerator {
        void GenerateHeader(StreamWriter writer);
        void GenerateEnum(StreamWriter writer, EnumModel enumModel);
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
        }

    }

}
