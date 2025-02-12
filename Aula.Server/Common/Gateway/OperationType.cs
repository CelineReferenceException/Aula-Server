namespace Aula.Server.Common.Gateway;

/// <summary>
///     The operation type of gateway payloads.
/// </summary>
internal enum OperationType
{
	/// <summary>
	///     An event dispatch.
	/// </summary>
	Dispatch = 0,

	/// <summary>
	///     Sent immediately after connecting, contains useful information for the client.
	/// </summary>
	Hello = 1,
}
