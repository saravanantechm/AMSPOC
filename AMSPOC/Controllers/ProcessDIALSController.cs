using System.IO;
using System.Threading.Tasks;
using AMSPOC.DatabaseOps;
using AMSPOC.Utility;
using Microsoft.AspNetCore.Mvc;

namespace AMSPOC.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProcessDIALSController : ControllerBase
    {
        //MicroService 3
        public async Task<IActionResult> ProcessDIALSDataNWriteToDB()
        {
            try
            {
                string dialsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "DIALSFiles");

                if (Directory.Exists(dialsFolderPath))
                {
                    var files = Directory.GetFiles(dialsFolderPath);

                    if (files.Length > 0)
                    {
                        foreach (string fileName in files)
                        {
                            FileStream fileStream = new FileStream(Path.Combine(dialsFolderPath, fileName), FileMode.Open);
                            using (BufferedStream bufferedStream = new BufferedStream(fileStream))
                            {
                                using (StreamReader streamReader = new StreamReader(bufferedStream))
                                {
                                    while (!streamReader.EndOfStream)
                                    {
                                        string dialsString = await streamReader.ReadLineAsync();

                                        //Process DIALS data
                                        var dialsObject = DIALSUtility.ProcessDIALSData(dialsString);

                                        //Store in to DB
                                        if (!string.IsNullOrEmpty(dialsObject.TrackingNumber))
                                        {
                                            SakilaContext context = HttpContext.RequestServices.GetService(typeof(SakilaContext)) as SakilaContext;
                                            context.AddNewDIALS(dialsObject);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch {
                return new JsonResult(new { Result = System.Net.HttpStatusCode.InternalServerError });
            }

            return Ok();
        }
    }
}