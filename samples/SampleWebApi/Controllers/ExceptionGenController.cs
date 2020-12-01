using System;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers
{
    [ApiController]
    public class ExceptionGenController : ControllerBase
    {
        private readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random());

        private readonly Type[] _exceptionTypes = new[]
        {
            typeof(InvalidCastException),
            typeof(ArgumentOutOfRangeException),
            typeof(InvalidOperationException),
            typeof(ArgumentNullException),
            typeof(AccessViolationException),
            typeof(ApplicationException),
        };

        [HttpGet("api/exception/throw")]
        public IActionResult Throw()
        {
            var randomExceptionType = _exceptionTypes[_random.Value.Next(_exceptionTypes.Length)];
            var exception = Activator.CreateInstance(randomExceptionType) as Exception;

            throw exception;
        }
    }
}
