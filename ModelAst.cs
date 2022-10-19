using System;

using System.Collections.Generic;
using System.Linq;
using System.Xml;


namespace Model {

    public class EnumModel {
        public string Name;
        public IEnumerable<String> Members;

        public override string ToString() {
            return "EnumModel<" + Name + ":" + string.Join("|", Members) + ">";
        }
    }

    public class TypedMember {
        public string Name;
        public Type Type;
    }

    public class StructModel {
        public string Name;
        public IEnumerable<TypedMember> Members;
    }

    public class ResultModel {
        public string Name;
        public string Tag;
        public IEnumerable<TypedMember> Members;
    }

    public class Model {
        public IEnumerable<EnumModel> Enums;
        public IEnumerable<ResultModel> Results;
        public IEnumerable<StructModel> Structs;
    }



    public interface Type {}

    public class Identifier : Type {
        public string Name;
    }

    public class IntType : Type {}

    public class ListType : Type {
        public Type SubType;
    }
}