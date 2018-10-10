using Microsoft.AspNetCore.Mvc;
using System.IO;
using AMSPOC.Models;
using AMSPOC.Utility;
using AMSPOC.DatabaseOps;


namespace AMSPOC.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProcessOPLDController : ControllerBase
    {
        //MicroService 1
        [HttpPost]
        public IActionResult ProcessOPLDNPushTOMQ1()
        {
            try
            {
                string opldFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "OPLDFiles");

                if (Directory.Exists(opldFolderPath))
                {
                    var files = Directory.GetFiles(opldFolderPath);

                    if (files.Length > 0)
                    {
                        foreach (string fileName in files)
                        {
                            string opldString = System.IO.File.ReadAllText(Path.Combine(opldFolderPath, fileName));

                            //Process OPLD data
                            var opldObject = OPLDUtility.ProcessOPLD(opldString);

                            //Push OPLD in to Active MQ1
                            if (!string.IsNullOrEmpty(opldObject.TrackingNumber))
                            {
                                CommonUtility<OPLD>.PushToActiveMQ(opldObject, 1);
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

        //MicroService 2
        public IActionResult ProcessMQ1OPLDMessageNWriteToDBNMQ2()
        {
            try
            {
                //Read from MQ
                OPLD opldObject = CommonUtility<OPLD>.PullFromActiveMQ(1);

                //Store in to DB
                SakilaContext context = HttpContext.RequestServices.GetService(typeof(SakilaContext)) as SakilaContext;
                context.AddNewOPLD(opldObject);

                //Push OPLD in to Active MQ2
                CommonUtility<OPLD>.PushToActiveMQ(opldObject, 2);
            }
            catch
            {
                return new JsonResult(new { Result = System.Net.HttpStatusCode.InternalServerError });
            }
            

            return Ok();
        }
    }
}