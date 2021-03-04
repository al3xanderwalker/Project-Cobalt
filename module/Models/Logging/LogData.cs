using System;

namespace Project_Cobalt.Models
{
    public class LogData
    {
        public string Value;
        public ConsoleColor Color;

        public LogData(string value, ConsoleColor color)
        {
            Value = value;
            Color = color;
        }
    }
}