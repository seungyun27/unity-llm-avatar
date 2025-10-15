// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
// REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
// INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
// LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
// OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
// PERFORMANCE OF THIS SOFTWARE.

namespace UnityLLMAvatar.LLM
{
    public static class XMLGrammar
    {
        public static string Grammar => @"
# Document
root ::= element*

# White Space
S ::= ( ""\x20"" | ""\x09"" | ""\x0D"" | ""\x0A"" )+

# Names and Tokens
NameStartChar ::= "":"" | [A-Z] | ""_"" | [a-z] | [\xC0-\xD6] | [\xD8-\xF6] |
                  [\xF8-\u02FF]   | [\u0370-\u037D] | [\u037F-\u1FFF] |
		  [\u200C-\u2FEF] | [\u3001-\uD7FF] | [\uF900-\uFDCF] |
		  [\uFDF0-\uFFFD] | [\U00010000-\U000EFFFF]
NameChar ::= NameStartChar | ""-"" | ""."" | [0-9] | ""\xB7"" |
             [\u0300-\u036F] | [\u203F-\u2040]
Name ::= NameStartChar (NameChar)*
Names ::= Name (""\x20"" Name)*

# Literals
AttValue ::= ( ""\x22"" ([^<&\x22] | Reference)* ""\x22"" ) |
             ( ""\x27"" ([^<&\x27] | Reference)* ""\x27"" )

# Character Data
CharData ::= (("" ]]"" [^>]) | [^<&])*

# Prolog
Eq ::= S? ""="" S?

# Element
element ::= EmptyElemTag | (STag content ETag)

# Start-tag
STag ::= ""<"" Name (S Attribute)* S? "">""
Attribute ::= Name Eq AttValue

# End-tag
ETag ::= ""</"" Name S? "">""

# Content of Elements
content ::= CharData? ((element | Reference) CharData?)*

# Tags for Empty Elements
EmptyElemTag ::= ""<"" Name (S Attribute)* S? ""/>""

# Character Reference
CharRef ::= (""&#"" [0-9]+ "";"") | (""&#x"" [0-9a-fA-F]+ "";"")

# Entity Reference
Reference ::= EntityRef | CharRef
EntityRef ::= ""&"" Name "";""
";
    }
}