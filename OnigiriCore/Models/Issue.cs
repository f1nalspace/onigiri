using DevExpress.Mvvm;
using Finalspace.Onigiri.Enums;

namespace Finalspace.Onigiri.Models
{
    public class Issue : BindableBase
    {
        public IssueKind Kind
        {
            get => GetValue<IssueKind>();
            private set => SetValue(value);
        }

        public bool IsSolved
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public string Message
        {
            get => GetValue<string>();
            private set => SetValue(value);
        }

        public string Path
        {
            get => GetValue<string>();
            private set => SetValue(value);
        }

        public object Value
        {
            get => GetValue<object>();
            private set => SetValue(value);
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
