using Blinkenlights.Basic.App.Equations;
using Blinkenlights.Basic.App.Expressions;
using Blinkenlights.Basic.App.Statements;
using Irony;
using Irony.Parsing;

namespace Blinkenlights.Basic.App;
public class StatementParser
{
    private readonly Dictionary<string, Func<ParseTreeNode, IStatement>> _statementParseFuncs = new()
    {
        { "endStatement", ParseEndStatement },
        { "forStatement", ParseForStatement },
        { "gosubStatement", ParseGosubStatement },
        { "gotoStatement", ParseGotoStatement },
        { "ifStatement", ParseIfStatement },
        { "inputStatement", ParseInputStatement },
        { "letStatement", ParseLetStatement },
        { "nextStatement", ParseNextStatement },
        { "printStatement", ParsePrintStatement },
        { "returnStatement", ParseReturnStatement }
    };
    
    public SortedDictionary<int, IStatement>? Parse(string source, TextWriter output, TextWriter error)
    {
        var grammar = new BasicGrammar();
        var language = new LanguageData(grammar);
        var parser = new Parser(language);
        var parseTree = parser.Parse(source);
        var root = parseTree.Root;

        foreach (var message in parseTree.ParserMessages)
        {
            switch (message.Level)
            {
                case ErrorLevel.Error:
                    error.WriteLine($"Error: {message.Message} at {message.Location}");
                    break;
                case ErrorLevel.Warning:
                    error.WriteLine($"Warning: {message.Message} at {message.Location}");
                    break;
                default:
                    output.WriteLine($"Info: {message.Message} at {message.Location}");
                    break;
            }
        }
        
        if (parseTree.HasErrors())
        {
            error.WriteLine("Failed: Parsing failed with errors.");
            return new SortedDictionary<int, IStatement>();
        }

        var statements = ParseStatements(root);

        return statements;
    }

    private SortedDictionary<int, IStatement> ParseStatements(ParseTreeNode root)
    {
        var statements = new SortedDictionary<int, IStatement>();

        foreach (var node in root.ChildNodes)
        {
            switch (node.Term.Name)
            {
                case "line":
                    ParseLine(node, statements);
                    break;
            }
        }

        return statements;
    }

    private void ParseLine(ParseTreeNode node, SortedDictionary<int, IStatement> statements)
    {
        var lineNumberNode = node.ChildNodes[0];
        var statementNode = node.ChildNodes[1];

        var lineNumber = LineNumberParser.Parse(lineNumberNode);
        var statementParseFunc = _statementParseFuncs[statementNode.ChildNodes[0].Term.Name];

        var statement = statementParseFunc(statementNode.ChildNodes[0]);

        statements[lineNumber] = statement;
    }

    public static IStatement ParseEndStatement(ParseTreeNode node)
    {
        return new EndStatement();
    }

    public static IStatement ParseForStatement(ParseTreeNode node)
    {
        var variableNode = node.ChildNodes[1];
        var startValueNode = node.ChildNodes[3];
        var toValueNode = node.ChildNodes[5];
        var variableName = variableNode.Token.ValueString;
        var startValue = int.Parse(startValueNode.Token.ValueString);
        var toValue = int.Parse(toValueNode.Token.ValueString);

        return new ForStatement(variableName, startValue, toValue);
    }

    public static IStatement ParseGosubStatement(ParseTreeNode node)
    {
        var gosubLineNumberNode = node.ChildNodes[1];
        var gosubLineNumber = int.Parse(gosubLineNumberNode.Token.ValueString);

        return new GosubStatement(gosubLineNumber);
    }

    public static IStatement ParseGotoStatement(ParseTreeNode node)
    {
        var gotoLineNumberNode = node.ChildNodes[1];
        var gotoLineNumber = int.Parse(gotoLineNumberNode.Token.ValueString);

        return new GotoStatement(gotoLineNumber);
    }

    public static IStatement ParseIfStatement(ParseTreeNode node)
    {
        var equationNode = node.ChildNodes[1];
        var targetLineNumberNode = node.ChildNodes[3];
        var equation = EquationParser.Parse(equationNode);
        var targetLineNumber = int.Parse(targetLineNumberNode.Token.ValueString);

        return new IfStatement(equation, targetLineNumber);
    }

    public static IStatement ParseInputStatement(ParseTreeNode node)
    {
        var variableNode = node.ChildNodes[1];
        var variableName = variableNode.Token.ValueString;

        return new InputStatement(variableName);
    }

    public static IStatement ParseLetStatement(ParseTreeNode node)
    {
        var variableNode = node.ChildNodes[1];
        var expressionNode = node.ChildNodes[3];
        var variableName = variableNode.Token.ValueString;
        var expression = ExpressionParser.Parse(expressionNode);

        return new LetStatement(variableName, expression);
    }

