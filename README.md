SymbolicEval
============

A tree-based **symbolic** evaluating program, with a Windows and an Android UI.

## How to use it
```C#
Scanner scan = new Scanner(theTextInputYouWant);
Parser parser = new Parser(scan);
try
{
    Tree t = parser.Parse();
    output = t.Print();
    var steps = t.Simplify();
    foreach (var step in steps)
    {
      output += "\n=" + step;
    }
} catch (Exception)
{
    output = "ERROR";
}
```
