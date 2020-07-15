
using System;
using static System.Console;
using System.Linq;
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
                    codeFile.Close();
                    currentLineNumber = 0;
                    codeFile = File.OpenText(codeFilePath);
                    while ((currentLine = codeFile.ReadLine()) != null)
                    {
                        currentLineNumber++;
                        // Null handler
                        if (IsNullOrEmpty(currentLine.Trim())) continue;

                        currentLine = currentLine.Trim();

                        // Code Comments <//>
                        if (currentLine.Substring(0, 2) == "//") continue;

                        // CP handler
                        if (currentLine.Substring(0, 3).ToLower() == "cp ") continue;

                        // VAR call (defining a variable) [var <VARIABLE> !"<VALUE>"]
                        if (currentLine.Substring(0, 3).ToLower() == "var")
                        {
                            if (currentLine.ToLower() == "var") abort($"VAR must be given atleast 1 argument! (Line Number: {currentLineNumber})");
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
                        if (currentLine.Substring(0, 3).ToLower() == "set")
                        {
                            if (currentLine.ToLower() == "set") abort($"SET must be given 2 arguments! (Line Number: {currentLineNumber})");
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
                        // This line of code is here so goto doesn't override gotoif.
                        if (currentLine.Trim().ToLower() == "goto") abort($"GOTO must be passed 1 argument! (Line Number: {currentLineNumber})"); 
                        if (currentLine.Substring(0, 5).ToLower() == "goto ")
                        {
                            codeFile.Close();
                            codeFile = File.OpenText(codeFilePath);
                            jumpNameToLookFor = currentLine.Substring(5).Trim().Split(' ')[0];
                            if (!jumpCheckpointNames.Contains(jumpNameToLookFor)) abort($"Checkpoint doesn't exist! " +
                                $"(Line Number: {currentLineNumber}, Value: [{jumpNameToLookFor}])");
                            numberToJumpTo = jumpLineNumbers[jumpCheckpointNames.IndexOf(jumpNameToLookFor)];
                            currentLineNumber = 0;
                            while (currentLineNumber != (numberToJumpTo))
                            {
                                currentLineNumber++;
                                currentLine = codeFile.ReadLine();
                            }
                            continue;
                        }

                        // CLEAR call (clear command line interface) [clear]
                        if (currentLine.ToLower() == "clear")
                        {
                            Clear();
                            continue;
                        }

                        // SLEEP call (sleep program for provided amount of seconds) [sleep <seconds>]
                        if (currentLine.Substring(0, 5).ToLower() == "sleep")
                        {
                            int actualNumber = Convert.ToInt32(currentLine.Substring(5).Trim()) * 1000;
                            Thread.Sleep(actualNumber);
                            continue;
                        }

                        // COLOR call (change color of text) [color <color name>]
                        if (currentLine.Substring(0, 5).ToLower() == "color")
                        {
                            if (currentLine.Trim().ToLower() == "color") abort($"COLOR must be passed 1 argument! (Line Number: {currentLineNumber})");
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

                        // SLEEPX call (sleep program for provided amount of milliseconds) [sleepx <milliseconds>]
                        if (currentLine.Substring(0, 6).ToLower() == "sleepx")
                        {
                            int actualNumber = Convert.ToInt32(currentLine.Substring(6).Trim());
                            Thread.Sleep(actualNumber);
                            continue;
                        }

                        // GOTOIF call (conditional jump) [gotoif <variable> "<value>" <checkpoint>]
                        if (currentLine.Substring(0, 6).ToLower() == "gotoif")
                        {
                            if (currentLine.ToLower() == "gotoif") abort($"GOTOIF must be passed 3 arguments! (Line Number: {currentLineNumber})");
                            string arguments = currentLine.Substring(6).Trim();
                            string variable = arguments.Substring(0, arguments.IndexOf(" ")).Trim();
                            arguments = arguments.Substring(arguments.IndexOf(" ")).Trim();
                            string value = arguments.Substring(0, arguments.IndexOf(" ")).removeStringAbort();
                            string checkpointName = arguments.Substring(arguments.IndexOf(" ")).Trim();

                            if (getValueOfVariable(variable) == value)
                            {
                                codeFile.Close();
                                codeFile = File.OpenText(codeFilePath);
                                if (!jumpCheckpointNames.Contains(checkpointName)) abort($"Checkpoint doesn't exist! " +
                                    $"(Line Number: {currentLineNumber}, Value: [{checkpointName}])");
                                float numberToJumpTo = jumpLineNumbers[jumpCheckpointNames.IndexOf(checkpointName)];
                                currentLineNumber = 0;
                                while (currentLineNumber != (numberToJumpTo))
                                {
                                    currentLineNumber++;
                                    currentLine = codeFile.ReadLine();
                                }
                            }
                            continue;
                        }

                        // PRINT call (print to screen with newline) [print "<STRING>"] || [print <VARIABLE>]
                        if (currentLine.Substring(0, 5).ToLower() == "print")
                        {
                            string stringToPrint;

                            if (currentLine.ToLower() == "print")
                            {
                                WriteLine();
                                continue;
                            }

                            if (currentLine.Substring(6)[0] == '"') stringToPrint = currentLine.Substring(6).Trim().removeStringAbort();
                            else stringToPrint = getValueOfVariable(currentLine.Substring(6).Trim());
                            WriteLine(stringToPrint);
                            continue;
                        }

                        // INPUT call (getting input) [input <VARIABLE>]
                        if (currentLine.Substring(0, 5).ToLower() == "input")
                        {
                            if (currentLine.ToLower() == "input") abort($"INPUT must be passed 1 argument! (Line Number: {currentLineNumber})");
                            string variableName = currentLine.Substring(6).Trim();
                            string userInput = ReadLine();
                            setValueOfVariable(variableName, userInput);
                            continue;
                        }

                        // XPRINT call (print to screen without newline) [xprint "<STRING>"] || [xprint <VARIABLE>]
                        if (currentLine.Substring(0, 6).ToLower() == "xprint")
                        {
                            string stringToPrint;

                            if (currentLine.ToLower() == "xprint")
                            {
                                warn($"Useless xprint. (Line Number: {currentLineNumber})");
                                continue;
                            }

                            if (currentLine.Substring(7)[0] == '"') stringToPrint = currentLine.Substring(7).Trim().removeStringAbort();
                            else stringToPrint = getValueOfVariable(currentLine.Substring(7).Trim());
                            Write(stringToPrint);
                            continue;
                        }

                        // INPUTX call (getting input that is lowercased) [inputx <VARIABLE>]
                        if (currentLine.Substring(0, 6).ToLower() == "inputx")
                        {
                            if (currentLine.ToLower() == "inputx") abort($"INPUTX must be passed 1 argument! (Line Number: {currentLineNumber})");
                            string variableName = currentLine.Substring(7).Trim();
                            string userInput = ReadLine().ToLower();
                            setValueOfVariable(variableName, userInput);
                            continue;
                        }

                        // INPUTXX call (getting input that is uppercased) [inputxx <VARIABLE>]
                        if (currentLine.Substring(0, 7).ToLower() == "inputxx")
                        {
                            if (currentLine.ToLower() == "inputxx") abort($"INPUTXX must be passed 1 argument! (Line Number: {currentLineNumber})");
                            string variableName = currentLine.Substring(8).Trim();
                            string userInput = ReadLine().ToUpper();
                            setValueOfVariable(variableName, userInput);
                            continue;
                        }

                        // Invalid command handler
                        if (currentLine.Trim()[0].ToString() != "") abort($"Invalid command! (Line Number: {currentLineNumber}, Value: [{currentLine}])");

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
                Write($"{err.StackTrace}\n\n");
                abort($"Sorry, but Masker ran into an error. If your code looks completely fine, please make a issue on the Github repository. Provide this message along with the text shown above. " +
                    $"(Line Number: [{currentLineNumber}], Current Line In File: [{currentLine}])");
            }
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
                ForegroundColor = White;
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
            ForegroundColor = Red;
            Write("\n\nWARNING: ");
            ForegroundColor = White;
            WriteLine(warningMessage);
        }

        public static string removeStringAbort(this string stringToCheck)
        {
            stringToCheck = stringToCheck.Trim();
            if (stringToCheck[0] == '"' && stringToCheck[stringToCheck.Length - 1] == '"')
            {
                return stringToCheck.Substring(1, stringToCheck.Length - 2);
            }
            else
            {
                abort($"String value does not have quotation marks. (Line Number: {currentLineNumber}, Value: [{stringToCheck}])");
                return "Fatal Error!";
            }
        }
    }
}