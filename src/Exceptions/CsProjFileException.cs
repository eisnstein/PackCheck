using System;

namespace PackCheck.Exceptions;

public class CsProjFileException : Exception
{
    public CsProjFileException(string message)
        : base(message)
    {
    }
}
