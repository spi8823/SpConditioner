This package is a parsing library for simplified expressions with variables, similar to conditional expressions in if statements.

# Usage

## Basic Usage

Use the `StatementParser.ParseToBool` function to parse a statement and get the result as a `bool` value.

```csharp
using SpConditioner;

var statement = "2 * 3 == 6";
var result = StatementParser.ParseToBool(statement);
Assert.AreEqual(result, true);  // 2 * 3 == 6 => true
```

## Working with Variables

To work with variables, first implement a class that adheres to the IVariableAccessor interface. For convenience, a VariableDictionary class, which extends Dictionary, is provided.

``` csharp
var dic = new VariableDictionary();
dic["two"] = 2;
dic["three"] = 3;
statement = "two + three == 5";
result = StatementParser.ParseToBool(statement, dic);
Assert.AreEqual(result, true);  // 2 + 3 == 5 => true

dic["flag_1"] = true;
dic["flag_2"] = false;
statement = "flag_1 && flag_2";
result = StatementParser.ParseToBool(statement, dic);
Assert.AreEqual(result, false); // true && false => false
```

# Issues and Feedback
If you encounter any issues or have feature requests, please open an issue on our GitHub repository.

Issues: https://github.com/spi8823/SpConditioner/issues
We appreciate your feedback and will do our best to address any concerns.