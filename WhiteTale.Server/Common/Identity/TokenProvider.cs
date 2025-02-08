using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using Encoding = System.Text.Encoding;

namespace WhiteTale.Server.Common.Identity;


internal sealed class TokenProvider
{
	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used through dependency injection")]
	internal String CreateToken(User user)
	{
		const Char separator = '.';

		var id = user.Id.ToString();
		var securityStamp = user.SecurityStamp;

		// Calculate the max count of bytes to use when encoding so we can reduce allocations by storing all into a single buffer.
		var idUtf8Length = Encoding.UTF8.GetMaxByteCount(id.Length);
		var securityStampUtf8Length = Encoding.UTF8.GetMaxByteCount(securityStamp.Length);
		var idBase64Length = Base64.GetMaxEncodedToUtf8Length(idUtf8Length);
		var securityStampBase64Length = Base64.GetMaxEncodedToUtf8Length(securityStampUtf8Length);

		var buffer = new Byte[idUtf8Length + securityStampUtf8Length + idBase64Length + securityStampBase64Length];
		var idUtf8 = buffer.AsSpan(0, idUtf8Length);
		var securityStampUtf8 = buffer.AsSpan(idUtf8Length, securityStampUtf8Length);
		var idBase64 = buffer.AsSpan(idUtf8Length + securityStampUtf8Length, idBase64Length);
		var securityStampBase64 = buffer.AsSpan(idUtf8Length + securityStampUtf8Length + idBase64Length, securityStampBase64Length);

		var idUtf8BytesWritten = Encoding.UTF8.GetBytes(id, idUtf8);
		var securityStampUtf8BytesWritten = Encoding.UTF8.GetBytes(securityStamp, securityStampUtf8);
		idUtf8 = idUtf8[..idUtf8BytesWritten];
		securityStampUtf8 = securityStampUtf8[..securityStampUtf8BytesWritten];

		_ = Base64.EncodeToUtf8(idUtf8, idBase64, out _, out var idBase64BytesWritten);
		_ = Base64.EncodeToUtf8(securityStampUtf8, securityStampBase64, out _, out var securityStampBase64BytesWritten);
		idBase64 = idBase64[..idBase64BytesWritten];
		securityStampBase64 = securityStampBase64[..securityStampBase64BytesWritten];

		return String.Create(idBase64.Length + 1 + securityStampBase64.Length,
			new Base64TokenSegments
			{
				Id = idBase64,
				SecurityStamp = securityStampBase64,
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
			});
	}

	[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Used through dependency injection")]
	internal Boolean TryReadFromToken(
		ReadOnlySpan<Char> token,
		[NotNullWhen(true)] out UInt64? userId,
		[NotNullWhen(true)] out String? securityStamp)
	{
		var tokenSegments = token.Split('.');

		if (!tokenSegments.MoveNext())
		{
			userId = null;
			securityStamp = null;
			return false;
		}

		var userIdSegmentStart = tokenSegments.Current.Start.Value;
		var userIdSegmentLength = userIdSegmentStart - tokenSegments.Current.End.Value;
		var userIdBase64 = token.Slice(userIdSegmentStart, userIdSegmentLength);

		if (!tokenSegments.MoveNext())
		{
			userId = null;
			securityStamp = null;
			return false;
		}

		var securityStampSegmentStart = tokenSegments.Current.Start.Value;
		var securityStampSegmentLength = securityStampSegmentStart - tokenSegments.Current.End.Value;
		var securityStampBase64 = token.Slice(securityStampSegmentStart, securityStampSegmentLength);

		if (!Base64.IsValid(userIdBase64) ||
		    !Base64.IsValid(securityStampBase64))
		{
			userId = null;
			securityStamp = null;
			return false;
		}

		// Calculate the max count of bytes to use when decoding so we can reduce allocations by storing all into a single buffer.
		var userIdUtf8Length = Base64.GetMaxDecodedFromUtf8Length(userIdBase64.Length);
		var securityStampUtf8Length = Base64.GetMaxDecodedFromUtf8Length(securityStampBase64.Length);

		var buffer = new Byte[userIdUtf8Length + securityStampUtf8Length];
		var userIdUtf8 = buffer.AsSpan(0, userIdUtf8Length);
		var securityStampUtf8 = buffer.AsSpan(userIdUtf8Length, securityStampUtf8Length);

		if (!Convert.TryFromBase64Chars(userIdBase64, userIdUtf8, out var userIdUtf8BytesWritten) ||
		    !UInt64.TryParse(userIdUtf8[..userIdUtf8BytesWritten], out var userIdValue))
		{
			userId = null;
			securityStamp = null;
			return false;
		}

		userId = userIdValue;

		if (!Convert.TryFromBase64Chars(securityStampBase64, securityStampUtf8, out var securityStampUtf8BytesWritten))
		{
			userId = null;
			securityStamp = null;
			return false;
		}

		securityStamp = Encoding.UTF8.GetString(securityStampUtf8[..securityStampUtf8BytesWritten]);
		return true;
	}

	private readonly ref struct Base64TokenSegments
	{
		internal required ReadOnlySpan<Byte> Id { get; init; }
		internal required ReadOnlySpan<Byte> SecurityStamp { get; init; }
	}
}
