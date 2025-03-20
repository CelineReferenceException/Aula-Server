namespace Aula.Server.Common.Validation;

internal interface ICustomValidator<in TObject>
{
	internal CustomValidationResult Validate(TObject obj);
}
