namespace Aula.Server.Common;

internal interface IValidator<in TObject, TProblem>
	where TProblem : class?
{
	Result<TProblem> Validate(TObject obj);
}
