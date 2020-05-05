using System;
using System.Threading.Tasks;
using HotChocolate.Language;
using HotChocolate.Types;

namespace HotChocolate.Execution.Pipeline
{
    internal sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IErrorHandler _errorHandler;

        public ExceptionMiddleware(RequestDelegate next, IErrorHandler errorHandler)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        }

        public async Task InvokeAsync(IRequestContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (GraphQLException ex)
            {
                context.Exception = ex;
                context.Result = QueryResultBuilder.CreateError(_errorHandler.Handle(ex.Errors));
            }
            catch (SyntaxException ex)
            {
                IError error = _errorHandler.CreateUnexpectedError(ex)
                    .SetMessage(ex.Message)
                    .AddLocation(ex.Line, ex.Column)
                    .Build();

                error = _errorHandler.Handle(error);

                context.Exception = ex;
                context.Result = QueryResultBuilder.CreateError(error);
            }
            catch (ScalarSerializationException ex)
            {
                IError error = _errorHandler.CreateUnexpectedError(ex)
                    .SetMessage(ex.Message)
                    .Build();

                error = _errorHandler.Handle(error);

                context.Exception = ex;
                context.Result = QueryResultBuilder.CreateError(error);
            }
            catch (InputObjectSerializationException ex)
            {
                IError error = _errorHandler.CreateUnexpectedError(ex)
                    .SetMessage(ex.Message)
                    .Build();

                error = _errorHandler.Handle(error);

                context.Exception = ex;
                context.Result = QueryResultBuilder.CreateError(error);
            }
            catch (Exception ex)
            {
                IError error = _errorHandler.CreateUnexpectedError(ex)
                    .Build();

                error = _errorHandler.Handle(error);

                context.Exception = ex;
                context.Result = QueryResultBuilder.CreateError(error);
            }
        }
    }
}

