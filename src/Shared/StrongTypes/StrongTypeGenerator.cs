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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace HoU.GuildBot.Shared.StrongTypes
{
	/// <summary>
	/// Implements the strong type <see cref="InternalUserId" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(InternalUserId.NewtonsoftJsonConverter))]
		public partial struct InternalUserId : IEquatable<InternalUserId>, IComparable<InternalUserId>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.Int32 V { get; set; }

		[ExcludeFromCodeCoverage]
		private InternalUserId(System.Int32 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private InternalUserId(SerializationInfo info, StreamingContext context)
		{
            V = (System.Int32)(info.GetValue("v", typeof(System.Int32)) ?? throw new InvalidOperationException("Couldn't get value."));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="InternalUserId"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator InternalUserId(System.Int32 value)
	    {
	        return new InternalUserId(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.Int32"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.Int32(InternalUserId value)
	    {
	        return value.V;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="InternalUserId"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(InternalUserId other)
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
		public override bool Equals(object? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals((InternalUserId)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="InternalUserId"/>.</returns>
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
        int IComparable.CompareTo(object? obj)
        {
            if (obj is InternalUserId other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="InternalUserId"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="InternalUserId"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(InternalUserId other)
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
        public static bool operator ==(InternalUserId first, InternalUserId second)
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
        public static bool operator !=(InternalUserId first, InternalUserId second)
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
	        V = (System.Int32)reader.ReadElementContentAs(typeof(System.Int32), null!);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (InternalUserId)(value ?? throw new ArgumentNullException(nameof(value)));
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(InternalUserId);
                instance.V = (System.Int32) (Convert.ChangeType(reader.Value, typeof(System.Int32)) ?? throw new InvalidOperationException("Couldn't change type."));
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(InternalUserId);
        }
        
            }

	/// <summary>
	/// Implements the strong type <see cref="InternalGameId" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(InternalGameId.NewtonsoftJsonConverter))]
		public partial struct InternalGameId : IEquatable<InternalGameId>, IComparable<InternalGameId>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.Int16 V { get; set; }

		[ExcludeFromCodeCoverage]
		private InternalGameId(System.Int16 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private InternalGameId(SerializationInfo info, StreamingContext context)
		{
            V = (System.Int16)(info.GetValue("v", typeof(System.Int16)) ?? throw new InvalidOperationException("Couldn't get value."));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="InternalGameId"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator InternalGameId(System.Int16 value)
	    {
	        return new InternalGameId(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.Int16"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.Int16(InternalGameId value)
	    {
	        return value.V;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="InternalGameId"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(InternalGameId other)
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
		public override bool Equals(object? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals((InternalGameId)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="InternalGameId"/>.</returns>
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
        int IComparable.CompareTo(object? obj)
        {
            if (obj is InternalGameId other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="InternalGameId"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="InternalGameId"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(InternalGameId other)
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
        public static bool operator ==(InternalGameId first, InternalGameId second)
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
        public static bool operator !=(InternalGameId first, InternalGameId second)
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
	        V = (System.Int16)reader.ReadElementContentAs(typeof(System.Int16), null!);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (InternalGameId)(value ?? throw new ArgumentNullException(nameof(value)));
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(InternalGameId);
                instance.V = (System.Int16) (Convert.ChangeType(reader.Value, typeof(System.Int16)) ?? throw new InvalidOperationException("Couldn't change type."));
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(InternalGameId);
        }
        
            }

	/// <summary>
	/// Implements the strong type <see cref="InternalGameRoleId" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(InternalGameRoleId.NewtonsoftJsonConverter))]
		public partial struct InternalGameRoleId : IEquatable<InternalGameRoleId>, IComparable<InternalGameRoleId>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.Int16 V { get; set; }

		[ExcludeFromCodeCoverage]
		private InternalGameRoleId(System.Int16 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private InternalGameRoleId(SerializationInfo info, StreamingContext context)
		{
            V = (System.Int16)(info.GetValue("v", typeof(System.Int16)) ?? throw new InvalidOperationException("Couldn't get value."));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="InternalGameRoleId"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator InternalGameRoleId(System.Int16 value)
	    {
	        return new InternalGameRoleId(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.Int16"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.Int16(InternalGameRoleId value)
	    {
	        return value.V;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="InternalGameRoleId"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(InternalGameRoleId other)
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
		public override bool Equals(object? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals((InternalGameRoleId)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="InternalGameRoleId"/>.</returns>
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
        int IComparable.CompareTo(object? obj)
        {
            if (obj is InternalGameRoleId other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="InternalGameRoleId"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="InternalGameRoleId"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(InternalGameRoleId other)
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
        public static bool operator ==(InternalGameRoleId first, InternalGameRoleId second)
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
        public static bool operator !=(InternalGameRoleId first, InternalGameRoleId second)
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
	        V = (System.Int16)reader.ReadElementContentAs(typeof(System.Int16), null!);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (InternalGameRoleId)(value ?? throw new ArgumentNullException(nameof(value)));
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(InternalGameRoleId);
                instance.V = (System.Int16) (Convert.ChangeType(reader.Value, typeof(System.Int16)) ?? throw new InvalidOperationException("Couldn't change type."));
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(InternalGameRoleId);
        }
        
            }

	/// <summary>
	/// Implements the strong type <see cref="DiscordUserId" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(DiscordUserId.NewtonsoftJsonConverter))]
		public partial struct DiscordUserId : IEquatable<DiscordUserId>, IComparable<DiscordUserId>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.UInt64 V { get; set; }

		[ExcludeFromCodeCoverage]
		private DiscordUserId(System.UInt64 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private DiscordUserId(SerializationInfo info, StreamingContext context)
		{
            V = (System.UInt64)(info.GetValue("v", typeof(System.UInt64)) ?? throw new InvalidOperationException("Couldn't get value."));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="DiscordUserId"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator DiscordUserId(System.UInt64 value)
	    {
	        return new DiscordUserId(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.UInt64"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.UInt64(DiscordUserId value)
	    {
	        return value.V;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="DiscordUserId"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(DiscordUserId other)
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
		public override bool Equals(object? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals((DiscordUserId)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="DiscordUserId"/>.</returns>
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
        int IComparable.CompareTo(object? obj)
        {
            if (obj is DiscordUserId other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="DiscordUserId"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="DiscordUserId"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(DiscordUserId other)
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
        public static bool operator ==(DiscordUserId first, DiscordUserId second)
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
        public static bool operator !=(DiscordUserId first, DiscordUserId second)
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
	        V = (System.UInt64)reader.ReadElementContentAs(typeof(System.UInt64), null!);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (DiscordUserId)(value ?? throw new ArgumentNullException(nameof(value)));
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(DiscordUserId);
                instance.V = reader.Value == null ? default : reader.Value is System.Numerics.BigInteger bi ? (System.UInt64)bi : reader.Value is System.Int64 @long ? (System.UInt64)@long : System.UInt64.TryParse(reader.Value.ToString(), out var parsedUInt64) ? parsedUInt64 : default;
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(DiscordUserId);
        }
        
            }

	/// <summary>
	/// Implements the strong type <see cref="DiscordChannelId" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(DiscordChannelId.NewtonsoftJsonConverter))]
		public partial struct DiscordChannelId : IEquatable<DiscordChannelId>, IComparable<DiscordChannelId>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.UInt64 V { get; set; }

		[ExcludeFromCodeCoverage]
		private DiscordChannelId(System.UInt64 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private DiscordChannelId(SerializationInfo info, StreamingContext context)
		{
            V = (System.UInt64)(info.GetValue("v", typeof(System.UInt64)) ?? throw new InvalidOperationException("Couldn't get value."));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="DiscordChannelId"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator DiscordChannelId(System.UInt64 value)
	    {
	        return new DiscordChannelId(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.UInt64"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.UInt64(DiscordChannelId value)
	    {
	        return value.V;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="DiscordChannelId"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(DiscordChannelId other)
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
		public override bool Equals(object? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals((DiscordChannelId)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="DiscordChannelId"/>.</returns>
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
        int IComparable.CompareTo(object? obj)
        {
            if (obj is DiscordChannelId other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="DiscordChannelId"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="DiscordChannelId"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(DiscordChannelId other)
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
        public static bool operator ==(DiscordChannelId first, DiscordChannelId second)
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
        public static bool operator !=(DiscordChannelId first, DiscordChannelId second)
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
	        V = (System.UInt64)reader.ReadElementContentAs(typeof(System.UInt64), null!);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (DiscordChannelId)(value ?? throw new ArgumentNullException(nameof(value)));
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(DiscordChannelId);
                instance.V = reader.Value == null ? default : reader.Value is System.Numerics.BigInteger bi ? (System.UInt64)bi : reader.Value is System.Int64 @long ? (System.UInt64)@long : System.UInt64.TryParse(reader.Value.ToString(), out var parsedUInt64) ? parsedUInt64 : default;
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(DiscordChannelId);
        }
        
            }

	/// <summary>
	/// Implements the strong type <see cref="DiscordRoleId" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(DiscordRoleId.NewtonsoftJsonConverter))]
		public partial struct DiscordRoleId : IEquatable<DiscordRoleId>, IComparable<DiscordRoleId>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.UInt64 V { get; set; }

		[ExcludeFromCodeCoverage]
		private DiscordRoleId(System.UInt64 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private DiscordRoleId(SerializationInfo info, StreamingContext context)
		{
            V = (System.UInt64)(info.GetValue("v", typeof(System.UInt64)) ?? throw new InvalidOperationException("Couldn't get value."));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="DiscordRoleId"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator DiscordRoleId(System.UInt64 value)
	    {
	        return new DiscordRoleId(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.UInt64"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.UInt64(DiscordRoleId value)
	    {
	        return value.V;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="DiscordRoleId"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(DiscordRoleId other)
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
		public override bool Equals(object? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals((DiscordRoleId)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="DiscordRoleId"/>.</returns>
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
        int IComparable.CompareTo(object? obj)
        {
            if (obj is DiscordRoleId other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="DiscordRoleId"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="DiscordRoleId"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(DiscordRoleId other)
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
        public static bool operator ==(DiscordRoleId first, DiscordRoleId second)
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
        public static bool operator !=(DiscordRoleId first, DiscordRoleId second)
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
	        V = (System.UInt64)reader.ReadElementContentAs(typeof(System.UInt64), null!);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (DiscordRoleId)(value ?? throw new ArgumentNullException(nameof(value)));
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(DiscordRoleId);
                instance.V = reader.Value == null ? default : reader.Value is System.Numerics.BigInteger bi ? (System.UInt64)bi : reader.Value is System.Int64 @long ? (System.UInt64)@long : System.UInt64.TryParse(reader.Value.ToString(), out var parsedUInt64) ? parsedUInt64 : default;
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(DiscordRoleId);
        }
        
            }

	/// <summary>
	/// Implements the strong type <see cref="DiscordMessageId" />.
	/// </summary>
	[GeneratedCode("Herdo.StrongTypes.StrongTypeGenerator", "2.1.0")]
	[Serializable]
	[Newtonsoft.Json.JsonConverter(typeof(DiscordMessageId.NewtonsoftJsonConverter))]
		public partial struct DiscordMessageId : IEquatable<DiscordMessageId>, IComparable<DiscordMessageId>, IComparable, ISerializable, IXmlSerializable
	{
        /// <summary>
        /// Actual backing property which holds the value.
        /// </summary>
        /// <remarks>This property is basically readonly, but must be non-readonly due to the XML-deserialization which will be called from outside the constructor.</remarks>
        [UsedImplicitly]
		public System.UInt64 V { get; set; }

		[ExcludeFromCodeCoverage]
		private DiscordMessageId(System.UInt64 value)
	    {
	        V = value;
	    }

		[ExcludeFromCodeCoverage]
		private DiscordMessageId(SerializationInfo info, StreamingContext context)
		{
            V = (System.UInt64)(info.GetValue("v", typeof(System.UInt64)) ?? throw new InvalidOperationException("Couldn't get value."));
		}

	    /// <summary>
	    /// Converts the weak type into a <see cref="DiscordMessageId"/> instance.
	    /// </summary>
	    /// <param name="value">The value to convert.</param>
	    /// <returns>A new instance of the strong type.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator DiscordMessageId(System.UInt64 value)
	    {
	        return new DiscordMessageId(value);
	    }

	    /// <summary>
	    /// Converts the strong type into a <see cref="System.UInt64"/> value.
	    /// </summary>
	    /// <param name="value">The instance to convert.</param>
	    /// <returns>The converted value.</returns>
		[ExcludeFromCodeCoverage]
	    public static explicit operator System.UInt64(DiscordMessageId value)
	    {
	        return value.V;
	    }

		/// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="DiscordMessageId"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><b>true</b> if <paramref name="other"/> is equal to this instance; otherwise, <b>false</b>.</returns>
		[ExcludeFromCodeCoverage]
		public bool Equals(DiscordMessageId other)
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
		public override bool Equals(object? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals((DiscordMessageId)other);
		}

		/// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="DiscordMessageId"/>.</returns>
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
        int IComparable.CompareTo(object? obj)
        {
            if (obj is DiscordMessageId other)
                return CompareTo(other);
            throw new ArgumentException($"{nameof(obj)} is not of the same type as this instance.", nameof(obj));
        }

		/// <summary>
        /// Compares this instance to a specified <see cref="DiscordMessageId"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="DiscordMessageId"/> to compare to.</param>
        /// <returns>A signed integer that indicates the relative order of this instance and <paramref name="other"/>.</returns>
		[ExcludeFromCodeCoverage]
		public int CompareTo(DiscordMessageId other)
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
        public static bool operator ==(DiscordMessageId first, DiscordMessageId second)
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
        public static bool operator !=(DiscordMessageId first, DiscordMessageId second)
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
	        V = (System.UInt64)reader.ReadElementContentAs(typeof(System.UInt64), null!);
	    }
		
		[ExcludeFromCodeCoverage]
	    void IXmlSerializable.WriteXml(XmlWriter writer)
	    {
            writer.WriteString(XmlConvert.ToString(V));
	    }

		public sealed class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
        {
		    [ExcludeFromCodeCoverage]
            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
			{
				var instance = (DiscordMessageId)(value ?? throw new ArgumentNullException(nameof(value)));
				writer.WriteValue(instance.V);
			}
			
		    [ExcludeFromCodeCoverage]
            public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
			    if (reader.Value == null && Nullable.GetUnderlyingType(objectType) != null)
				    return null;

                var instance = default(DiscordMessageId);
                instance.V = reader.Value == null ? default : reader.Value is System.Numerics.BigInteger bi ? (System.UInt64)bi : reader.Value is System.Int64 @long ? (System.UInt64)@long : System.UInt64.TryParse(reader.Value.ToString(), out var parsedUInt64) ? parsedUInt64 : default;
                return instance;
			}
			
		    [ExcludeFromCodeCoverage]
            public override bool CanConvert(Type objectType) => objectType == typeof(DiscordMessageId);
        }
        
            }
}