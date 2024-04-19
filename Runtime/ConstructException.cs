using System;

namespace TanitakaTech.NestedDIContainer
{
    public class ConstructException : Exception
    {
        public ConstructException() { }
        public ConstructException(string message) : base(message) { }
    }
}