using System;

namespace TactileLibrary
{
    // find explicit maybe casts and change them
    public struct Maybe<T>
    {
        private bool _isSomething;
        private readonly T _value;

        #region Accessors
        public bool IsSomething { get { return _isSomething; } }
        public bool IsNothing { get { return !_isSomething; } }

        public T ValueOrDefault
        {
            get
            {
                if (IsNothing)
                    return default(T);
                return _value;
            }
        }

        public static Maybe<T> Nothing { get { return default(Maybe<T>); } }
        #endregion

        public override string ToString()
        {
            if (_isSomething)
                return ((T)this).ToString();
            else
                return "Nothing";
        }

        /// <summary>
        /// Creates a maybe object that is not Nothing.
        /// </summary>
        internal Maybe(T value) : this(true, value) { }
        private Maybe(bool something, T value)
        {
            _isSomething = something;
            _value = value;
        }

        public static implicit operator Maybe<T>(T value)
        {
            return new Maybe<T>(value);
        }
        public static implicit operator T(Maybe<T> value)
        {
            if (value.IsNothing)
                throw new ArgumentException("Tried to convert a Maybe<> with no value into a value.");
            return value._value;
        }

        public T OrIfNothing(T value)
        {
            if (this.IsSomething)
                return _value;
            return value;
        }

        public override int GetHashCode()
        {
            return IsNothing ? 0 : ReferenceEquals(_value, null) ? -1 : -_value.GetHashCode();
        }

        public static bool operator !=(Maybe<T> maybe1, Maybe<T> maybe2)
        {
            return !maybe1.Equals(maybe2);
        }
        public static bool operator !=(Maybe<T> maybe, T value)
        {
            return !maybe.Equals(value);
        }
        public static bool operator !=(T value, Maybe<T> maybe)
        {
            return !maybe.Equals(value);
        }

        public static bool operator ==(Maybe<T> maybe1, Maybe<T> maybe2)
        {
            return maybe1.Equals(maybe2);
        }
        public static bool operator ==(Maybe<T> maybe, T value)
        {
            return maybe.Equals(value);
        }
        public static bool operator ==(T value, Maybe<T> maybe)
        {
            return maybe.Equals(value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Maybe<T> || obj is T)
                return Equals((Maybe<T>)obj);
            return false;
        }
        public bool Equals(Maybe<T> other)
        {
            if (IsSomething != other.IsSomething)
                return false;
            return IsNothing || Equals(_value, other._value);
        }
    }
}
