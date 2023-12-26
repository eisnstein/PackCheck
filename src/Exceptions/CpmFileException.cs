using System;

namespace PackCheck.Exceptions;

public class CpmFileException(string message) : Exception(message);
