
using System;
using static System.Console;
using static System.Array;
using System.Linq;
using System.Threading;
using System.IO;

namespace Masker
{
    public static class MaskerEPL
    {

        public static string[] variableNames = new string[] { };
        public static string[] variableValues = new string[] { };
        public static int currentLineNumber = 0;

        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                string codeFilePath = args[0];
                if (!File.Exists(codeFilePath)) abort("Masker: File does not exist.", false);
                Clear();
                using (StreamReader codeFile = File.OpenText(codeFilePath))
                {
                    string currentLine;
                    while ((currentLine = codeFile.ReadLine()) != null)
                    {
                        currentLineNumber++;
                        processCommand(currentLine);
                    }
                }
            } else
            {
                abort("Masker: Please provide a file to run. (arguments)", false);
            }
        }

        public static void processCommand(string command)
        {
            // Code Comments <//>
            // Code Comments <//>
            if (command.Substring(0, 1) == "//") return;

            // PRINT call (print to screen with newline) [print "<STRING>"]
            if (command.Substring(0, 5).ToLower() == "print")
            {
                string stringToPrint = command.Substring(5).Trim();
                stringToPrint = stringToPrint.Substring(1);
                stringToPrint = stringToPrint.Substring(0, stringToPrint.Length - 1);
                WriteLine(stringToPrint);
                return;
            }

            // XPRINT call (print to screen without newline) [xprint "<STRING>"]
            if (command.Substring(0, 6).ToLower() == "xprint")
            {
                string stringToPrint = command.Substring(6).Trim();
                stringToPrint = stringToPrint.Substring(1);
                stringToPrint = stringToPrint.Substring(0, stringToPrint.Length - 1);
                Write(stringToPrint);
                return;
            }

            // VAR call (defining a variable)
            if (command.Substring(0, 3).ToLower() == "var")
            {
                string actualDefine = command.Substring(4);
                int indexOfSpace = actualDefine.IndexOf(" ");
                string variableValue = actualDefine.Substring(indexOfSpace + 1).removeStringAbort();
                string variableName = actualDefine.Substring(0, indexOfSpace - 1);
                if (!variableExists(variableName))
                {
                    newVariable(variableName, variableValue);
                } else
                {
                    abort($"Variable already exists. (Line Number: {currentLineNumber}, Variable Name: [{variableName}])");
                }
                return;
            }

            // INP call (getting input)
            if (command.Substring(0, 3).ToLower() == "inp")
            {
                string variableName = command.Substring(4).Trim();
                string userInput = ReadLine();
                setValueOfVariable(variableName, userInput);
                return;
            }

            // INPX call (getting input that is lowercased)
            if (command.Substring(0, 3).ToLower() == "inpx")
            {
                string variableName = command.Substring(5).Trim();
                string userInput = ReadLine().ToLower();
                setValueOfVariable(variableName, userInput);
                return;
            }

            // INPXX call (getting input that is uppercased)
            if (command.Substring(0, 3).ToLower() == "inpxx")
            {
                string variableName = command.Substring(6).Trim();
                string userInput = ReadLine().ToUpper();
                setValueOfVariable(variableName, userInput);
                return;
            }
        }

        public static string getValueOfVariable(string variableName)
        {
            // You should check if the variable exists before you call this!
            if (!variableExists(variableName)) abort($"Variable does not exist. (Line Number: {currentLineNumber})");
            int index = IndexOf(variableNames, variableName);
            string variableValue = variableValues[index];
            return variableValue;
        }

        public static void setValueOfVariable(string variableName, string newValue)
        {
            // You should check if the variable exists before you call this!
            if (!variableExists(variableName)) abort($"Variable does not exist. (Line Number: {currentLineNumber})");
            int index = IndexOf(variableNames, variableName);
            variableValues[index] = newValue;
        }

        public static bool variableExists(string variableName)
        {
            if (variableNames.Contains(variableName))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public static int newVariable(string variableName, string variableValue)
        {
            if (!variableExists(variableName))
            {
                variableNames.Push(variableName);
                variableValues.Push(variableValue);
                return 1;
            } else
            {
                return 0;
            }
        }

        public static void abort(string errMessage, bool processError = true)
        {
            if (processError)
            {
                ForegroundColor = ConsoleColor.Red;
                Write("\n\nPROCESS ERROR: ");
                ForegroundColor = ConsoleColor.White;
                Write(errMessage);
            } else
            {
                WriteLine("\n" + errMessage);
            }
            Environment.Exit(0);
        }

        public static void Push<T>(this T[] source, T value)
        {
            Array.Resize(ref source, source.Length + 1);
            source[source.GetUpperBound(0)] = value;
        }

        public static string removeStringAbort(this string stringToCheck)
        {
            if (stringToCheck[0] == '"' && stringToCheck[stringToCheck.Length - 1] == '"')
            {
                return stringToCheck.Substring(1, stringToCheck.Length - 1);
            } else {
                abort($"String value does not have quotation marks. (Line Number: {currentLineNumber}, Value: [{stringToCheck}])");
                return "Fatal Error!";
            }
        }
    }
}