    public static IStatement ParseNextStatement(ParseTreeNode node)
    {
        var variableNode = node.ChildNodes[1];
        var variableName = variableNode.Token.ValueString;

        return new NextStatement(variableName);
    }

    public static IStatement ParsePrintStatement(ParseTreeNode node)
    {
        var printArguments = PrintArgumentParser.Parse(node.ChildNodes[1]);

        return new PrintStatement(printArguments);
    }

    public static IStatement ParseReturnStatement(ParseTreeNode node)
    {
        return new ReturnStatement();
    }
}


public class LineNumberParser
{
    public static int Parse(ParseTreeNode node)
    {
        var lineNumberNode = node.ChildNodes[0];

        var lineNumber = int.Parse(lineNumberNode.Token.ValueString);

        return lineNumber;
    }
}

public interface IStatementParser
{
    IStatement Parse(ParseTreeNode node);
}

public class EquationParser
{
    public static IEquation Parse(ParseTreeNode node)
    {
        var leftExpressionNode = node.ChildNodes[0];
        var inequalityNode = node.ChildNodes[1];
        var rightExpressionNode = node.ChildNodes[2];

        var leftExpression = ExpressionParser.Parse(leftExpressionNode);
        var inequality = inequalityNode.Token.ValueString;
        var rightExpression = ExpressionParser.Parse(rightExpressionNode);
        
        return inequality switch
        {
            "==" => new EqualsEquation(leftExpression, rightExpression),
            "!=" => new DoesNotEqualEquation(leftExpression, rightExpression),
            "<" => new LessThanEquation(leftExpression, rightExpression),
            ">" => new GreaterThanEquation(leftExpression, rightExpression),
            "<=" => new LessThanOrEqualEquation(leftExpression, rightExpression),
            ">=" => new GreaterThanOrEqualEquation(leftExpression, rightExpression),
            _ => throw new NotImplementedException($"Unknown operator: {inequality}")
        };
    }
}

public class ExpressionParser
{
    public static IExpression Parse(ParseTreeNode node)
    {
        var expressionNode = node.ChildNodes[0];
        switch (expressionNode.Term.Name)
        {
            case "number":
                var numberValue = int.Parse(expressionNode.Token.ValueString);
                
                return new NumberExpression(numberValue);
            case "varName":
                var variableName = expressionNode.Token.ValueString;

                return new VariableExpression(variableName);
            case "additionExpression":
                var additionLeft = Parse(expressionNode.ChildNodes[0]);
                var additionRight = Parse(expressionNode.ChildNodes[2]);

                return new AdditionExpression(additionLeft, additionRight);
            case "subtractionExpression":
                var subtractionLeft = Parse(expressionNode.ChildNodes[0]);
                var subtractionRight = Parse(expressionNode.ChildNodes[2]);

                return new SubtractionExpression(subtractionLeft, subtractionRight);
            case "multiplicationExpression":
                var multiplicationLeft = Parse(expressionNode.ChildNodes[0]);
                var multiplicationRight = Parse(expressionNode.ChildNodes[2]);

                return new MultiplicationExpression(multiplicationLeft, multiplicationRight);
            case "divisionExpression":
                var divisionLeft = Parse(expressionNode.ChildNodes[0]);
                var divisionRight = Parse(expressionNode.ChildNodes[2]);
 
                return new DivisionExpression(divisionLeft, divisionRight);
            case "parenthesisExpression":
                var parenthesisExpression = Parse(expressionNode.ChildNodes[1]);

                return parenthesisExpression;
        }
        
        throw new NotImplementedException();
    }
}

public class PrintArgumentParser
{
    public static PrintArgument[] Parse(ParseTreeNode node)
    {
        var printArguments = new List<PrintArgument>();

        foreach (var argNode in node.ChildNodes)
        {
            switch (argNode.Term.Name)
            {
                case "printArguments":
                    var newPrintArguments = Parse(argNode);
                    printArguments.AddRange(newPrintArguments);
                    break;
                case "argSeparator":
                    switch (argNode.Token.ValueString)
                    {
                        case ",":
                            printArguments.Add(PrintArgument.FromText(" "));
                            break;
                        case ";":
                            // No space, no argument added.
                            break;
                    }
                    break;
                case "arg":
                    var printArgumentNode = argNode.ChildNodes[0];
                    switch (printArgumentNode.Term.Name)
                    {
                        case "quotedString":
                            var quotedString = printArgumentNode.Token.ValueString;
                            quotedString = quotedString.Substring(1, quotedString.Length - 2);
                            var printArgument = PrintArgument.FromText(quotedString);
                            printArguments.Add(printArgument);
                            break;
                        case "expression":
                            var expression = ExpressionParser.Parse(printArgumentNode);
                            printArgument = PrintArgument.FromExpression(expression);
                            printArguments.Add(printArgument);
                            break;
                    }
                    break;
            }

        }

        return printArguments.ToArray();
    }
}
