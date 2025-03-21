namespace Aula.Server.Common;

internal interface IValidator<in TObject>
{
	Result Validate(TObject obj);
}
