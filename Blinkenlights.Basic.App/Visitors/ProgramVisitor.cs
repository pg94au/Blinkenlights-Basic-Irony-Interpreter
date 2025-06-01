using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blinkenlights.Basic.App.Statements;
using Irony.Parsing;

namespace Blinkenlights.Basic.App.Visitors;
public class ProgramVisitor
{
    public SortedDictionary<int, IStatement> Statements { get; } = new SortedDictionary<int, IStatement>();
    private int _currentLineNumber;


    //TODO: This may be a dead end because the tree nodes are concrete and do not accept visitors.
    public string Visit(ParseTreeNode node)
    {
        return null!;
    }
}
