using DevExpress.Mvvm;
using Finalspace.Onigiri.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Finalspace.Onigiri.ViewModels
{
    public class SortItemViewModel : ViewModelBase, IEquatable<SortItemViewModel>
    {
        public string DisplayName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public AnimeSortKey Value
        {
            get => GetValue<AnimeSortKey>();
            set => SetValue(value);
        }

        public bool Equals(SortItemViewModel other)
        {
            if (other is null) return false;
            return Value == other.Value;
        }

        public override int GetHashCode() => Value.GetHashCode();
        public override bool Equals(object obj) => obj is SortItemViewModel other && Equals(other);
    }
}
