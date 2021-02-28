using System;

namespace PackCheck.Exceptions
{
    public class CsProjFileNotFoundException : Exception
    {
        public CsProjFileNotFoundException()
            : base($"Could not find a .csproj file in the current directory.")
        {
        }

        public CsProjFileNotFoundException(string message)
            : base(message)
        {
        }
    }
}