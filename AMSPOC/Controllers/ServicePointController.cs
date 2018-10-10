using AMSPOC.DatabaseOps;
using AMSPOC.Models;
using AMSPOC.Utility;
using Microsoft.AspNetCore.Mvc;

namespace AMSPOC.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ServicePointController : ControllerBase
    {
        //MicroService 4
        public IActionResult ReadMQ2OPLDNCreateSPNPushTOMQ3()
        {
            try
            {
                //Read from MQ
                OPLD opldObject = CommonUtility<OPLD>.PullFromActiveMQ(2);

                ServicePoint servicePointObject = new ServicePoint();

                //CreateServicepoint          
                //Check if opld tracking number matches with dials matching number
                SakilaContext context = HttpContext.RequestServices.GetService(typeof(SakilaContext)) as SakilaContext;
                DIALS dialsObject = context.GetMatchingDialsID(opldObject.TrackingNumber);
                if (dialsObject != null)
                {
                    servicePointObject = ServicePointUtility.CreateServicePoint(opldObject, dialsObject.ConsigneeName, dialsObject.ClarifiedSignature, true);
                }
                else
                {
                    servicePointObject = ServicePointUtility.CreateServicePoint(opldObject, "", "", false);
                }

                //Push OPLD in to Active MQ2
                CommonUtility<ServicePoint>.PushToActiveMQ(servicePointObject, 3);
            }
            catch
            {
                return new JsonResult(new { Result = System.Net.HttpStatusCode.InternalServerError });
            }

            return Ok();
        }

        //MicroService 5
        public IActionResult ReadMQ3ServicePointNWriteToDB()
        {
            try
            {
                //Read from MQ
                ServicePoint servicePointObject = CommonUtility<ServicePoint>.PullFromActiveMQ(3);

                //Write Servicepoint to DB
                SakilaContext context = HttpContext.RequestServices.GetService(typeof(SakilaContext)) as SakilaContext;
                context.AddNewServicePoint(servicePointObject);
            }
            catch
            {
                return new JsonResult(new { Result = System.Net.HttpStatusCode.InternalServerError });
            }


            return Ok();
        }
    }
}