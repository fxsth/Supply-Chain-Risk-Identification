using System;

namespace SCRI.Exceptions
{
    public class InvalidGraphElementException:Exception
    {
        private string _message;
        public InvalidGraphElementException(string message)
        {
            _message = message;
        }
    }
}