using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAI_Tools.Frostbite;

namespace DAI_Tools.Search
{
    public static class TextSerialization
    {
        public static char INDENT_CHAR = ' ';
        private static string NL = Environment.NewLine;
        /* todo: write partials info, indent+1 */
        public static string toText(this EbxDataContainers containers)
        {
            var sb = new StringBuilder();
            foreach(var instance in containers.instances.Values)
                convertToText(instance.guid, containers.getFlattenedDataFor(instance.guid), 0, sb);
            return sb.ToString();
        }

        public static void convertToText(string fieldName, AValue avalue, int indent, StringBuilder sb)
        {
            if (avalue == null)
            {
                appendValue(fieldName, "null", indent, sb);
                return;
            }
            
            switch (avalue.Type)
            {
                case ValueTypes.SIMPLE:
                    var asimpleval = avalue.castTo<ASimpleValue>();
                    appendValue(fieldName, strB =>
                        {
                            strB.Append(asimpleval.Val);
                            if (asimpleval.unhashed != null) strB.Append(" / ").Append(asimpleval.unhashed);
                        }, indent, sb);
                    break;
                case ValueTypes.NULL_REF:
                    appendTypeFieldname(fieldName, "NullRef", indent, sb);
                    break;
                case ValueTypes.IN_REF:
                    var ainref = avalue.castTo<AIntRef>();
                    appendTypeFieldname(fieldName, "InRef", indent, sb);
                    appendValue("instance", ainref.instanceGuid, indent+1, sb);
                    appendValue("status", ainref.refStatus.ToString(), indent+1, sb);
                    break;
                case ValueTypes.EX_REF:
                    var aexref = avalue.castTo<AExRef>();
                    appendTypeFieldname(fieldName, "ExRef", indent, sb);
                    appendValue("ebx", aexref.fileGuid, indent+1, sb);
                    appendValue("instance", aexref.instanceGuid, indent+1, sb);
                    appendValue("refName", aexref.refName, indent+1, sb);
                    appendValue("refType", aexref.refName, indent+1, sb);
                    break;
                case ValueTypes.STRUCT:
                    var astruct = avalue.castTo<AStruct>();
                    appendTypeFieldname(fieldName, astruct.name, indent, sb);
                    
                    foreach (var childField in astruct.fields)
                        convertToText(childField.Key, childField.Value, indent+1, sb);

                    break;
                case ValueTypes.ARRAY:
                    var aarray = avalue.castTo<AArray>().elements;
                    appendTypeFieldname(fieldName, "array", indent, sb);

                    for(int i = 0; i < aarray.Count; i++)
                        convertToText(i.ToString(), aarray[i], indent+1, sb);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void appendTypeFieldname(string fieldName, string type, int indent, StringBuilder sb)
        {
            sb.Append(INDENT_CHAR, indent).Append(fieldName).Append(" [").Append(type).Append("]:").Append(NL);
        }

        public static void appendValue(string fieldName, string value, int indent, StringBuilder sb)
        {
            sb.Append(INDENT_CHAR, indent).Append(fieldName).Append(": ").Append(value).Append(NL);
        }

        public static void appendValue(string fieldName, Action<StringBuilder> valueProvider, int indent, StringBuilder sb)
        {
            sb.Append(INDENT_CHAR, indent).Append(fieldName).Append(": ");
            valueProvider(sb);
            sb.Append(NL);
        }
    }
}
