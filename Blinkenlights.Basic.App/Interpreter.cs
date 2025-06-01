using Blinkenlights.Basic.App.Statements;

namespace Blinkenlights.Basic.App;

public class Interpreter
{
    public TextReader InputReader { get; }
    public TextWriter OutputWriter { get; }
    public TextWriter ErrorWriter { get; }

    private int _currentLineNumber;
    private readonly SortedDictionary<int, IStatement> _statements;
    private readonly Stack<int> _stack = new();
    private readonly Stack<ForState> _forStack = new();
    private readonly Dictionary<string, int> _variables = new();
    private bool _running = false;

    public Interpreter(string program) : this(program, Console.In, Console.Out, Console.Error)
    {
    }

    public Interpreter(string program, TextReader inputReader, TextWriter outputWriter, TextWriter errorWriter)
        : this(ParseStatementsFromProgramText(program, outputWriter, errorWriter), inputReader, outputWriter, errorWriter)
    {
    }

    public Interpreter(SortedDictionary<int, IStatement> statements, TextReader inputReader, TextWriter outputWriter,
        TextWriter errorWriter)
    {
        _statements = statements;
        _currentLineNumber = statements.FirstOrDefault().Key;
        InputReader = inputReader;
        OutputWriter = outputWriter;
        ErrorWriter = errorWriter;
    }

    public void AdvanceLine()
    {
        var currentIndex = _statements.Keys.ToList().BinarySearch(_currentLineNumber);

        _currentLineNumber = _statements.Keys.ElementAtOrDefault(currentIndex + 1) != 0
            ? _statements.Keys.ElementAtOrDefault(currentIndex + 1)
            : int.MaxValue;
    }

    public ForState CreateForState(string variableName, int limit)
    {
        return new ForState(variableName, limit, _currentLineNumber);
    }

    public void End()
    {
        _currentLineNumber = Int32.MaxValue;
    }

    public void ExecuteProgram()
    {
        _running = true;

        while (_running && !Finished)
        {
            if (_statements.TryGetValue(_currentLineNumber, out var statement))
            {
                statement.Execute(this);
            }
            else
            {
                _running = false;
            }
        }
    }

    public bool Finished => _currentLineNumber == int.MaxValue;

    public void GotoLine(int targetLine)
    {
        if (_statements.ContainsKey(targetLine))
        {
            _currentLineNumber = targetLine;
        }
        else
        {
            throw new ArgumentException();
        }
    }

    public ForState PopForLoop()
    {
        return _forStack.Pop();
    }

    private static SortedDictionary<int, IStatement>? ParseStatementsFromProgramText(string program, TextWriter outputwriter, TextWriter errorWriter)
    {
        var statementParser = new StatementParser();
        var statements = statementParser.Parse(program, outputwriter, errorWriter);

        return statements;
    }

    public void PopLineNumber()
    {
        if (!_stack.TryPop(out _currentLineNumber))
        {
            throw new InvalidOperationException();
        }
    }

    public void PushForLoop(ForState forState)
    {
        _forStack.Push(forState);
    }

    public void PushLineNumber()
    {
        _stack.Push(_currentLineNumber);
    }

    public int ReadVariable(string variableName)
    {
        return _variables[variableName];
    }

    public void Stop()
    {
        ErrorWriter.WriteLine($"! Stopped at line {_currentLineNumber}");
        _running = false;
    }

    public void WriteVariable(string variableName, int value)
    {
        _variables[variableName] = value;
    }
}
