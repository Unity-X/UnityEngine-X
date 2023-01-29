using System;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngineX;

namespace UnityEditorX
{
    public static class CodeParsing
    {
        public struct TermIterator
        {
            private readonly string _text;
            private int _i;

            /// <summary>
            /// according to https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/
            /// </summary>
            private static readonly string[] s_multiLetterOperators = new string[]
            {
                // NB: IT IS IMPORTANT TO ORDER THEM BY LENGTH
                "<<=",
                ">>=",
                "??=",
                "++",
                "--",
                "->",
                "..",
                "<<",
                ">>",
                ">=",
                "<=",
                "==",
                "!=",
                "&&",
                "||",
                "+=",
                "-=",
                "*=",
                "/=",
                "%=",
                "&=",
                "|=",
                "^=",
                "=>",
            };

            public TermIterator(string text) : this()
            {
                _text = text ?? throw new ArgumentNullException(nameof(text));
                _i = 0;
            }

            public TermIterator(string text, bool considerSpecialCharactersTerms) : this()
            {
                _text = text ?? throw new ArgumentNullException(nameof(text));
                _i = 0;
                ConsiderSpecialCharactersTerms = considerSpecialCharactersTerms;
            }

            public bool ConsiderSpecialCharactersTerms { get; set; }

            public string Current { get; private set; }

            public TermIterator GetEnumerator() => this;

            public bool MoveNext()
            {
                bool inCommentBlock = false;
                bool inCommentedLine = false;

                int termBegin = -1;
                bool termIsMadeOfLetters = false;

                for (; _i < _text.Length; _i++)
                {
                    if (HasSubstringAt(_i, "/*"))
                    {
                        inCommentBlock = true;
                    }

                    if (HasSubstringAt(_i - 2, "*/"))
                    {
                        inCommentBlock = false;
                    }

                    if (HasSubstringAt(_i, "//"))
                    {
                        inCommentedLine = true;
                    }

                    if (HasCharAt(_i - 1, '\n'))
                    {
                        inCommentedLine = false;
                    }

                    if (!inCommentBlock && !inCommentedLine)
                    {
                        //if(char.IsWhiteSpace(_text[_i]))
                        char c = _text[_i];
                        if (char.IsLetterOrDigit(c))
                        {
                            if (termBegin != -1 && !termIsMadeOfLetters)
                            {
                                break; // End term
                            }

                            if (termBegin == -1)
                            {
                                termBegin = _i; // Begin term
                                termIsMadeOfLetters = true;
                            }
                        }
                        else if (ConsiderSpecialCharactersTerms && !char.IsWhiteSpace(c) && !char.IsSeparator(c) && c != '\t')
                        {
                            if (termBegin != -1)
                            {
                                if (!termIsMadeOfLetters)
                                {
                                    foreach (string multiCharOperator in s_multiLetterOperators)
                                    {
                                        if (HasSubstringAt(termBegin, multiCharOperator))
                                        {
                                            // End term and move 'i' past the term
                                            _i = termBegin + multiCharOperator.Length;
                                            break;
                                        }
                                    }
                                }

                                break; // End term

                            }

                            if (termBegin == -1)
                            {
                                termBegin = _i; // Begin term
                                termIsMadeOfLetters = false;
                            }
                        }
                        else
                        {
                            if (termBegin != -1)
                            {
                                break; // End term
                            }
                        }
                    }
                    else
                    {
                        if (termBegin != -1)
                        {
                            break;
                        }
                    }
                }

                if (termBegin != -1)
                {
                    Current = _text.Substring(termBegin, _i - termBegin);
                }
                else
                {
                    Current = string.Empty;
                }

                return !string.IsNullOrEmpty(Current);
            }

            private bool HasCharAt(int i, char c)
            {
                return (i >= 0 && i < _text.Length) ? c == _text[i] : false;
            }

