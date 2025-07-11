﻿using NUnit.Framework;

namespace Blinkenlights.Basic.Tests.Statements;

[TestFixture]
public class PrintStatementTests
{
    [Test]
    public void CanPrintLiteralString()
    {
        @"
                10 PRINT ""HELLO""
                20 END
            ".ExecuteWithOutput(out var output);

        Assert.That(output, Is.EqualTo($"HELLO{Environment.NewLine}"));
    }

    [Test]
    public void CanPrintExpression()
    {
        @"
                10 PRINT 12 + 23
                20 END
            ".ExecuteWithOutput(out var output);

        Assert.That(output, Is.EqualTo($"35{Environment.NewLine}"));
    }

    [Test]
    public void CanPrintVariable()
    {
        @"
                10 LET X=123
                20 PRINT X
                30 END
            ".ExecuteWithOutput(out var output);

        Assert.That(output, Is.EqualTo($"123{Environment.NewLine}"));
    }

    [Test]
    public void CommaIncludesASpaceBetweenPrintedArguments()
    {
        @"
                10 PRINT ""Hello"", ""there""
                20 END
            ".ExecuteWithOutput(out var output);

        Assert.That(output, Is.EqualTo($"Hello there{Environment.NewLine}"));
    }

    [Test]
    public void SemicolonDirectlyAppendsPrintedArguments()
    {
        @"
                10 PRINT ""Hello""; ""there""
                20 END
            ".ExecuteWithOutput(out var output);

        Assert.That(output, Is.EqualTo($"Hellothere{Environment.NewLine}"));
    }
}
