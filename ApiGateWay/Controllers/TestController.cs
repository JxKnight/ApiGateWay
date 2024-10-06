using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiGateWay.Model;
using ApiGateWay.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiGateWay.Controllers
{
    [Route("Test")]
    public class TestController : Controller
    {
        [HttpPost("1")]
        public IActionResult Index1(string apiId)
        {
            Database database = new Database();
            Dictionary<string, object> dic = new Dictionary<string, object>()
            {
                {"Session","Get" },{"ApiID",apiId}
            };
            using(DataTable data = database.selectSP("uspApi", dic))
            {
                if (data != null)
                {
                    DataRow row = data.Rows[0];
                    return Ok(new { APIName = row["ApiIDName"].ToString(),APIURL = row["ApiIDURL"].ToString(),IdentityCheck = row["IdentityCheck"].ToString() });
                }
            }
            return Ok("Not Found");
        }
        [HttpPost("2")]
        public async Task<IActionResult> Index2(string projectId,string apiId)
        {
            Cryptography cryptography = new Cryptography("ACCESS", null);
            string EncryptedProjectId = cryptography.EncryptString(projectId);
            AppSetting appSetting = new AppSetting();
            using (HttpClient client = new HttpClient())
            {
                // The URL to which the POST request will be sent
                string url = appSetting.Get("TestURL")+"?Id="+ System.Net.WebUtility.UrlEncode(EncryptedProjectId);
                DateTime currentDateTime = DateTime.Now;
                string RequestId = currentDateTime.ToString("yyyyddMM")+apiId;
                Keys keys = new Keys(projectId);
                string PrimaryKey = keys.Key1.Substring(0, 8)+RequestId+keys.Key1.Substring(keys.Key1.Length-8);
                cryptography = new Cryptography(projectId, keys);
                //Request.Headers.Add("BT-Primary-Key", cryptography.EncryptString(PrimaryKey));
                string Signature = currentDateTime.ToString("yyyyddMM") + keys.Key1.Substring(0, 8) + RequestId + keys.Key1.Substring(keys.Key1.Length - 8) + currentDateTime.ToString("yyyyddMM");
                //Response.Headers.Add("BT-Signature", cryptography.ComputeSha256Hash(Signature));
                return Ok(new { url = url, BTPrimaryKey = cryptography.EncryptString(PrimaryKey), BTSignature = cryptography.ComputeSha256Hash(Signature) });
            }

        }
    }
}

