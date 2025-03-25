namespace Aula.Server.Core.Domain.Messages;

/// <summary>
///     Enumerates behaviors that can be associated with messages.
/// </summary>
[Flags]
internal enum MessageFlags
{
	/// <summary>
	///     The author of the message should be hidden.
	/// </summary>
	HideAuthor = 1 << 0,
}
