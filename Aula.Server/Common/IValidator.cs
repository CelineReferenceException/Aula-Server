namespace Aula.Server.Common;

internal interface IValidator<in TObject, TProblem>
	where TProblem : class?
{
	Result Validate(TObject obj);
}
