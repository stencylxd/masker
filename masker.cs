
using System;
using static System.Console;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using static System.String;
using static System.ConsoleColor;

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
        public static float errorLineNumber;
        public static StreamReader codeFile;
        public static float numberToJumpTo;
        public static string jumpNameToLookFor;
        public static string[] code = new string[] { };

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    string codeFilePath = args[0];
                    if (!File.Exists(codeFilePath)) abort("Masker: File does not exist.", false);
                    codeFile = new StreamReader(codeFilePath);
                    code = codeFile.ReadToEnd().Split('\n');
                    Clear();
                    foreach (string line in code)
                    {
                        currentLine = line.Trim();
                        currentLineNumber++;
                        errorLineNumber = currentLineNumber + 1;
                        // Empty handler
                        if (currentLine.Trim() == Empty) continue;
                        // To handle substring length errors when statement less than 2 chars is processed
                        if (currentLine.Length < 2) abort($"There are no statements less than 2 characters long! (Line Number: {currentLineNumber}, Value: {currentLine})");
                        if (currentLine.Substring(0, 2).ToLower() == "cp")
                        {
                            if (currentLine.Trim().ToLower() == "cp") abort($"CP must be passed 1 argument! (Line Number: {currentLineNumber})");
                            string checkpointName = currentLine.Substring(3).Split(' ')[0];
                            if (jumpCheckpointNames.Contains(checkpointName)) abort($"Checkpoint already exists at line number " +
                                $"{jumpLineNumbers[jumpCheckpointNames.IndexOf(checkpointName)]} " +
                                $"(Line Number: {currentLineNumber}, Value: [{checkpointName}])");
                            jumpLineNumbers.Add(currentLineNumber);
                            jumpCheckpointNames.Add(currentLine.Substring(3).Split(' ')[0]);
                        }
                    }
                    currentLineNumber = -1;
                    while (true)
                    {
                        currentLineNumber++;
                        errorLineNumber = currentLineNumber + 1;
                        if (currentLineNumber > code.Length - 1) break;
                        currentLine = code[(int)currentLineNumber].Trim();

                        // Handlers for errors

                        // Empty handler
                        if (currentLine.Trim() == Empty) continue;
                        // Code Comments <//>
                        if (currentLine.Length == 2 && currentLine.Substring(0, 2) == "//") continue;
                        
                        // CP handler
                        if (currentLine.Length >= 3 && currentLine.Substring(0, 3).ToLower() == "cp ") continue;

                        // Command processors

                        // VAR call (defining a variable) [var <VARIABLE> !"<VALUE>"]
                        if (currentLine.ToLower() == "var") abort($"VAR must be given atleast 1 argument! (Line Number: {currentLineNumber})");
                        if (currentLine.Length >= 4 && currentLine.Substring(0, 4).ToLower() == "var ")
                        { 
                            string actualDefine = currentLine.Substring(4) + " ";
                            int indexOfSpace = actualDefine.IndexOf(" ");
                            string variableName = actualDefine.Substring(0, indexOfSpace);
                            string variableValue = actualDefine.Substring(indexOfSpace);
                            if (IsNullOrEmpty(variableValue.Trim())) variableValue = "NULL";
                            else variableValue = variableValue.removeStringAbort();
                            if (!variableExists(variableName)) newVariable(variableName, variableValue);
                            else abort($"Variable already exists. (Line Number: {currentLineNumber}, Variable Name: [{variableName}])");
                            continue;
                        }

                        // SET call (set value of existing variable) [set <VARIABLE> "<VALUE>"]
                        if (currentLine.ToLower() == "set") abort($"SET must be given 2 arguments! (Line Number: {currentLineNumber})");
                        if (currentLine.Length >= 4 && currentLine.Substring(0, 4).ToLower() == "set ")
                        {
                            string actualStatement = currentLine.Substring(4).Trim();
                            int indexOfSpace = actualStatement.IndexOf(" ");
                            string variableName = actualStatement.Substring(0, indexOfSpace).Trim();
                            string variableValue = actualStatement.Substring(indexOfSpace).removeStringAbort();
                            setValueOfVariable(variableName, variableValue);
                            continue;
                        }

                        // EXIT call (exit program) [exit]
                        if (currentLine.ToLower() == "exit") Environment.Exit(0);

                        // GOTO call (jump to checkpoint) [GOTO <CHECKPOINT>] 
                        if (currentLine.Trim().ToLower() == "goto") abort($"GOTO must be passed 1 argument! (Line Number: {currentLineNumber})");
                        if (currentLine.Length >= 5 && currentLine.Substring(0, 5).ToLower() == "goto ")
                        {
                            string checkpointName = currentLine.Substring(5).Trim();
                            if (!jumpCheckpointNames.Contains(checkpointName)) abort($"Checkpoint doesn't exist! " +
                                    $"(Line Number: {currentLineNumber}, Value: [{checkpointName}])");
                            float numberToJumpTo = jumpLineNumbers[jumpCheckpointNames.IndexOf(checkpointName)];
                            currentLine = code[(int)numberToJumpTo - 1];
                            currentLineNumber = numberToJumpTo - 1;
                            continue;
                        }

                        // CLEAR call (clear command line interface) [clear]
                        if (currentLine.ToLower() == "clear")
                        {
                            Clear();
                            continue;
                        }

                        // SLEEP call (sleep program for provided amount of seconds) [sleep <seconds>]
                        if (currentLine.ToLower() == "sleep") abort($"SLEEP must be given atleast 1 argument! (Line Number: {currentLineNumber})");
                        if (currentLine.Length >= 6 && currentLine.Substring(0, 6).ToLower() == "sleep ")
                        {
                            int actualNumber = Convert.ToInt32(currentLine.Substring(5).Trim()) * 1000;
                            Thread.Sleep(actualNumber);
                            continue;
                        }

                        // COLOR call (change color of text) [color <color name>]
                        if (currentLine.Trim().ToLower() == "color") abort($"COLOR must be passed 1 argument! (Line Number: {currentLineNumber})");
                        if (currentLine.Length >= 6 && currentLine.Substring(0, 6).ToLower() == "color ")
                        {
                            string colorName = currentLine.Substring(5).Trim().ToLower();
                            if (colorName == "blue") ForegroundColor = Blue;
                            if (colorName == "red") ForegroundColor = Red;
                            if (colorName == "magenta") ForegroundColor = Magenta;
                            if (colorName == "yellow") ForegroundColor = Yellow;
                            if (colorName == "cyan") ForegroundColor = Cyan;
                            if (colorName == "grey") ForegroundColor = Gray;
                            if (colorName == "green") ForegroundColor = Green;
                            if (colorName == "darkred") ForegroundColor = DarkRed;
                            if (colorName == "darkblue") ForegroundColor = DarkBlue;
                            if (colorName == "darkgreen") ForegroundColor = DarkGreen;
                            if (colorName == "darkyellow") ForegroundColor = DarkYellow;
                            if (colorName == "darkgrey") ForegroundColor = DarkGray;
                            if (colorName == "darkcyan") ForegroundColor = DarkCyan;
                            if (colorName == "darkmegenta") ForegroundColor = DarkMagenta;
                            if (colorName == "black") ForegroundColor = Black;
                            if (colorName == "white") ForegroundColor = White;
                            continue;
                        }

                        // INPUT call (getting input) [input <VARIABLE>]
                        if (currentLine.ToLower() == "input") abort($"INPUT must be passed 1 argument! (Line Number: {currentLineNumber})");
                        if (currentLine.Length >= 6 && currentLine.Substring(0, 6).ToLower() == "input ")
                        {
                            string variableName = currentLine.Substring(6).Trim();
                            string userInput = ReadLine();
                            setValueOfVariable(variableName, userInput);
                            continue;
                        }

                        // PRINT call (print to screen with newline) [print "<STRING>"] || [print <VARIABLE>]
                        if (currentLine.ToLower() == "print")
                        {
                            WriteLine();
                            continue;
                        }
                        if (currentLine.Length >= 6 && currentLine.Substring(0, 6).ToLower() == "print ")
                        {
                            string stringToPrint;

                            string value = currentLine.Substring(6).Trim();

                            if (value.removeStringAbort(true) == "yes") stringToPrint = value.removeStringAbort();
                            else stringToPrint = getValueOfVariable(value);
                            WriteLine(stringToPrint);
                            continue;
                        }

                        // XPRINT call (print to screen without newline) [xprint "<STRING>"] || [xprint <VARIABLE>]
                        if (currentLine.ToLower() == "xprint")
                        {
                            warn($"Useless xprint. (Line Number: {errorLineNumber})");
                            continue;
                        }
                        if (currentLine.Length >= 5 && currentLine.Substring(0, 6).ToLower() == "xprint")
                        {
                            string stringToPrint;

                            string value = currentLine.Substring(6).Trim();

                            if (value.removeStringAbort(true) == "yes") stringToPrint = value.removeStringAbort();
                            else stringToPrint = getValueOfVariable(value);
                            Write(stringToPrint);
                            continue;
                        }

                        // GOTOIF call (conditional jump) [gotoif <value> <value> <checkpoint>]
                        if (currentLine.ToLower() == "gotoif") abort($"GOTOIF must be passed 3 arguments! (Line Number: {currentLineNumber})");
                        if (currentLine.Substring(0, 6).ToLower() == "gotoif")
                        {
                            string arguments = currentLine.Substring(6).Trim();
                            string value1 = arguments.Substring(0, arguments.IndexOf(" ")).Trim();
                            arguments = arguments.Substring(arguments.IndexOf(" ")).Trim();
                            string value2 = arguments.Substring(0, arguments.IndexOf(" "));
                            string checkpointName = arguments.Substring(arguments.IndexOf(" ")).Trim();

                            if (value1.removeStringAbort(true) != "no") value1 = value1.removeStringAbort();
                            else value1 = getValueOfVariable(value1);
                            if (value2.removeStringAbort(true) != "no") value2 = value2.removeStringAbort();
                            else value2 = getValueOfVariable(value2);

                            if (value1 == value2)
                            {
                                if (!jumpCheckpointNames.Contains(checkpointName)) abort($"Checkpoint doesn't exist! " +
                                    $"(Line Number: {currentLineNumber}, Value: [{checkpointName}])");
                                float numberToJumpTo = jumpLineNumbers[jumpCheckpointNames.IndexOf(checkpointName)];
                                currentLine = code[(int)numberToJumpTo - 1];
                                currentLineNumber = numberToJumpTo - 1;
                            }
                            continue;
                        }

                        // SLEEPX call (sleep program for provided amount of milliseconds) [sleepx <milliseconds>]
                        if (currentLine.ToLower() == "sleepx") abort($"SLEEPX must be passed 1 arguments! (Line Number: {currentLineNumber})");
                        if (currentLine.Substring(0, 7).ToLower() == "sleepx ")
                        {
                            int actualNumber = Convert.ToInt32(currentLine.Substring(6).Trim());
                            Thread.Sleep(actualNumber);
                            continue;
                        }

                        // XINPUT call (getting input that is lowercased) [xinput <VARIABLE>]
                        if (currentLine.ToLower() == "xinput") abort($"XINPUT must be passed 1 argument! (Line Number: {currentLineNumber})");
                        if (currentLine.Substring(0, 7).ToLower() == "xinput ")
                        { 
                            string variableName = currentLine.Substring(7).Trim();
                            string userInput = ReadLine().ToLower();
                            setValueOfVariable(variableName, userInput);
                            continue;
                        }

                        // XXINPUT call (getting input that is uppercased) [xxinput <VARIABLE>]
                        if (currentLine.ToLower() == "xxinput") abort($"XXINPUT must be passed 1 argument! (Line Number: {currentLineNumber})");
                        if (currentLine.Substring(0, 8).ToLower() == "xxinput ")
                        {
                            string variableName = currentLine.Substring(8).Trim();
                            string userInput = ReadLine().ToUpper();
                            setValueOfVariable(variableName, userInput);
                            continue;
                        }

                        // Invalid command handler
                        if (currentLine[0].ToString() != "") abort($"Invalid command! (Line Number: {currentLineNumber}, Value: [{currentLine}])");

                    }
                }
                else
                {
                    abort("Masker: Please provide a file to run. (arguments)", false);
                }
            }
            catch (Exception err)
            {
                if (err is IOException) abort("Something changed about the running codefile! (Caught: IOException)");
                if (err is PathTooLongException) abort("Please enter a shorter file path. (Caught: PathTooLongException");
                if (err is NotSupportedException) abort("The file you gave cannot be read by Masker. (Caught: NotSupportedException)");
                if (err is UnauthorizedAccessException) abort("Masker does not have permission to read this file. (Caught: UnauthorizedAccessException)");
                WriteLine($"{err.Message}");
                Write($"{err.StackTrace}\n\n");
                abort($"Sorry, but Masker ran into an error. If your code looks completely fine, please make a issue on the Github repository. Provide this message along with the text shown above. " +
                    $"(Line Number: [{currentLineNumber}], Current Line In File: [{currentLine}])");
            }
        }

        public static string getValueOfVariable(string variableName)
        {
            if (!variableExists(variableName)) abort($"Variable does not exist. (Line Number: {currentLineNumber}, Value: {variableName})");
            int index = variableNames.IndexOf(variableName);
            string variableValue = variableValues[index];
            return variableValue;
        }

        public static void setValueOfVariable(string variableName, string newValue)
        {
            if (!variableExists(variableName)) abort($"Variable does not exist. (Line Number: {currentLineNumber}, Value: {variableName})");
            int index = variableNames.IndexOf(variableName);
            variableValues[index] = newValue;
        }

        public static bool variableExists(string variableName)
        {
            if (variableNames.Contains(variableName))
            {
                return true;
            }
            else
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
            }
            else
            {
                return 0;
            }
        }

        public static void abort(string errMessage, bool error = true, int exitCode = 0)
        {
            if (error)
            {
                ForegroundColor = DarkRed;
                Write("\n\nERROR: ");
                ResetColor();
                WriteLine(errMessage);
            }
            else
            {
                WriteLine("\n" + errMessage);
            }
            Environment.Exit(exitCode);
        }

        public static void warn(string warningMessage)
        {
            ConsoleColor backupColor = ForegroundColor;
            ForegroundColor = Red;
            Write("\n\nWARNING: ");
            ResetColor();
            WriteLine(warningMessage);
            ForegroundColor = backupColor;
        }

        public static string removeStringAbort(this string stringToCheck, bool justCheck = false)
        {
            stringToCheck = stringToCheck.Trim();
            if (stringToCheck[0] == '"' && stringToCheck[stringToCheck.Length - 1] == '"')
            {
                // If it does have quotation marks:
                if (!justCheck) return stringToCheck.Substring(1, stringToCheck.Length - 2);
                return "yes";
            }
            else
            {
                // If it doesn't:
                if (!justCheck) abort($"String value does not have quotation marks. (Line Number: {currentLineNumber}, Value: [{stringToCheck}])");
                return "no";
            }
        }
    }
}