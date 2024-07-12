using Demo.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Demo.Utilities.ResponseHelper
{
    public static class ResponseHelper
    {
        public static IActionResult GetResponse(ResponseDto response)
        {
            return (HttpStatusCode)response.StatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(response),
                HttpStatusCode.BadRequest => new BadRequestObjectResult(response),
                HttpStatusCode.NotFound => new NotFoundObjectResult(response),
                HttpStatusCode.InternalServerError => new ObjectResult(response)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                },
                _ => new StatusCodeResult(response.StatusCode),
            };
        }

        public static IActionResult GetResponse<T>(ResponseDto<T> response)
        {
            return (HttpStatusCode)response.StatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(response),
                HttpStatusCode.BadRequest => new BadRequestObjectResult(response),
                HttpStatusCode.NotFound => new NotFoundObjectResult(response),
                HttpStatusCode.InternalServerError => new ObjectResult(response)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                },
                _ => new StatusCodeResult(response.StatusCode),
            };
        }
    }
}
