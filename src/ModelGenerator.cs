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
