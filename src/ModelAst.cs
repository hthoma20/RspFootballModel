using System;

using System.Collections.Generic;
using System.Linq;
using System.Xml;


namespace Model {

    public interface TopLevelConstruct {
        string Name {get;}
    }

    public class EnumModel : TopLevelConstruct {
        public string Name {get; set;}
        public IEnumerable<String> Members;
    }

    public class TypedMember {
        public string Name;
        public Type Type;
    }

    public class StructModel : TopLevelConstruct {
        public string Name {get; set;}
        public IEnumerable<TypedMember> Members;
    }

    public class TaggedUnionModel : TopLevelConstruct {
        public string Name {get; set;}
        public string TagKey;
        public IEnumerable<StructModel> Members;
    }

    public class Model {
        public IEnumerable<TopLevelConstruct> Constructs;
    }

    public static class TopLevelConstructExtensions {
        // Return the construct with the given name, or null if the name is not found
        public static TopLevelConstruct FindConstruct(this IEnumerable<TopLevelConstruct> constructs, string name) {
            foreach (TopLevelConstruct construct in constructs) {
                if (construct.Name == name) {
                    return construct;
                }
            }
            return null;
        }   
    }
        

    public interface Type {}

    public class Identifier : Type {
        public string Name;
    }

    public class IntType : Type {}

    public class StringType : Type {}

    public class BooleanType : Type {}

    public class ListType : Type {
        public Type SubType;
    }

    public class OptionalType : Type {
        public Type SubType;
    }

    public class PlayerMap : Type {
        public Type SubType;
    }

    public class ServerDefault : Type {
        public Type SubType;
        public string DefaultValue;
    }

    // Used for TaggedUnions
    public class TagType : Type {
        public string Value;
    }
}