            private bool HasSubstringAt(int i, string substring)
            {
                for (int s = 0; s < substring.Length; s++)
                {
                    if (!HasCharAt(i + s, substring[s]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }


        /// <summary>
        /// Parses classes, structs and interfaces from code text. NB: parsing delegate types is not yet supported
        /// </summary>
        public static Type[] ParseTypesFromCode(string codeText)
        {
            using var _ = ListPool<Type>.Get(out List<Type> foundTypes);

            TermIterator termIterator = new TermIterator(codeText, considerSpecialCharactersTerms: true);

            bool nextTermIsType = false;

            //Keeps track of type contexts so we can find sub types (e.g classes in classes)
            Stack<Type> contextStack = new Stack<Type>();

            // Keeps track of which scope index matches the type in the 'contextStack'
            Stack<int> contextScope = new Stack<int>();

            // Keeps track of scope: { { } }
            int scopeCounter = 0;

            // variables for generic type parsing
            string potentialGenericTypeName = null;
            bool expectingGenericTypeOpening = false;
            int genericTypeParameterCount = 0;
            int genericTypeParameterAttributeScopeCounter = 0;

            while (termIterator.MoveNext())
            {
                if (termIterator.Current == "class" ||
                    termIterator.Current == "interface" ||
                    termIterator.Current == "struct")
                {
                    // With the terms 'class', 'interface' and 'struct, we're expecing the next term to be a type name

                    nextTermIsType = true;
                }

                else if (nextTermIsType)
                {
                    string typeName = termIterator.Current;

                    // If we're in the context of another type (e.g sub class), prepend the container type name
                    if (contextStack.Count > 0)
                    {
                        typeName = $"{contextStack.Peek().FullName}+{typeName}";
                    }

                    // try to find the type from the name
                    Type foundType = TypeUtility.FindType(typeName, throwOnError: false);
                    if (foundType != null)
                    {
                        foundTypes.Add(foundType);
                        contextStack.Push(foundType);
                    }
                    else
                    {
                        // If we failed to find the type, that might be because it's a generic type.
                        // Generic types' names are formatted as such: MyGenericType`3  (3 being the number of parameters)
                        // We need to look at the following terms to find how many parameters it has
                        potentialGenericTypeName = typeName;
                        expectingGenericTypeOpening = true;
                        genericTypeParameterCount = 1;
                    }

                    nextTermIsType = false;
                }

                // Enter a scope ?
                else if (termIterator.Current == "{")
                {
                    if (contextScope.Count < contextStack.Count)
                    {
                        // If 'contextScope' has less elements than 'contextStack', it means we've just passed a type definition
                        // and we're entering this type's scope. E.g:
                        // class MyType 
                        // {   <- we're here
                        // }
                        contextScope.Push(scopeCounter);
                    }

                    scopeCounter++;
                }

                // Exit a scope
                else if (termIterator.Current == "}")
                {
                    scopeCounter--;

                    while (contextScope.Count > 0 && contextScope.Peek() == scopeCounter)
                    {
                        // If the topmost 'contextScope' matches our scope counter, it means we're 
                        // exiting the scope of a type defintion. E.g:
                        // class MyType 
                        // {   
                        // } <- we're here
                        contextScope.Pop();
                        contextStack.Pop();
                    }
                }

                // Are we expecting a generic type ?
                else if (!string.IsNullOrEmpty(potentialGenericTypeName))
                {
                    if (expectingGenericTypeOpening)
                    {
                        if (termIterator.Current != "<")
                        {
                            // If we encounter another term than '<', it means we're not in a generic type definition.
                            potentialGenericTypeName = null;
                        }
                        expectingGenericTypeOpening = false;
                    }
                    else if (termIterator.Current == ",")
                    {
                        // Meeting a , means the generic type has +1 parameter.
                        // e.g. MyType<T, U, V> has 3 params

                        // this check makes sure we're not inside an attribute e.g. MyType<[SpecialTag(1, 20)] T , U, V>
                        if (genericTypeParameterAttributeScopeCounter == 0)
                        {
                            genericTypeParameterCount++;
                        }
                    }

                    // Are we entering an attribute ?
                    else if (termIterator.Current == "[")
                    {
                        genericTypeParameterAttributeScopeCounter++;
                    }

                    // Are we exiting an attribute ?
                    else if (termIterator.Current == "]")
                    {
                        genericTypeParameterAttributeScopeCounter--;
                    }

                    // Are we closing the generic parameters definition?
                    else if (termIterator.Current == ">")
                    {
                        string typeName = potentialGenericTypeName;

                        typeName += $"`{genericTypeParameterCount}";

                        Type foundType = TypeUtility.FindType(typeName, throwOnError: false);
                        if (foundType != null)
                        {
                            foundTypes.Add(foundType);
                            contextStack.Push(foundType);
                        }

                        potentialGenericTypeName = null;
                    }
                }
            }

            Type[] result = foundTypes.ToArray();

            return result;
        }

    }
}