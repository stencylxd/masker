
using System;
using static System.Console;
using static System.Array;
using System.Linq;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using static System.String;

namespace Masker
{
    public static class MaskerEPL
    {

        public static List<string> variableNames = new List<string> { };
        public static List<string> variableValues = new List<string> { };
        public static float currentLineNumber = 0;
        public static List<float> jumpLineNumbers = new List<float> { };
        public static List<string> jumpCheckpointNames = new List<string> { };
        public static string currentLine;
        public static float backupCurrentLineNumber;
        public static StreamReader codeFile;
        public static float numberToJumpTo;
        public static string jumpNameToLookFor;

        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                string codeFilePath = args[0];
                if (!File.Exists(codeFilePath)) abort("Masker: File does not exist.", false);
                codeFile = File.OpenText(codeFilePath);
                Clear();
                while ((currentLine = codeFile.ReadLine()) != null)
                {
                    currentLineNumber++;
                    if (currentLine.Substring(0, 3).ToLower() == "cp ")
                    {
                        string checkpointName = currentLine.Substring(3).Split(' ')[0];
                        if (jumpCheckpointNames.Contains(checkpointName)) abort($"Checkpoint already exists at line number " +
                            $"{jumpLineNumbers[jumpCheckpointNames.IndexOf(checkpointName)]} " +
                            $"(Line Number: {currentLineNumber}, Value: [{checkpointName}])");
                        jumpLineNumbers.Add(currentLineNumber);
                        jumpCheckpointNames.Add(currentLine.Substring(3).Split(' ')[0]); 
                    }
                }
                codeFile.Close();
                currentLineNumber = 0;
                codeFile = File.OpenText(codeFilePath);
                while ((currentLine = codeFile.ReadLine()) != null)
                {
                    currentLineNumber++;
                    if (currentLine.Substring(0, 5).ToLower() == "jump ")
                    {;
                        codeFile.Close();
                        codeFile = File.OpenText(codeFilePath);
                        jumpNameToLookFor = currentLine.Substring(5).Trim().Split(' ')[0];
                        numberToJumpTo = jumpLineNumbers[jumpCheckpointNames.IndexOf(jumpNameToLookFor)];
                        if (!jumpCheckpointNames.Contains(jumpNameToLookFor)) abort($"Checkpoint doesn't exist! " +
                            $"(Line Number: {currentLineNumber}, Value: [{jumpNameToLookFor}])");
                        currentLineNumber = 0;
                        while (currentLineNumber != (numberToJumpTo))
                        {
                            currentLineNumber++;
                        }
                    }
                    processCommand(currentLine);
                }
            } else
            {
                abort("Masker: Please provide a file to run. (arguments)", false);
            }
        }

        public static void processCommand(string command)
        {
            // Code Comments <//>
            if (command.Substring(0, 2) == "//") return;

            // PRINT call (print to screen with newline) [print "<STRING>"]
            if (command.Substring(0, 6).ToLower() == "print ")
            {
                string stringToPrint = command.Substring(6).Trim().removeStringAbort();
                WriteLine(stringToPrint);
                return;
            }

            // XPRINT call (print to screen without newline) [xprint "<STRING>"]
            if (command.Substring(0, 7).ToLower() == "xprint ")
            {
                string stringToPrint = command.Substring(6).Trim().removeStringAbort();
                Write(stringToPrint);
                return;
            }

            // VAR call (defining a variable)
            if (command.Substring(0, 4).ToLower() == "var ")
            {
                string actualDefine = command.Substring(4).Trim();
                int indexOfSpace = actualDefine.IndexOf(" ");
                string variableValue = actualDefine.Substring(indexOfSpace + 1).removeStringAbort();
                string variableName = actualDefine.Substring(0, indexOfSpace);
                if (!variableExists(variableName))
                {
                    newVariable(variableName, variableValue);
                } else
                {
                    abort($"Variable already exists. (Line Number: {currentLineNumber}, Variable Name: [{variableName}])");
                }
                return;
            }

            // SET call (set value of existing variable)
            if (command.Substring(0, 4).ToLower() == "set ")
            {
                string[] actualStatement = command.Substring(0, 4).Trim().Split(' ');
                string variableName = actualStatement[0];
                string variableValue = actualStatement[1].removeStringAbort();
                setValueOfVariable(variableName, variableValue);
                return;
            }

            // INP call (getting input)
            if (command.Substring(0, 4).ToLower() == "inp ")
            {
                string variableName = command.Substring(4).Trim();
                string userInput = ReadLine();
                setValueOfVariable(variableName, userInput);
                return;
            }

            // INPX call (getting input that is lowercased)
            if (command.Substring(0, 5).ToLower() == "inpx ")
            {
                string variableName = command.Substring(5).Trim();
                string userInput = ReadLine().ToLower();
                setValueOfVariable(variableName, userInput);
                return;
            }

            // INPXX call (getting input that is uppercased)
            if (command.Substring(0, 6).ToLower() == "inpxx ")
            {
                string variableName = command.Substring(6).Trim();
                string userInput = ReadLine().ToUpper();
                setValueOfVariable(variableName, userInput);
                return;
            }

            // cp handler
            if (command.Substring(0, 3).ToLower() == "cp ") return;

            // jump handler
            if (command.Substring(0, 5).ToLower() == "jump ") return;

            // invalid command handler
            if (command.Trim()[0].ToString() != "") abort($"Invalid command! (Line Number: {currentLineNumber}, Value: [{command}])");

            // null entry will handle itself
        }

        public static string getValueOfVariable(string variableName)
        {
            if (!variableExists(variableName)) abort($"Variable does not exist. (Line Number: {currentLineNumber})");
            int index = variableNames.IndexOf(variableName);
            string variableValue = variableValues[index];
            return variableValue;
        }

        public static void setValueOfVariable(string variableName, string newValue)
        {
            if (!variableExists(variableName)) abort($"Variable does not exist. (Line Number: {currentLineNumber})");
            int index = variableNames.IndexOf(variableName);
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
                variableNames.Add(variableName);
                variableValues.Add(variableValue);
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

        public static string removeStringAbort(this string stringToCheck)
        {
            if (stringToCheck[0] == '"' && stringToCheck[stringToCheck.Length - 1] == '"')
            {
                return stringToCheck.Substring(1, stringToCheck.Length - 2);
            } else {
                abort($"String value does not have quotation marks. (Line Number: {currentLineNumber}, Value: [{stringToCheck}])");
                return "Fatal Error!";
            }
        }
    }
}