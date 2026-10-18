using System;

namespace PackCheck.Exceptions;

public class NuGetFeedException(string message, Exception innerException) : Exception(message, innerException);
