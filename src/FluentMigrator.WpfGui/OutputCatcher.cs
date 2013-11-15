using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner;

namespace FluentMigrator.WpfGui
{
    public class OutputCatcher : IAnnouncer
    {
        private StringBuilder _totalOutput;

        public OutputCatcher()
        {
            _totalOutput = new StringBuilder();
        }

        public string Output { get { return _totalOutput.ToString(); } }

        public void Heading(string message)
        {
            _totalOutput.Append(message);
        }

        public void Say(string message)
        {
            if (message == "Task completed." || message.StartsWith("Using Database"))
                return;
            _totalOutput.Append(message);
        }

        public void Emphasize(string message)
        {
            _totalOutput.Append(message);
        }

        public void Sql(string sql)
        {
            _totalOutput.Append(sql);
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
            _totalOutput.Append(timeSpan);
        }

        public void Error(string message)
        {
            _totalOutput.Append(message);
        }

        public void Write(string message, bool escaped)
        {
            _totalOutput.Append(message);
        }
    }
}
