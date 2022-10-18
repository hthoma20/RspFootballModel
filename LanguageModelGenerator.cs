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

        private string GetTypeString(Type type) {
            switch (type) {
                case Identifier identifier:
                    return identifier.Name;
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
            }
            throw new ArgumentException("Unknown type: " + type);
        }
    }

}