namespace WhiteTale.Server.Features.Gateway;

/// <summary>
///     Gateway intents define which events will be dispatched during the session.
/// </summary>
internal enum Intents
{
	/// <summary>
	///     <list type="bullet">
	///         <item>
	///             <term>
	///                 <see cref="EventType.UserUpdated" />
	///             </term>
	///         </item>
	///         <item>
	///             <term>
	///                 <see cref="EventType.UserCurrentRoomUpdated" />
	///             </term>
	///         </item>
	///     </list>
	/// </summary>
	Users = 1 << 0,

	Rooms = 1 << 1,

	Messages = 1 << 2,
}
