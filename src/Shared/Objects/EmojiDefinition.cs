namespace HoU.GuildBot.Shared.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public sealed class EmojiDefinition : IEquatable<EmojiDefinition>
    {
        private static readonly List<EmojiDefinition> Emojis;

        public static IReadOnlyList<EmojiDefinition> AllEmojis => Emojis;
        
        public Kind EmojiKind { get; }

        public string Unicode { get; }

        public string Name { get; }

        public ulong? Id { get; }

        static EmojiDefinition()
        {
            Emojis = new List<EmojiDefinition>();
        }

        public EmojiDefinition(string unicode)
        {
            Unicode = unicode ?? throw new ArgumentNullException(nameof(unicode));
            EmojiKind = Kind.UnicodeEmoji;
        }

        public EmojiDefinition(string name,
                               ulong? id)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Id = id;
            EmojiKind = id == null ? Kind.ReadonlyCustomEmote : Kind.CustomEmote;
        }

        public static void InitializeAll()
        {
            var constantsType = typeof(Constants);
            foreach (var nestedType in constantsType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
            {
                foreach (var fieldInfo in nestedType.GetFields(BindingFlags.Public | BindingFlags.Static)
                                                    .Where(m => m.FieldType == typeof(EmojiDefinition)))
                {
                    var emojiDefinition = fieldInfo.GetValue(null) as EmojiDefinition;
                    if (emojiDefinition == null)
                        continue;
                    if (Emojis.All(m => m != emojiDefinition))
                        Emojis.Add(emojiDefinition);
                }
            }
        }

        public bool Equals(EmojiDefinition other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(Unicode, other.Unicode) && string.Equals(Name, other.Name) && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is EmojiDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Unicode != null ? Unicode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(EmojiDefinition left,
                                       EmojiDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EmojiDefinition left,
                                       EmojiDefinition right)
        {
            return !Equals(left, right);
        }

        public enum Kind
        {
            UnicodeEmoji = 1,
            CustomEmote = 2,
            ReadonlyCustomEmote = 3
        }
    }
}