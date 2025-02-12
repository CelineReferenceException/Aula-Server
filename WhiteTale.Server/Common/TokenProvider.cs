using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using Encoding = System.Text.Encoding;

namespace WhiteTale.Server.Common.Identity;

internal sealed class TokenProvider
{
	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used through dependency injection")]
	internal String CreateToken(String id, String stamp)
	{
		const Char separator = '.';

		// Calculate the max count of bytes to use when encoding so we can reduce allocations by storing all into a single buffer.
		var idUtf8Length = Encoding.UTF8.GetMaxByteCount(id.Length);
		var stampUtf8Length = Encoding.UTF8.GetMaxByteCount(stamp.Length);
		var idBase64Length = Base64.GetMaxEncodedToUtf8Length(idUtf8Length);
		var stampBase64Length = Base64.GetMaxEncodedToUtf8Length(stampUtf8Length);

		var buffer = ArrayPool<Byte>.Shared.Rent(idUtf8Length + stampUtf8Length + idBase64Length + stampBase64Length);
		var idUtf8 = buffer.AsSpan(0, idUtf8Length);
		var stampUtf8 = buffer.AsSpan(idUtf8Length, stampUtf8Length);
		var idBase64 = buffer.AsSpan(idUtf8Length + stampUtf8Length, idBase64Length);
		var stampBase64 = buffer.AsSpan(idUtf8Length + stampUtf8Length + idBase64Length, stampBase64Length);

		var idUtf8BytesWritten = Encoding.UTF8.GetBytes(id, idUtf8);
		var stampUtf8BytesWritten = Encoding.UTF8.GetBytes(stamp, stampUtf8);
		idUtf8 = idUtf8[..idUtf8BytesWritten];
		stampUtf8 = stampUtf8[..stampUtf8BytesWritten];

		_ = Base64.EncodeToUtf8(idUtf8, idBase64, out _, out var idBase64BytesWritten);
		_ = Base64.EncodeToUtf8(stampUtf8, stampBase64, out _, out var stampBase64BytesWritten);
		idBase64 = idBase64[..idBase64BytesWritten];
		stampBase64 = stampBase64[..stampBase64BytesWritten];

		return String.Create(idBase64.Length + 1 + stampBase64.Length,
			new Base64TokenSegments
			{
				Id = idBase64,
				SecurityStamp = stampBase64,
			}, (span, segments) =>
			{
				var position = 0;

				for (var i = 0; i < segments.Id.Length; i++, position++)
				{
					span[position] = (Char)segments.Id[i];
				}

				span[position++] = separator;

				for (var i = 0; i < segments.SecurityStamp.Length; i++, position++)
				{
					span[position] = (Char)segments.SecurityStamp[i];
				}

				ArrayPool<Byte>.Shared.Return(buffer);
			});
	}

	internal String CreateToken(User user)
	{
		var id = user.Id.ToString();
		var stamp = user.SecurityStamp ?? throw new ArgumentException("The user security stamp cannot be null.");
		return CreateToken(id, stamp);
	}

	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used through dependency injection")]
	internal Boolean TryReadFromToken(
		ReadOnlySpan<Char> token,
		[NotNullWhen(true)] out String? id,
		[NotNullWhen(true)] out String? stamp)
	{
		var tokenSegments = token.Split('.');

		if (!tokenSegments.MoveNext())
		{
			id = null;
			stamp = null;
			return false;
		}

		var userIdSegmentStart = tokenSegments.Current.Start.Value;
		var userIdSegmentLength = tokenSegments.Current.End.Value - userIdSegmentStart;
		var userIdBase64 = token.Slice(userIdSegmentStart, userIdSegmentLength);

		if (!tokenSegments.MoveNext())
		{
			id = null;
			stamp = null;
			return false;
		}

		var stampSegmentStart = tokenSegments.Current.Start.Value;
		var stampSegmentLength = tokenSegments.Current.End.Value - stampSegmentStart;
		var stampBase64 = token.Slice(stampSegmentStart, stampSegmentLength);

		if (!Base64.IsValid(userIdBase64) ||
		    !Base64.IsValid(stampBase64))
		{
			id = null;
			stamp = null;
			return false;
		}

		// Calculate the max count of bytes to use when decoding so we can reduce allocations by storing all into a single buffer.
		var userIdUtf8Length = Base64.GetMaxDecodedFromUtf8Length(userIdBase64.Length);
		var stampUtf8Length = Base64.GetMaxDecodedFromUtf8Length(stampBase64.Length);

		var buffer = ArrayPool<Byte>.Shared.Rent(userIdUtf8Length + stampUtf8Length);
		var userIdUtf8 = buffer.AsSpan(0, userIdUtf8Length);
		var stampUtf8 = buffer.AsSpan(userIdUtf8Length, stampUtf8Length);

		if (!Convert.TryFromBase64Chars(userIdBase64, userIdUtf8, out var userIdUtf8BytesWritten))
		{
			id = null;
			stamp = null;
			return false;
		}

		id = Encoding.UTF8.GetString(userIdUtf8[..userIdUtf8BytesWritten]);

		if (!Convert.TryFromBase64Chars(stampBase64, stampUtf8, out var stampUtf8BytesWritten))
		{
			id = null;
			stamp = null;
			return false;
		}

		stamp = Encoding.UTF8.GetString(stampUtf8[..stampUtf8BytesWritten]);
		ArrayPool<Byte>.Shared.Return(buffer);
		return true;
	}

	private readonly ref struct Base64TokenSegments
	{
		internal required ReadOnlySpan<Byte> Id { get; init; }
		internal required ReadOnlySpan<Byte> SecurityStamp { get; init; }
	}
}
