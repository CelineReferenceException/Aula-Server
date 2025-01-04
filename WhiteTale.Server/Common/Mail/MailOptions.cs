using System.ComponentModel.DataAnnotations;

namespace WhiteTale.Server.Common.Mail;

internal sealed class MailOptions
{
	internal const String SectionName = "Mail";
	
	[Required]
	public required String Address { get; set; }

	[Required]
	public required String Password { get; set; }

	[Required]
	public required String SmtpHost { get; set; }

	[Required]
	public required Int32 SmtpPort { get; set; }

	[Required]
	public required Boolean EnableSsl { get; set; }
}
