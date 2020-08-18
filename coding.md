# Coding Tutorial
This page will teach you how to code in Masker.<br>
If you are having trouble understanding, [come in contact with me](mailto:mail@stencylxd.ga) and tell me what I should fix.
## Simple Data Types / Variables
To understand basic programming, you need to know data types and variables.<br>
There are 4 main types that are used:
- Strings (a long sequence of characters)
- Integers (a number type that doesn't support decimal points)
- Chars (single characters)
- Booleans (true or false)

Here is an example of each of them, in order:
```
String:  "This is a string, I am surrounded by quotation marks!"
Number:  100
Char:    'A'
Boolean: true
```
Now that you know data types, you can learn variables.<br>
Think of a variable as a box. When you put something in the box, it stays there until you change the item in it.<br>
Now just convert the items that you put in the box to values, and the box to a variable.<br>
The only thing different is that in most languages you come across, you can't just put anything in the box. You have to tell the box what you are putting in.<br>
This is where the data types come in. Data types are the thing telling the box what you are putting in.<br>
For example: If I gave the box the data type of string, it would only take strings.<br>
But don't worry, you don't have to do that stuff in Masker. Every variable you make is automatically assigned the string data type.

## Statements
Now that you know variables and data types, it's time for the meat and potatos.<br>
Statements are what make up your code. They are very important.<br>
As of right now, there are 17 statements in Masker.<br>
This section will tell you the syntax for all of the statements.
<br><br>
To write code comments:<br>
(explination: code comments are not executed, they are comments that help people understand what code does. they can also be helpful markers.)
```
Put 2 forward slashes at the beginning of the line.
// <your comment text here>
```

Basic Commands:
```
// exit command (will exit program)
exit

// color command (will set foreground color of text being printed)
color <color name>
// to set background color:
xcolor <color name>

// sleep command (will pause program execution for an amount of time)
sleep <number of seconds>
// for milliseconds:
sleepx <number of milliseconds>
```

Making variables:
```
var <your variable name here> <value>

Also, setting the value is optional.
The defualt value is "NULL" (if you don't set it).
```

Working with variables:
```
To set the value of a variable:
set <variable name> <value>

To add a number to a variable:
(to do this, your variable must be a string representation of a integer)
add <variable> <number with no quotation marks>

```

Putting things on the screen:
```
print "your cool text here"

OR:

print <variable name to print variable value>

And if you don't want the carriage return:

xprint <same as normal print values>
```

Getting input:
```
// Must create a variable first.
var user_input

// Syntax:
input <variable name>

// This will get input from the user and put the value in the variable you provided.
// Do you want the user input to be converted to uppercase or lowercase?

// xinput = input will be lowercased
// xxinput = input will be uppercased
// input = no processing will happen to input
```

## Checkpoints, Conditionals, and Loops
These are some of the more complicated stuff, but once you know it it's a piece of cake.
### Checkpoints and Gotos

Checkpoints are little markers.<br>
When you place one down, you can always go back to it.<br>

Example:
```
// To make a checkpoint, use the statement "CP"

cp <your chosen checkpoint name>

// Here is a simple example of a checkpoint being made:

cp start

// Now here's another statement:

goto <checkpoint name>

// This statement will goto the line where you created your checkpoint and continue from there.
// Guess what this code does:

cp infinite_loop

print "haha poop piss fart"

goto infinite_loop

// The code above is an infinite loop because it will keep on executing the goto statement with no stopping.
// But what about this:

cp noninf_loop

goto exit

goto noninf_loop

cp exit

// This is not infinite because it breaks out of the loop by gotoing a statement outside of the loop.
```

### Conditionals

Conditionals are statements that do something if a conditional statement is true.<br>
For example:

**if this is equal to that then do something**

In Masker, there are conditional goto statements. Lets have a look:

```

// Syntax:
gotoif <value1> <value2> <checkpoint>

// If value1 is equal to value2 then it will goto the checkpoint.
// Otherwise, it won't do anything.

// Example of it being used:

var user_input
var password "12345"

xprint "Enter your Password: "
input user_input

gotoif user_input password correct
print "You got it incorrect. Ouch!"
exit

cp correct
print "Wow, you got it correct!"
```

##### Class dismissed. [Go back to main page](https://stencylxd.github.io/masker).