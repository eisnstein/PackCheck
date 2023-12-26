using System;

namespace PackCheck.Exceptions;

public class CsProjFileException(string message) : Exception(message);
