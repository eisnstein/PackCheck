using System;

namespace PackCheck.Exceptions;

public class SolutionFileException(string message) : Exception(message);
