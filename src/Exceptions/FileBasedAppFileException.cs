using System;

namespace PackCheck.Exceptions;

public class FileBasedAppFileException(string message) : Exception(message);
