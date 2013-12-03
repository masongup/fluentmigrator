using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner;

namespace FluentMigrator.WpfGui
{
    public class OutputCatcher : IAnnouncer
    {
        private readonly StringBuilder _totalOutput;

        public OutputCatcher()
        {
            _totalOutput = new StringBuilder();
        }

        public string Output { get { return _totalOutput.ToString(); } }

        public void Heading(string message)
        {
            _totalOutput.AppendLine(message);
        }

        public void Say(string message)
        {
            if (message == "Task completed." || message.StartsWith("Using Database"))
                return;
            _totalOutput.AppendLine(message);
        }

        public void Emphasize(string message)
        {
            _totalOutput.AppendLine(message);
        }

        public void Sql(string sql)
        {
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
        }

        public void Error(string message)
        {
            _totalOutput.AppendLine(message);
        }

        public void Write(string message, bool escaped)
        {
            _totalOutput.AppendLine(message);
        }
    }
}
