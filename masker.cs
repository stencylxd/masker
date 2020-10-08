// Masker by stencylxd, full codefile

#region Deps
using System;
using static System.Console;
using System.IO;
using System.Collections.Generic;
using static System.ConsoleColor;
using static System.String;
#endregion Deps

namespace Masker
{
    public static class MaskerEPL
    {
        #region Variable Definitions
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
        #endregion Variable Definitions
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    // Open file
                    string codeFilePath = args[0];
                    if (!File.Exists(codeFilePath)) abort("Masker: File does not exist.", false);
                    codeFile = new StreamReader(codeFilePath);
                    code = codeFile.ReadToEnd().Split('\n');
                    Clear();
                    // Tons of filters
                    foreach (string line in code)
                    {
                        currentLine = line.Trim();
                        currentLineNumber++;
                        errorLineNumber = currentLineNumber + 1;
                        // Empty handler
                        if (currentLine.Trim() == Empty) continue;
                        // To handle substring length errors when statement less than 2 chars is processed
                        if (currentLine.Length < 2) abort($"There are no statements less than 2 characters long! (Line Number: {errorLineNumber}, Value: {currentLine})");
                        // Checkpoint scanner
                        if (currentLine.Substring(0, 2).ToLower() == "cp")
                        {
                            if (currentLine.Trim().ToLower() == "cp") abort($"CP must be passed 1 argument! (Line Number: {errorLineNumber})");
                            string checkpointName = currentLine.Substring(3).Split(' ')[0];
                            if (jumpCheckpointNames.Contains(checkpointName)) abort($"Checkpoint already exists at line number " +
                                $"{jumpLineNumbers[jumpCheckpointNames.IndexOf(checkpointName)]} " +
                                $"(Line Number: {errorLineNumber}, Value: [{checkpointName}])");
                            jumpLineNumbers.Add(currentLineNumber);
                            jumpCheckpointNames.Add(currentLine.Substring(3).Split(' ')[0]);
                        }
                    }
                    currentLineNumber = -1;
                    // Main program loop
                    while (true)
                    {
                        // Clear keybuffer
                        while (KeyAvailable)
                        {
                            ConsoleKeyInfo key = ReadKey(true);
                        }

                        // Swap to next line in file
                        currentLineNumber++;
                        errorLineNumber = currentLineNumber + 1;
                        if (currentLineNumber > code.Length - 1) break;
                        currentLine = code[(int)currentLineNumber].Trim();

                        // Empty handler
                        if (currentLine.Trim() == Empty) continue;
                        // Code Comments <//>
                        if (currentLine.Length >= 2 && currentLine.Substring(0, 2) == "//") continue;              
                        // CP handler
                        if (currentLine.Length >= 3 && currentLine.Substring(0, 3).ToLower() == "cp ") continue;

                        // Statements
                        #region Variables
                        // VAR call (defining a variable) [var <VARIABLE> !"<VALUE>"]
                        if (currentLine.ToLower() == "var") abort($"VAR must be given atleast 1 argument! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 4 && currentLine.Substring(0, 4).ToLower() == "var ")
                        { 
                            string actualDefine = currentLine.Substring(4) + " ";
                            int indexOfSpace = actualDefine.IndexOf(" ");
                            string variableName = actualDefine.Substring(0, indexOfSpace);
                            string variableValue = actualDefine.Substring(indexOfSpace);
                            if (IsNullOrEmpty(variableValue.Trim())) variableValue = "NULL";
                            else variableValue = variableValue.removeStringAbort();
                            if (!variableExists(variableName)) newVariable(variableName, variableValue);
                            else abort($"Variable already exists. (Line Number: {errorLineNumber}, Variable Name: [{variableName}])");
                            continue;
                        }

                        // SET call (set value of existing variable) [set <VARIABLE> "<VALUE>"]
                        if (currentLine.ToLower() == "set") abort($"SET must be given 2 arguments! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 4 && currentLine.Substring(0, 4).ToLower() == "set ")
                        {
                            string actualStatement = currentLine.Substring(4).Trim();
                            int indexOfSpace = actualStatement.IndexOf(" ");
                            string variableName = actualStatement.Substring(0, indexOfSpace).Trim();
                            string variableValue = actualStatement.Substring(indexOfSpace).removeStringAbort();
                            setValueOfVariable(variableName, variableValue);
                            continue;
                        }

                        // ADD call (add value to number var) [add <VARIABLE> <VALUE>]
                        if (currentLine.ToLower() == "ADD") abort($"ADD must be given 2 arguments! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 4 && currentLine.Substring(0, 4).ToLower() == "add ")
                        {
                            string[] vals = currentLine.Substring(4).Split(" ");
                            string variableName = vals[0];
                            string value = vals[1];
                            int number;
                            int number2;
                            if (int.TryParse(value, out number) && int.TryParse(getValueOfVariable(variableName), out number2))
                            {
                                number += number2;
                                setValueOfVariable(variableName, number.ToString());
                            } else abort($"One of your given values was not a number. (Line Number: {errorLineNumber})");
                            continue;
                        }
                        #endregion Variables
                        #region Program Order
                        // EXIT call (exit program) [exit]
                        if (currentLine.ToLower() == "exit") Environment.Exit(0);

                        // GOTO call (jump to checkpoint) [GOTO <CHECKPOINT>] 
                        if (currentLine.Trim().ToLower() == "goto") abort($"GOTO must be passed 1 argument! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 5 && currentLine.Substring(0, 5).ToLower() == "goto ")
                        {
                            string checkpointName = currentLine.Substring(5).Trim();
                            if (!jumpCheckpointNames.Contains(checkpointName)) abort($"Checkpoint doesn't exist! " +
                                    $"(Line Number: {errorLineNumber}, Value: [{checkpointName}])");
                            float numberToJumpTo = jumpLineNumbers[jumpCheckpointNames.IndexOf(checkpointName)];
                            currentLine = code[(int)numberToJumpTo - 1];
                            currentLineNumber = numberToJumpTo - 1;
                            continue;
                        }

                        // GOTOIF call (conditional jump) [gotoif <value> <value> <checkpoint>]
                        if (currentLine.ToLower() == "gotoif") abort($"GOTOIF must be passed 3 arguments! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 7 && currentLine.Substring(0, 7).ToLower() == "gotoif ")
                        {
                            string argus = currentLine.Substring(6).Trim();
                            string[] argus2 = argus.Split(' ');
                            string value1;
                            string value2;
                            string checkpointName = argus2[^1];
                            if (argus[0] == '"')
                            {
                                value2 = argus2[^2];
                                value1 = argus.Substring(0, argus.Length - (value2.Length + 1 + checkpointName.Length)).Trim();
                            }
                            else
                            {
                                value1 = argus.Substring(0, argus.IndexOf(' ')).Trim();
                                value2 = argus.Substring(argus.IndexOf(' '), argus.Length - (checkpointName.Length + 1 + value1.Length)).Trim();
                            }
                            if (value1.removeStringAbort(true) != "no") value1 = value1.removeStringAbort();
                            else value1 = getValueOfVariable(value1);
                            if (value2.removeStringAbort(true) != "no") value2 = value2.removeStringAbort();
                            else value2 = getValueOfVariable(value2);

                            if (value1 == value2)
                            {
                                if (!jumpCheckpointNames.Contains(checkpointName)) abort($"Checkpoint doesn't exist! " +
                                    $"(Line Number: {errorLineNumber}, Value: [{checkpointName}])");
                                float numberToJumpTo = jumpLineNumbers[jumpCheckpointNames.IndexOf(checkpointName)];
                                currentLine = code[(int)numberToJumpTo - 1];
                                currentLineNumber = numberToJumpTo - 1;
                            }
                            continue;
                        }

                        // SLEEP call (sleep program for provided amount of seconds) [sleep <seconds>]
                        if (currentLine.ToLower() == "sleep") abort($"SLEEP must be given atleast 1 argument! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 6 && currentLine.Substring(0, 6).ToLower() == "sleep ")
                        {
                            int actualNumber;
                            if (!int.TryParse(currentLine.Substring(6).Trim(), out actualNumber)) abort($"Value is not number! (INT type!) (Line Number: {errorLineNumber})");
                            System.Threading.Thread.Sleep(actualNumber * 1000);
                            continue;
                        }

                        // SLEEPX call (sleep program for provided amount of milliseconds) [sleepx <milliseconds>]
                        if (currentLine.ToLower() == "sleepx") abort($"SLEEPX must be passed 1 arguments! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 7 && currentLine.Substring(0, 7).ToLower() == "sleepx ")
                        {
                            int actualNumber;
                            if (!int.TryParse(currentLine.Substring(6).Trim(), out actualNumber)) abort($"Value is not number! (INT type!) (Line Number: {errorLineNumber})");
                            System.Threading.Thread.Sleep(actualNumber);
                            continue;
                        }
                        #endregion Program Order
                        #region Graphics
                        // COLOR call (change color of text) [color <color name>]
                        if (currentLine.Trim().ToLower() == "color") abort($"COLOR must be passed 1 argument! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 6 && currentLine.Substring(0, 6).ToLower() == "color ")
                        {
                            string colorName = currentLine.Substring(5).Trim().ToLower();
                            ForegroundColor = stringColor(colorName);
                            continue;
                        }

                        // XCOLOR call (change background of text) [xcolor <color name>]
                        if (currentLine.Trim().ToLower() == "xcolor") abort($"XCOLOR must be passed 1 argument! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 7 && currentLine.Substring(0, 7).ToLower() == "xcolor ")
                        {
                            string colorName = currentLine.Substring(6).Trim().ToLower();
                            BackgroundColor = stringColor(colorName);
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

                        // CLEAR call (clear command line interface) [clear]
                        if (currentLine.ToLower() == "clear")
                        {
                            Clear();
                            continue;
                        }
                        #endregion Graphics
                        #region Input
                        // INPUT call (getting input) [input <VARIABLE>]
                        if (currentLine.ToLower() == "input") abort($"INPUT must be passed 1 argument! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 6 && currentLine.Substring(0, 6).ToLower() == "input ")
                        {
                            string variableName = currentLine.Substring(6).Trim();
                            string userInput = ReadLine();
                            setValueOfVariable(variableName, userInput);
                            continue;
                        }

                        // XINPUT call (getting input that is lowercased) [xinput <VARIABLE>]
                        if (currentLine.ToLower() == "xinput") abort($"XINPUT must be passed 1 argument! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 7 && currentLine.Substring(0, 7).ToLower() == "xinput ")
                        {
                            string variableName = currentLine.Substring(7).Trim();
                            string userInput = ReadLine().ToLower();
                            setValueOfVariable(variableName, userInput);
                            continue;
                        }

                        // READCHAR call (read one char to variable) [readchar <variable name>]
                        if (currentLine.ToLower() == "readchar") abort($"READCHAR must be given atleast 1 argument! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 9 && currentLine.Substring(0, 9).ToLower() == "readchar ")
                        {
                            while (!KeyAvailable) { }
                            string input = ReadKey(true).Key.ToString();
                            setValueOfVariable(currentLine.Substring(8).Trim(), input);
                            continue;
                        }
                        // XREADCHAR call (read one char to variable converted to lowercase) [xreadchar <variable name>]
                        if (currentLine.ToLower() == "xreadchar") abort($"XREADCHAR must be given atleast 1 argument! (Line Number: {errorLineNumber})");
                        if (currentLine.Length >= 10 && currentLine.Substring(0, 10).ToLower() == "xreadchar ")
                        {
                            while (!KeyAvailable) { }
                            string input = ReadKey(true).Key.ToString();
                            setValueOfVariable(currentLine.Substring(8).Trim(), input);
                            continue;
                        }

                        #endregion Input

                        // Invalid command handler
                        if (currentLine[0].ToString() != "") abort($"Invalid command! (Line Number: {errorLineNumber}, Value: [{currentLine}])");

                    }
                }
                // Abort if no file provided in args
                else abort("Masker: Please provide a file to run. (arguments)", false);
            }
            #region Fatal Error Handler
            catch (Exception err)
            {
                ResetColor();
                if (err is IOException) abort("Something changed about the running codefile! (Caught: IOException)");
                if (err is PathTooLongException) abort("Please enter a shorter file path. (Caught: PathTooLongException");
                if (err is NotSupportedException) abort("The file you gave cannot be read by Masker. (Caught: NotSupportedException)");
                if (err is UnauthorizedAccessException) abort("Masker does not have permission to read this file. (Caught: UnauthorizedAccessException)");
                WriteLine($"{err.Message}");
                Write($"{err.StackTrace}\n\n");
                abort($"Sorry, but Masker ran into an error.\n" + 
                "If your code looks completely fine, please make a issue on the Github repository.\n" +
                "Provide this message along with the text shown above.\n" +
                $"(Line Number: [{errorLineNumber}], Current Line In File: [{currentLine}])");
            }
            #endregion Fatal Error Handler
        }
        // Program Functions
        #region Variable Functions
        // These function's purposes are in the names.
        public static string getValueOfVariable(string variableName) // Get value of created variable
        {
            if (!variableExists(variableName)) abort($"Variable does not exist. (Line Number: {errorLineNumber}, Value: {variableName})");
            int index = variableNames.IndexOf(variableName);
            string variableValue = variableValues[index];
            return variableValue;
        }

        public static void setValueOfVariable(string variableName, string newValue) // Set value of created variable
        {
            if (!variableExists(variableName)) abort($"Variable does not exist. (Line Number: {errorLineNumber}, Value: {variableName})");
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

        public static int newVariable(string variableName, string variableValue) // Make new variable in database**
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
        #endregion Variable Functions
        #region Error Handler Functions
        // Abort with error message
        public static void abort(string errMessage, bool error = true, int exitCode = 0) // Close program due to error
        {
            ResetColor();
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

        // Warn user of problem
        public static void warn(string warningMessage) // Warn without closing program
        {
            ConsoleColor backupColorF = ForegroundColor;
            ConsoleColor backupcolorB = BackgroundColor;
            ResetColor();
            ForegroundColor = Red;
            Write("\n\nWARNING: ");
            ResetColor();
            WriteLine(warningMessage);
            ForegroundColor = backupColorF;
            BackgroundColor = backupColorB
        }
        #endregion Error Handler Functions
        #region Misc Functions
        // Function to remove quotation marks from strings
        public static string removeStringAbort(this string stringToCheck, bool justCheck = false)
        {
            stringToCheck = stringToCheck.Trim();
            if (stringToCheck[0] == '"' && stringToCheck[stringToCheck.Length - 1] == '"')
            {
                if (!justCheck) return stringToCheck.Substring(1, stringToCheck.Length - 2);
                return "yes";
            }
            else
            {
                if (!justCheck) abort($"String value does not have quotation marks. (Line Number: {errorLineNumber}, Value: [{stringToCheck}])");
                return "no";
            }
        }

        // Get color from string
        public static ConsoleColor stringColor(string color)
        {
            switch (color)
            {
                case "blue": return Blue;
                case "red": return Red;
                case "magenta": return Magenta;
                case "yellow": return Yellow;
                case "cyan": return Cyan;
                case "gray": return Gray;
                case "green": return Green;
                case "darkred": return DarkRed;
                case "darkblue": return DarkBlue;
                case "darkgreen": return DarkGreen;
                case "darkyellow": return DarkYellow;
                case "darkgray": return DarkGray;
                case "darkcyan": return DarkCyan;
                case "darkmagenta": return DarkMagenta;
                case "black": return Black;
                default: return White; // White if invalid color specified
            }
        }
#endregion Misc Functions
    }
}
