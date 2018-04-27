using Nancy;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YM.Json;

namespace AP.Search.Web.Nancy.Modules.Api
{
    public abstract class ApiModuleBase : NancyModule
    {
        public async Task<Response> ExecuteAsync(object request = null)
        {
            var timer = Stopwatch.StartNew();

            try
            {
                var response = await InternalExecuteAsync(request);

                timer.Stop();
                //Log(response.Log, timer.ElapsedMilliseconds);

                return response.ToResponse();
            }
            catch (Exception ex)
            {
                timer.Stop();
                
                string error = new JsonObject()
                    .Add("error", ToJson(ex))
                    .ToString(true);

                //Log(error, timer.ElapsedMilliseconds, ex);

                return new ApiResponse(error, HttpStatusCode.InternalServerError).ToResponse();

            }
            finally
            {
                if (timer.IsRunning)
                {
                    timer.Stop();
                }
            }
        }

        public string RequestBody()
        {
            if (Request == null || Request.Body == null)
            {
                return null;
            }

            using (var reader = new StreamReader(Request.Body))
            {
                return reader.ReadToEnd();
            }
        }

        protected abstract Task<ApiResponse> InternalExecuteAsync(object request);

        private static JsonObject ToJson(Exception ex)
        {
            var jo = new JsonObject().Add("message", ex.Message);

            if (ex.InnerException != null)
            {
                jo.Add("inner_error", ToJson(ex.InnerException));
            }

            return jo;
        }
    }
}
