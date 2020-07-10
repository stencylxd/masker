
using System;
using static System.Console;
using static System.Array;
using System.Linq;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using static System.String;
using System.Diagnostics;

namespace Masker
{
    public static class MaskerEPL
    {

        public static List<string> variableNames = new List<string> { };
        public static List<string> variableValues = new List<string> { };
        public static float currentLineNumber = 0;
        public static List<float> jumpLineNumbers = new List<float> { };
        public static List<string> jumpCheckpointNames = new List<string> { };
        public static string currentLine = "NO_FILE_OPEN";
        public static float backupCurrentLineNumber;
        public static StreamReader codeFile;
        public static float numberToJumpTo;
        public static string jumpNameToLookFor;

        public static void Main(string[] args)
        {
            try
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
                        // Null handler
                        if (IsNullOrEmpty(currentLine.Trim())) continue;
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
                        // Null handler
                        if (IsNullOrEmpty(currentLine.Trim())) continue;
                        if (currentLine.Substring(0, 5).ToLower() == "jump ")
                        {
                            ;
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
                                currentLine = codeFile.ReadLine();
                            }
                        }
                        processCommand(currentLine);
                    }
                }
                else
                {
                    abort("Masker: Please provide a file to run. (arguments)", false);
                }
            }
            catch (Exception err)
            {
                if (err is IOException) abort("Something changed about the running codefile! (Caught: System.IOException)");
                if (err is PathTooLongException) abort("Please enter a shorter file path. (Caught: PathTooLongException");
                if (err is NotSupportedException) abort("The file you gave cannot be read by Masker. (Caught: NotSupportedException)");
                if (err is UnauthorizedAccessException) abort("Masker does not have permission to read this file. (Caught: UnauthorizedAccessException)");
                WriteLine(err.Message + "\n" + err.StackTrace);
                abort($"Sorry, but Masker ran into an error. Please make a issue on the Github repository, provide this message with the text shown above. " +
                    $"(Line Number: [{currentLineNumber}], Current Line In File: [{currentLine}])");
            }
        }

        public static void processCommand(string command)
        {
            command = command.Trim();

            // Code Comments <//>
            if (command.Substring(0, 2) == "//") return;

            // CP handler
            if (command.Substring(0, 3).ToLower() == "cp ") return;

            // VAR call (defining a variable)
            if (command.Substring(0, 3).ToLower() == "var")
            {
                if (command.ToLower() == "var") abort($"VAR must be given atleast 1 argument! (Line Number: {currentLineNumber})");
                string actualDefine = command.Substring(4) + " ";
                int indexOfSpace = actualDefine.IndexOf(" ");
                string variableName = actualDefine.Substring(0, indexOfSpace);
                string variableValue = actualDefine.Substring(indexOfSpace);
                if (IsNullOrEmpty(variableValue.Trim())) variableValue = "NULL";
                else variableValue = variableValue.removeStringAbort();
                if (!variableExists(variableName))
                {
                    newVariable(variableName, variableValue);
                }
                else
                {
                    abort($"Variable already exists. (Line Number: {currentLineNumber}, Variable Name: [{variableName}])");
                }
                return;
            }

            // SET call (set value of existing variable) [set <VARIABLE> <VALUE>]
            if (command.Substring(0, 3).ToLower() == "set")
            {
                if (command.ToLower() == "set") abort($"SET must be given 2 arguments! (Line Number: {currentLineNumber})");
                string actualStatement = command.Substring(0, 4).Trim();
                int indexOfSpace = actualStatement.IndexOf(" ");
                string variableName = actualStatement.Substring(0, indexOfSpace).Trim();
                string variableValue = actualStatement.Substring(indexOfSpace).Trim().removeStringAbort();
                setValueOfVariable(variableName, variableValue);
                return;
            }

            // PRINT call (print to screen with newline) [print <STRING>] || [print <VARIABLE>
            if (command.Substring(0, 5).ToLower() == "print")
            {
                string stringToPrint;

                if (command.ToLower() == "print")
                {
                    WriteLine();
                    return;
                }

                if (command.Substring(6)[0] == '"') stringToPrint = command.Substring(6).Trim().removeStringAbort();
                else
                {
                    stringToPrint = getValueOfVariable(command.Substring(6).Trim());
                }

                WriteLine(stringToPrint);
                return;
            }

            // INP call (getting input) [inp <VARIABLE>]
            if (command.Substring(0, 6).ToLower() == "input ")
            {
                string variableName = command.Substring(6).Trim();
                string userInput = ReadLine();
                setValueOfVariable(variableName, userInput);
                return;
            }

            // XPRINT call (print to screen without newline) [xprint <STRING>] || [xprint <VARIABLE>]
            if (command.Substring(0, 6).ToLower() == "xprint")
            {
                string stringToPrint;

                if (command.ToLower() == "xprint")
                {
                    warn($"Useless xprint. (Line Number: {currentLineNumber})");
                    return;
                }

                if (command.Substring(7)[0] == '"') stringToPrint = command.Substring(7).Trim().removeStringAbort();
                else
                {
                    stringToPrint = getValueOfVariable(command.Substring(7).Trim());
                }

                Write(stringToPrint);
                return;
            }

            // INPX call (getting input that is lowercased) [inpx <VARIABLE>]
            if (command.Substring(0, 7).ToLower() == "inputx ")
            {
                string variableName = command.Substring(7).Trim();
                string userInput = ReadLine().ToLower();
                setValueOfVariable(variableName, userInput);
                return;
            }

            // INPXX call (getting input that is uppercased) [inpxx <VARIABLE>]
            if (command.Substring(0, 8).ToLower() == "inputxx ")
            {
                string variableName = command.Substring(8).Trim();
                string userInput = ReadLine().ToUpper();
                setValueOfVariable(variableName, userInput);
                return;
            }

            // Invalid command handler
            if (command.Trim()[0].ToString() != "") abort($"Invalid command! (Line Number: {currentLineNumber}, Value: [{command}])");

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
                variableNames.Add(variableName.Trim());
                variableValues.Add(variableValue.Trim());
                return 1;
            } else
            {
                return 0;
            }
        }

        public static void abort(string errMessage, bool error = true, int exitCode = 0)
        {
            if (error)
            {
                ForegroundColor = ConsoleColor.DarkRed;
                Write("\n\nERROR: ");
                ForegroundColor = ConsoleColor.White;
                WriteLine(errMessage);
            } else
            {
                WriteLine("\n" + errMessage);
            }
            Environment.Exit(exitCode);
        }

        public static void warn(string warningMessage)
        {
            ForegroundColor = ConsoleColor.Red;
            Write("\n\nWARNING: ");
            ForegroundColor = ConsoleColor.White;
            WriteLine(warningMessage);
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