using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable InheritdocConsiderUsage
// ReSharper disable RedundantNameQualifier
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ReferenceEqualsWithValueType
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable RedundantCast.0

namespace HoU.GuildBot.Shared.StrongTypes
{
	/// <summary>
	/// Implements the strong type <see cref="InternalUserID" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "1.0.0")]
	[Serializable]
	public partial struct InternalUserID : IEquatable<InternalUserID>, IComparable<InternalUserID>, ISerializable, IXmlSerializable
	{
		/// <summary>
		/// Actual backing field which holds the value.
		/// </summary>
		/// <remarks>This field is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
		private System.Int32 _value;

		[ExcludeFromCodeCoverage]
		private InternalUserID(System.Int32 value)
	    {
	        _value = value;
	    }

		[ExcludeFromCodeCoverage]
		private InternalUserID(SerializationInfo info, StreamingContext context)
		{
            _value = (System.Int32)info.GetValue("v", typeof(System.Int32));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="InternalUserID"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator InternalUserID(System.Int32 value)
	    {
	        return new InternalUserID(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.Int32"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.Int32(InternalUserID value)
	    {
	        return value._value;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="InternalUserID"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(InternalUserID other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _value == other._value;
		}
		
		/// <summary>
        /// Returns a value indicating whether this instance and a specified object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public override bool Equals(object other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals((InternalUserID)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="InternalUserID"/>.</returns>
		[ExcludeFromCodeCoverage]
		public override int GetHashCode()
		{
			if (Equals(null, _value))
				return 0;
			return _value.GetHashCode();
		}

		/// <summary>
        /// Compares this instance to a specified <see cref="InternalUserID"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="InternalUserID"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(InternalUserID other)
		{
			if (Equals(null, _value))
				return 1;
			return _value.CompareTo(other._value);
		}

        /// <summary>
        /// Checks if both operands are equal.
        /// </summary>
        /// <param name="first">The first operand.</param>
        /// <param name="second">The second operand.</param>
        /// <returns><b>True</b>, if both operands are equal; otherwise, <b>false</b>.</returns>
        [ExcludeFromCodeCoverage]
        public static bool operator ==(InternalUserID first, InternalUserID second)
	    {
	        if (ReferenceEquals(first, second))
	            return true;

	        return first._value == second._value;
	    }

        /// <summary>
        /// Checks if both operands are not equal.
        /// </summary>
        /// <param name="first">The first operand.</param>
        /// <param name="second">The second operand.</param>
        /// <returns><b>False</b>, if both operands are equal; otherwise, <b>true</b>.</returns>
        [ExcludeFromCodeCoverage]
        public static bool operator !=(InternalUserID first, InternalUserID second)
	    {
	        return !(first == second);
	    }

		/// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
		[ExcludeFromCodeCoverage]
	    public override string ToString()
	    {
	        if ((object) _value == null)
	            return null;
	        return _value.ToString();
	    }
				
		[ExcludeFromCodeCoverage]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("v", _value);
		}
		
		[ExcludeFromCodeCoverage]
		XmlSchema IXmlSerializable.GetSchema()
	    {
	        return null;
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.ReadXml(XmlReader reader)
	    {
	        _value = (System.Int32)reader.ReadElementContentAs(typeof(System.Int32), null);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(_value));
	    }
	}

	/// <summary>
	/// Implements the strong type <see cref="DiscordUserID" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "1.0.0")]
	[Serializable]
	public partial struct DiscordUserID : IEquatable<DiscordUserID>, IComparable<DiscordUserID>, ISerializable, IXmlSerializable
	{
		/// <summary>
		/// Actual backing field which holds the value.
		/// </summary>
		/// <remarks>This field is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
		private System.UInt64 _value;

		[ExcludeFromCodeCoverage]
		private DiscordUserID(System.UInt64 value)
	    {
	        _value = value;
	    }

		[ExcludeFromCodeCoverage]
		private DiscordUserID(SerializationInfo info, StreamingContext context)
		{
            _value = (System.UInt64)info.GetValue("v", typeof(System.UInt64));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="DiscordUserID"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator DiscordUserID(System.UInt64 value)
	    {
	        return new DiscordUserID(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.UInt64"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.UInt64(DiscordUserID value)
	    {
	        return value._value;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="DiscordUserID"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(DiscordUserID other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _value == other._value;
		}
		
		/// <summary>
        /// Returns a value indicating whether this instance and a specified object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public override bool Equals(object other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals((DiscordUserID)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="DiscordUserID"/>.</returns>
		[ExcludeFromCodeCoverage]
		public override int GetHashCode()
		{
			if (Equals(null, _value))
				return 0;
			return _value.GetHashCode();
		}

		/// <summary>
        /// Compares this instance to a specified <see cref="DiscordUserID"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="DiscordUserID"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(DiscordUserID other)
		{
			if (Equals(null, _value))
				return 1;
			return _value.CompareTo(other._value);
		}

        /// <summary>
        /// Checks if both operands are equal.
        /// </summary>
        /// <param name="first">The first operand.</param>
        /// <param name="second">The second operand.</param>
        /// <returns><b>True</b>, if both operands are equal; otherwise, <b>false</b>.</returns>
        [ExcludeFromCodeCoverage]
        public static bool operator ==(DiscordUserID first, DiscordUserID second)
	    {
	        if (ReferenceEquals(first, second))
	            return true;

	        return first._value == second._value;
	    }

        /// <summary>
        /// Checks if both operands are not equal.
        /// </summary>
        /// <param name="first">The first operand.</param>
        /// <param name="second">The second operand.</param>
        /// <returns><b>False</b>, if both operands are equal; otherwise, <b>true</b>.</returns>
        [ExcludeFromCodeCoverage]
        public static bool operator !=(DiscordUserID first, DiscordUserID second)
	    {
	        return !(first == second);
	    }

		/// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
		[ExcludeFromCodeCoverage]
	    public override string ToString()
	    {
	        if ((object) _value == null)
	            return null;
	        return _value.ToString();
	    }
				
		[ExcludeFromCodeCoverage]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("v", _value);
		}
		
		[ExcludeFromCodeCoverage]
		XmlSchema IXmlSerializable.GetSchema()
	    {
	        return null;
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.ReadXml(XmlReader reader)
	    {
	        _value = (System.UInt64)reader.ReadElementContentAs(typeof(System.UInt64), null);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(_value));
	    }
	}
}