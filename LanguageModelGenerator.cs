using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Model {

    class PydanticGenerator : LanguageModelGenerator {

        private string TAB = "    ";

        public void GenerateHeader(StreamWriter writer) {
            writer.WriteLine("from enum import Enum");
            writer.WriteLine("from typing import Literal, Optional, Union");
            writer.WriteLine("from pydantic import BaseModel");
        }

        public void GenerateEnum(StreamWriter writer, EnumModel enumModel) {
            writer.WriteLine($"class {enumModel.Name}(str, Enum):");

            foreach (string member in enumModel.Members) {
                writer.WriteLine($"{TAB}{member} = '{member}'");
            }
        }

        public void GenerateStruct(StreamWriter writer, StructModel structModel) {
            writer.WriteLine($"class {structModel.Name}(BaseModel):");

            foreach (TypedMember member in structModel.Members) {
                string typeString = GetTypeString(member.Type);
                writer.WriteLine($"{TAB}{member.Name}: {typeString}");
            }
        }

        public void GenerateResult(StreamWriter writer, ResultModel resultModel) {
            writer.WriteLine($"class {resultModel.Name}(BaseModel):");
            writer.WriteLine($"{TAB}name: Literal['{resultModel.Tag}'] = '{resultModel.Tag}'");

            foreach (TypedMember member in resultModel.Members) {
                string typeString = GetTypeString(member.Type);
                writer.WriteLine($"{TAB}{member.Name}: {typeString}");
            }
        }

        public void GenerateAggregateResult(StreamWriter writer, IEnumerable<ResultModel> results) {
            string union = string.Join(", ", from result in results select result.Name);
            writer.WriteLine($"Result = Union[{union}]");
        }

        public void GenerateTaggedUnion(StreamWriter writer, TaggedUnionModel unionModel) {
            string unionString = string.Join(", ", from member in unionModel.Members
                select member.Name);

            writer.WriteLine($"{unionModel.Name} = Union[{unionString}]");
        }

        private string GetTypeString(Type type) {
            switch (type) {
                case Identifier identifier:
                    return identifier.Name;
                case IntType intType:
                    return "int";
                case StringType stringType:
                    return "str";
                case ListType listType:
                    return $"list[{GetTypeString(listType.SubType)}]";
                case TagType tagType:
                    return $"Literal[{tagType.Value}]";
            }
            throw new ArgumentException("Unknown type: " + type);
        }
    }

    class TypescriptGenerator : LanguageModelGenerator {

        private string TAB = "    ";

        public void GenerateHeader(StreamWriter writer) {}

        public void GenerateEnum(StreamWriter writer, EnumModel enumModel) {
            
            IEnumerable<string> quotedMembers = from member in enumModel.Members
                select $"'{member}'";
            string values = string.Join(" | ", quotedMembers);

            writer.WriteLine($"export type {enumModel.Name} = {values};");
        }

        public void GenerateStruct(StreamWriter writer, StructModel structModel) {
            writer.WriteLine($"export type {structModel.Name} = {{");

            foreach (TypedMember member in structModel.Members) {
                string typeString = GetTypeString(member.Type);
                writer.WriteLine($"{TAB}{member.Name}: {typeString};");
            }

            writer.WriteLine("};");
        }

        public void GenerateTaggedUnion(StreamWriter writer, TaggedUnionModel unionModel) {
            string unionString = string.Join(" | ", from member in unionModel.Members
                select member.Name);

            writer.WriteLine($"export type {unionModel.Name} = {unionString};");
        }


        public void GenerateResult(StreamWriter writer, ResultModel resultModel) {
            writer.WriteLine($"export type {resultModel.Name} = {{");
            writer.WriteLine($"{TAB}name: '{resultModel.Tag}';");

            foreach (TypedMember member in resultModel.Members) {
                string typeString = GetTypeString(member.Type);
                writer.WriteLine($"{TAB}{member.Name}: {typeString};");
            }

            writer.WriteLine("};");
        }

        public void GenerateAggregateResult(StreamWriter writer, IEnumerable<ResultModel> results) {
            string union = string.Join(" | ", from result in results select result.Name);
            writer.WriteLine($"export type Result = {union};");
            writer.WriteLine("export type ResultName = Result['name'];");
        }

        private string GetTypeString(Type type) {
            switch (type) {
                case Identifier identifier:
                    return identifier.Name;
                case IntType intType:
                    return "number";
                case StringType stringType:
                    return "string";
                case ListType listType:
                    return $"{GetTypeString(listType.SubType)}[]";
                case TagType tagType:
                    return $"'{tagType.Value}'";
            }
            throw new ArgumentException("Unknown type: " + type);
        }
    }

}