using System;

namespace PackCheck.Exceptions;

public class SolutionFileException : Exception
{
    public SolutionFileException(string message)
        : base(message)
    {
    }
}
