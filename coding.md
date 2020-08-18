# Masker : Coding Tutorial
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
The only thing different is that in most programming languages you come across, you can't just put any item in the box. You have to tell the box what you are putting in.<br>
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

print <variable name>

And if you don't want the carriage return:

xprint <same as normal print values>
```