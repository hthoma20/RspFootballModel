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
    }

    class TypescriptGenerator : LanguageModelGenerator {
        public void GenerateHeader(StreamWriter writer) {
        }

        public void GenerateEnum(StreamWriter writer, EnumModel enumModel) {
            
            IEnumerable<string> quotedMembers = from member in enumModel.Members
                select $"'{member}'";
            string values = string.Join(" | ", quotedMembers);

            writer.WriteLine($"export type {enumModel.Name} = {values};");
        }
    }

}