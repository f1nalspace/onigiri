using Finalspace.Onigiri.Enums;
using Finalspace.Onigiri.MVVM;

namespace Finalspace.Onigiri.Models
{
    public class Issue : BindableBase
    {
        public IssueKind Kind
        {
            get { return GetValue(() => Kind); }
            private set { SetValue(() => Kind, value); }
        }

        public bool IsSolved
        {
            get { return GetValue(() => IsSolved); }
            set { SetValue(() => IsSolved, value); }
        }

        public string Message
        {
            get { return GetValue(() => Message); }
            private set { SetValue(() => Message, value); }
        }

        public string Path
        {
            get { return GetValue(() => Path); }
            private set { SetValue(() => Path, value); }
        }

        public object Value
        {
            get { return GetValue(() => Value); }
            private set { SetValue(() => Value, value); }
        }

        public Issue(IssueKind kind, string message, string path, object value)
        {
            Kind = kind;
            Message = message;
            Path = path;
            Value = value;
            IsSolved = false;
        }
    }
}
