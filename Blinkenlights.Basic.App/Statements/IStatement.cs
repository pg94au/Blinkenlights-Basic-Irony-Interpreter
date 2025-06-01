using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blinkenlights.Basic.App.Statements;
public interface IStatement
{
    void Execute(Interpreter interpreter);
}
