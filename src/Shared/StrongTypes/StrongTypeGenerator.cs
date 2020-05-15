// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable InheritdocConsiderUsage
// ReSharper disable RedundantNameQualifier
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ReferenceEqualsWithValueType
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable RedundantCast
// ReSharper disable RedundantCast.0
// ReSharper disable SpecifyACultureInStringConversionExplicitly
// ReSharper disable ArrangeThisQualifier
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable RedundantToStringCall

using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace HoU.GuildBot.Shared.StrongTypes
{
	/// <summary>
	/// Implements the strong type <see cref="InternalUserID" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(InternalUserID.NewtonsoftJsonConverter))]
		public partial struct InternalUserID : IEquatable<InternalUserID>, IComparable<InternalUserID>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.Int32 V { get; set; }

		[ExcludeFromCodeCoverage]
		private InternalUserID(System.Int32 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private InternalUserID(SerializationInfo info, StreamingContext context)
		{
            V = (System.Int32)info.GetValue("v", typeof(System.Int32));
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
	        return value.V;
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
			return this.V == other.V;
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
			if (Equals(null, V))
				return 0;
			return V.GetHashCode();
		}
        
        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        /// an integer that indicates whether the current instance precedes, follows, or
        /// occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <exception cref="System.ArgumentException"><paramref name="obj"/> is not the same type as this instance.</exception>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The
        /// return value has these meanings: Value Meaning Less than zero This instance precedes
        /// obj in the sort order. Zero This instance occurs in the same position in the
        /// sort order as obj. Greater than zero This instance follows obj in the sort order.
        /// </returns>
        [ExcludeFromCodeCoverage]
        int IComparable.CompareTo(object obj)
        {
            if (obj is InternalUserID other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="InternalUserID"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="InternalUserID"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(InternalUserID other)
		{
			if (Equals(null, V))
				return 1;
			return V.CompareTo(other.V);
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

			return first.V == second.V;
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
	        if ((object)V == null)
#pragma warning disable CS8603 // Possible null reference return.
	            return null;
#pragma warning restore CS8603 // Possible null reference return.
	        return V.ToString();
	    }
				
		[ExcludeFromCodeCoverage]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("v", V);
		}
		
		[ExcludeFromCodeCoverage]
		XmlSchema IXmlSerializable.GetSchema()
	    {
#pragma warning disable CS8603 // Possible null reference return.
	        return null;
#pragma warning restore CS8603 // Possible null reference return.
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.ReadXml(XmlReader reader)
	    {
	        V = (System.Int32)reader.ReadElementContentAs(typeof(System.Int32), null);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (InternalUserID)value;
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(InternalUserID);
                instance.V = (System.Int32) Convert.ChangeType(reader.Value, typeof(System.Int32));
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(InternalUserID);
        }
        
            }

	/// <summary>
	/// Implements the strong type <see cref="DiscordUserID" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(DiscordUserID.NewtonsoftJsonConverter))]
		public partial struct DiscordUserID : IEquatable<DiscordUserID>, IComparable<DiscordUserID>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.UInt64 V { get; set; }

		[ExcludeFromCodeCoverage]
		private DiscordUserID(System.UInt64 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private DiscordUserID(SerializationInfo info, StreamingContext context)
		{
            V = (System.UInt64)info.GetValue("v", typeof(System.UInt64));
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
	        return value.V;
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
			return this.V == other.V;
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
			if (Equals(null, V))
				return 0;
			return V.GetHashCode();
		}
        
        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        /// an integer that indicates whether the current instance precedes, follows, or
        /// occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <exception cref="System.ArgumentException"><paramref name="obj"/> is not the same type as this instance.</exception>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The
        /// return value has these meanings: Value Meaning Less than zero This instance precedes
        /// obj in the sort order. Zero This instance occurs in the same position in the
        /// sort order as obj. Greater than zero This instance follows obj in the sort order.
        /// </returns>
        [ExcludeFromCodeCoverage]
        int IComparable.CompareTo(object obj)
        {
            if (obj is DiscordUserID other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="DiscordUserID"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="DiscordUserID"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(DiscordUserID other)
		{
			if (Equals(null, V))
				return 1;
			return V.CompareTo(other.V);
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

			return first.V == second.V;
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
	        if ((object)V == null)
#pragma warning disable CS8603 // Possible null reference return.
	            return null;
#pragma warning restore CS8603 // Possible null reference return.
	        return V.ToString();
	    }
				
		[ExcludeFromCodeCoverage]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("v", V);
		}
		
		[ExcludeFromCodeCoverage]
		XmlSchema IXmlSerializable.GetSchema()
	    {
#pragma warning disable CS8603 // Possible null reference return.
	        return null;
#pragma warning restore CS8603 // Possible null reference return.
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.ReadXml(XmlReader reader)
	    {
	        V = (System.UInt64)reader.ReadElementContentAs(typeof(System.UInt64), null);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (DiscordUserID)value;
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(DiscordUserID);
                instance.V = reader.Value == null ? default : reader.Value is System.Numerics.BigInteger bi ? (System.UInt64)bi : reader.Value is System.Int64 @long ? (System.UInt64)@long : System.UInt64.TryParse(reader.Value.ToString(), out var parsedUInt64) ? parsedUInt64 : default;
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(DiscordUserID);
        }
        
            }

	/// <summary>
	/// Implements the strong type <see cref="DiscordChannelID" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(DiscordChannelID.NewtonsoftJsonConverter))]
		public partial struct DiscordChannelID : IEquatable<DiscordChannelID>, IComparable<DiscordChannelID>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.UInt64 V { get; set; }

		[ExcludeFromCodeCoverage]
		private DiscordChannelID(System.UInt64 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private DiscordChannelID(SerializationInfo info, StreamingContext context)
		{
            V = (System.UInt64)info.GetValue("v", typeof(System.UInt64));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="DiscordChannelID"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator DiscordChannelID(System.UInt64 value)
	    {
	        return new DiscordChannelID(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.UInt64"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.UInt64(DiscordChannelID value)
	    {
	        return value.V;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="DiscordChannelID"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(DiscordChannelID other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return this.V == other.V;
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
			return Equals((DiscordChannelID)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="DiscordChannelID"/>.</returns>
		[ExcludeFromCodeCoverage]
		public override int GetHashCode()
		{
			if (Equals(null, V))
				return 0;
			return V.GetHashCode();
		}
        
        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        /// an integer that indicates whether the current instance precedes, follows, or
        /// occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <exception cref="System.ArgumentException"><paramref name="obj"/> is not the same type as this instance.</exception>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The
        /// return value has these meanings: Value Meaning Less than zero This instance precedes
        /// obj in the sort order. Zero This instance occurs in the same position in the
        /// sort order as obj. Greater than zero This instance follows obj in the sort order.
        /// </returns>
        [ExcludeFromCodeCoverage]
        int IComparable.CompareTo(object obj)
        {
            if (obj is DiscordChannelID other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="DiscordChannelID"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="DiscordChannelID"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(DiscordChannelID other)
		{
			if (Equals(null, V))
				return 1;
			return V.CompareTo(other.V);
		}

        /// <summary>
        /// Checks if both operands are equal.
        /// </summary>
        /// <param name="first">The first operand.</param>
        /// <param name="second">The second operand.</param>
        /// <returns><b>True</b>, if both operands are equal; otherwise, <b>false</b>.</returns>
        [ExcludeFromCodeCoverage]
        public static bool operator ==(DiscordChannelID first, DiscordChannelID second)
	    {
	        if (ReferenceEquals(first, second))
	            return true;

			return first.V == second.V;
	    }

        /// <summary>
        /// Checks if both operands are not equal.
        /// </summary>
        /// <param name="first">The first operand.</param>
        /// <param name="second">The second operand.</param>
        /// <returns><b>False</b>, if both operands are equal; otherwise, <b>true</b>.</returns>
        [ExcludeFromCodeCoverage]
        public static bool operator !=(DiscordChannelID first, DiscordChannelID second)
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
	        if ((object)V == null)
#pragma warning disable CS8603 // Possible null reference return.
	            return null;
#pragma warning restore CS8603 // Possible null reference return.
	        return V.ToString();
	    }
				
		[ExcludeFromCodeCoverage]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("v", V);
		}
		
		[ExcludeFromCodeCoverage]
		XmlSchema IXmlSerializable.GetSchema()
	    {
#pragma warning disable CS8603 // Possible null reference return.
	        return null;
#pragma warning restore CS8603 // Possible null reference return.
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.ReadXml(XmlReader reader)
	    {
	        V = (System.UInt64)reader.ReadElementContentAs(typeof(System.UInt64), null);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (DiscordChannelID)value;
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(DiscordChannelID);
                instance.V = reader.Value == null ? default : reader.Value is System.Numerics.BigInteger bi ? (System.UInt64)bi : reader.Value is System.Int64 @long ? (System.UInt64)@long : System.UInt64.TryParse(reader.Value.ToString(), out var parsedUInt64) ? parsedUInt64 : default;
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(DiscordChannelID);
        }
        
            }
}