using Irony.Parsing;

namespace Blinkenlights.Basic.App;
public class BasicGrammar : Grammar
{
    public BasicGrammar()
    {
        var varName = new RegexBasedTerminal("varName", "[a-zA-Z][a-zA-Z0-9_]*");
        var integerValue = new RegexBasedTerminal("number", "-?[0-9]+");
        var quotedStringValue = new RegexBasedTerminal("quotedString", "\"(\\\"|.)*?\"");

        var additionExpression = new NonTerminal("additionExpression");
        var subtractionExpression = new NonTerminal("subtractionExpression");
        var multiplicationExpression = new NonTerminal("multiplicationExpression");
        var divisionExpression = new NonTerminal("divisionExpression");
        var parenthesisExpression = new NonTerminal("parenthesisExpression");

        RegisterOperators(1, "+", "-");
        RegisterOperators(2, "*", "/");
        
        var expression = new NonTerminal("expression")
        {
            Rule = integerValue |
                   varName |
                   additionExpression |
                   subtractionExpression |
                   multiplicationExpression |
                   divisionExpression |
                   parenthesisExpression
        };

        additionExpression.Rule = expression + "+" + expression;
        subtractionExpression.Rule = expression + "-" + expression;
        multiplicationExpression.Rule = expression + "*" + expression;
        divisionExpression.Rule = expression + "/" + expression;
        parenthesisExpression.Rule = "(" + expression + ")";

        var arg = new NonTerminal("arg")
        {
            Rule = quotedStringValue |
                   expression |
                   varName
        };
        var argSeparator = new RegexBasedTerminal("argSeparator", "[,;]");
        argSeparator.SetFlag(TermFlags.IsPunctuation, false);

        var equation = new NonTerminal("equation")
        {
            Rule = expression + "==" + expression |
                   expression + "!=" + expression |
                   expression + ">" + expression |
                   expression + "<" + expression |
                   expression + ">=" + expression |
                   expression + "<=" + expression
        };

        var endStatement = new NonTerminal("endStatement")
        {
            Rule = ToTerm("END")
        };
        var forStatement = new NonTerminal("forStatement")
        {
            Rule = ToTerm("FOR") + varName + "=" + integerValue + "TO" + integerValue
        };
        var gosubStatement = new NonTerminal("gosubStatement")
        {
            Rule = ToTerm("GOSUB") + integerValue
        };
        var gotoStatement = new NonTerminal("gotoStatement")
        {
            Rule = ToTerm("GOTO") + integerValue
        };
        var ifStatement = new NonTerminal("ifStatement")
        {
            Rule = ToTerm("IF") + equation + "THEN" + integerValue
        };
        var inputStatement = new NonTerminal("inputStatement")
        {
            Rule = ToTerm("INPUT") + varName
        };
        var letStatement = new NonTerminal("letStatement")
        {
            Rule = ToTerm("LET") + varName + "=" + expression
        };
        var nextStatement = new NonTerminal("nextStatement")
        {
            Rule = ToTerm("NEXT") + varName
        };

        // Seems we should be able to use MakePlusRule here, but argSeparator is left out of the tree
        // even though it is not marked as punctuation.
        var printArguments = new NonTerminal("printArguments");
        printArguments.Rule = arg | printArguments + argSeparator + arg;

        var printStatement = new NonTerminal("printStatement")
        {
            Rule = ToTerm("PRINT") + printArguments
        };
        
        var returnStatement = new NonTerminal("returnStatement")
        {
            Rule = ToTerm("RETURN")
        };

        var statement = new NonTerminal(Statement)
        {
            Rule = endStatement |
                   forStatement |
                   gosubStatement |
                   gotoStatement |
                   ifStatement |
                   inputStatement |
                   letStatement |
                   nextStatement |
                   printStatement |
                   returnStatement
        };

        var lineNum = new NonTerminal(LineNumber)
        {
            Rule = integerValue
        };

        var line = new NonTerminal("line")
        {
            Rule = lineNum + statement
        };

        var program = new NonTerminal("program");
        MakePlusRule(program, line);

        Root = program;
    }
    
    public static string LineNumber => "lineNum";
    public static string Statement => "statement";
}
