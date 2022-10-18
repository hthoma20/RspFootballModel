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

    public class Model {
        public IEnumerable<EnumModel> Enums;
    }
}