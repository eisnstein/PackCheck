using System;

namespace PackCheck.Exceptions;

public class CpmFileException : Exception
{
    public CpmFileException(string message)
        : base(message)
    {
    }
}
