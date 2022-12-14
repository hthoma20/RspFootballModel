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
                case BooleanType boolType:
                    return "bool";
                case ListType listType:
                    return $"list[{GetTypeString(listType.SubType)}]";
                case OptionalType optionalType:
                    return $"Optional[{GetTypeString(optionalType.SubType)}]";
                case PlayerMap playerMap:
                    return $"dict[Player, {GetTypeString(playerMap.SubType)}]";
                case ServerDefault serverDefault:
                    return $"{GetTypeString(serverDefault.SubType)} = {serverDefault.DefaultValue}";
                case TagType tagType:
                    return $"Literal['{tagType.Value}'] = '{tagType.Value}'";
            }
            throw new ArgumentException("Unknown type: " + type);
        }
    }

    class TypescriptGenerator : LanguageModelGenerator {

        private string TAB = "    ";

        public void GenerateHeader(StreamWriter writer) {
            writer.WriteLine("export type PlayerMap<T> = {'home': T, 'away': T}");
        }

        public void GenerateEnum(StreamWriter writer, EnumModel enumModel) {
            
            IEnumerable<string> quotedMembers = from member in enumModel.Members
                select $"'{member}'";
            string values = string.Join(", ", quotedMembers);

            string valuesName = $"_{enumModel.Name}Values";
            writer.WriteLine($"const {valuesName} = [{values}] as const;");
            writer.WriteLine($"export type {enumModel.Name} = typeof {valuesName}[number];");
            writer.WriteLine($"export function is{enumModel.Name}(value: string): value is {enumModel.Name} {{");
            writer.WriteLine($"{TAB}return {valuesName}.includes(value as any);");
            writer.WriteLine("}");
        }

        public void GenerateStruct(StreamWriter writer, StructModel structModel) {
            writer.WriteLine($"export type {structModel.Name} = {{");

            foreach (TypedMember member in structModel.Members) {
                string typeString = GetTypeString(member.Type);
                string nameQualifier = member.Type is ServerDefault ? "?" : "";
                writer.WriteLine($"{TAB}{member.Name}{nameQualifier}: {typeString};");
            }

            writer.WriteLine("};");
        }

        public void GenerateTaggedUnion(StreamWriter writer, TaggedUnionModel unionModel) {
            string unionString = string.Join(" | ", from member in unionModel.Members
                select member.Name);

            writer.WriteLine($"export type {unionModel.Name} = {unionString};");
        }

        private string GetTypeString(Type type) {
            switch (type) {
                case Identifier identifier:
                    return identifier.Name;
                case IntType intType:
                    return "number";
                case StringType stringType:
                    return "string";
                case BooleanType boolType:
                    return "boolean";
                case ListType listType:
                    return $"{GetTypeString(listType.SubType)}[]";
                case OptionalType optionalType:
                    return $"{GetTypeString(optionalType.SubType)} | null";
                case PlayerMap playerMap:
                    return $"PlayerMap<{GetTypeString(playerMap.SubType)}>";
                case ServerDefault serverDefault:
                    // Making this type optional is handled in the member name
                    return $"{GetTypeString(serverDefault.SubType)}";
                case TagType tagType:
                    return $"'{tagType.Value}'";
            }
            throw new ArgumentException("Unknown type: " + type);
        }
    }

}