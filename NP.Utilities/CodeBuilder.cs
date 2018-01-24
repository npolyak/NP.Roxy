// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author(s) of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NP.Utilities
{
    public class CodeBuilder
    {
        Stack<string> RegionStack { get; } = new Stack<string>();

        public const string OPEN_BLOCK_STR = "{";
        public const int TAB_SIZE = 4;

        public const char SHIFT_CHAR = ' ';

        public string ShiftStr =>
            new string(SHIFT_CHAR, Shift * TAB_SIZE);

        // number of tabs
        public int Shift { get; private set; }
        private StringBuilder TheBuilder { get; set; }

        // initial shifts are only used to 
        // get the pops
        public CodeBuilder(int initialShift = 0)
        {
            Reset();

            Shift = initialShift;
        }

        public void Reset()
        {
            TheBuilder = new StringBuilder();
            Shift = 0;
            TheCurrentOffset = 0;
        }

        public int TheCurrentOffset { get; private set; } = 0;

        private void AppendShift()
        {
            TheBuilder.Append(ShiftStr);

            TheCurrentOffset += ShiftStr.Length;

            _lastChar = SHIFT_CHAR;
        }

        char _lastChar = (char)0;

        public void AppendShiftIsLastCharNewLine()
        {
            if (ShouldAppendShift)
            {
                AppendShift();
            }
        }

        bool ShouldAppendShift =>
            (_lastChar == '\n') || IsAtStart;

        void AddCharAsIs(char c)
        {
            TheBuilder.Append(c);
            TheCurrentOffset += 1;

            _lastChar = c;
        }

        public void AddTextAsIs(string text)
        {
            if (text == null)
                return;

            foreach (char c in text)
            {
                AddCharAsIs(c);
            }
        }

        public void AddText(string text)
        {
            if (text == null)
                return;

            foreach (char c in text)
            {
                AppendShiftIsLastCharNewLine();

                AddCharAsIs(c);
            }
        }

        public bool IsAtStart
        {
            get
            {
                return (this.TheBuilder.ToString() == null) ||
                       (this.TheBuilder.ToString() == string.Empty);
            }
        }

        public void AddEmptyLine()
        {
            AddText("\n");
        }


        public void CloseStatement()
        {
            AddText(";");
        }

        public void AddLine
        (
            string text = null,
            bool addSemiColon = false,
            bool trim = true
        )
        {
            if (trim)
                text = text?.Trim();

            if (!IsAtStart)
            {
                AddEmptyLine();
            }

            AddText(text);

            if (addSemiColon)
            {
                CloseStatement();
            }
        }

        public void AddLines(string text)
        {
            string[] lines =
                text.Split
                (
                    new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()
                ).ToArray();

            foreach (string line in lines)
            {
                if (line == "}")
                    Pop();
                else if (line == "{")
                    Push();
                else
                    AddLine(line);
            }
        }

        public void Push()
        {
            AddLine(OPEN_BLOCK_STR);
            Shift++;
        }

        public void PushRegion(string regionText)
        {
            AddLine($"#region {regionText}");

            RegionStack.Push(regionText);
        }

        public void PopRegion(bool addEmptyLine = true)
        {
            string regionText = RegionStack.Pop();

            AddLine($"#endregion {regionText}");

            if (addEmptyLine)
                AddEmptyLine();
        }

        public void AddNamespace(string namespaceName)
        {
            AddLine($"namespace {namespaceName}");
            Push();
        }

        public void AddClass
        (
            string className,
            Type baseType = null,
            Func<Type, Type> genericArgToType = null
        )
        {
            string extentionString = string.Empty;

            if (baseType != null)
            {
                extentionString = $" : {baseType.GetTypeStr(genericArgToType)}";
            }

            // make it internal so that it won't nameclash with other assemblies
            AddLine($"public class {className}{extentionString}");
            Push();
        }

        public void AddMethodOpening
        (
            MethodInfo methodInfo,
            bool shouldOverride = false,
            Func<Type, Type> genericArgToType = null,
            Func<string, string> paramNameConverter = null
        )
        {
            this.AddLine
            (
                methodInfo.GetMethodSignature
                (
                    shouldOverride,
                    genericArgToType,
                    paramNameConverter
                )
            );
            this.Push();
        }

        public void Pop(bool addEmptyLine = false)
        {
            string closingStr = OPEN_BLOCK_STR.GetClosing();
            Shift--;
            AddLine(closingStr);

            if (addEmptyLine)
                AddEmptyLine();
        }

        public void PopAll()
        {
            while (Shift > 0)
            {
                Pop();
            }
        }

        public void AddUsing(string namespaceName)
        {
            AddLine(namespaceName.GetUsingText(), true);
        }

        public override string ToString()
        {
            return TheBuilder.ToString();
        }

        public const string GETTER = "get";
        public const string SETTER = "set";

        public virtual void AddPropGetter()
        {
            AddLine(GETTER);
        }

        public virtual void AddPropSetter()
        {
            AddLine(SETTER);
        }

        public void OpenPropGetter()
        {
            AddPropGetter();
            Push();
        }

        public void OpenPropSetter()
        {
            AddPropSetter();
            Push();
        }
    }
}
