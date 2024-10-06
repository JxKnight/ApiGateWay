using System;
using System.Data;
using System.Dynamic;
using System.Net.Http.Json;
using System.Text;
using ApiGateWay.Model;
using ApiGateWay.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiGateWay.Controllers
{
    [Route("Api")]
    public class API : Controller
    {
        //private readonly SessionManager _sessionManager;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private Database database = new Database();
        //public API(SessionManager sessionManager)
        //{
        //    _sessionManager = sessionManager;
        //}
        [HttpPost("Access")]
        public async Task<IActionResult> Access(string Id,[FromBody] object model)
        {
            ProjectModel projectModel = new ProjectModel();
            //Verify Project Id Start
            if (string.IsNullOrEmpty(Id)) return new Response().ReturnMsg(false, "Invalid Id",null, projectModel);
            Cryptography cryptography = new Cryptography("ACCESS",null);
            projectModel.Project_Id_Encrypted = Id;
            projectModel.Project_Id = cryptography.DecryptString(projectModel.Project_Id_Encrypted);
            if (string.IsNullOrEmpty(projectModel.Project_Id)) return new Response().ReturnMsg(false, "Invalid Id",null, projectModel);
            projectModel.EncryptedKeys = new Keys(projectModel.Project_Id);
            if (projectModel.EncryptedKeys == null) return new Response().ReturnMsg(false, "Project Id founded but Invalid Key Data.",null, projectModel);
            if(string.IsNullOrEmpty(projectModel.EncryptedKeys.Key1)) return new Response().ReturnMsg(false, "Project Id founded but Invalid Key 1 Data.",null, projectModel);
            if (string.IsNullOrEmpty(projectModel.EncryptedKeys.Key2)) return new Response().ReturnMsg(false, "Project Id founded but Invalid Key 2 Data.",null, projectModel);
            projectModel.Project_Key = projectModel.EncryptedKeys.Key1.Substring(4,8);
            //Verified Project Id End

            projectModel = new Project().GetProject(projectModel.Project_Id, string.Empty,projectModel);

            //Verify Header Start
            projectModel.Project_Api = new APIModel();
            cryptography = new Cryptography("Headers", projectModel.EncryptedKeys);

            //Verifying Customheader BT-Primary-Key
            projectModel.ReqHeaders = new RequestHeaders();
            projectModel.ResHeaders = new ResponseHeaders();
            if (Request.Headers.TryGetValue("BT-Primary-Key".ToLower(), out var BTPrimaryKey))
            {
                string temp = cryptography.DecryptString(BTPrimaryKey) ?? string.Empty;
                if (!string.IsNullOrEmpty(temp))
                {
                    projectModel.ReqHeaders.PrimaryKey = BTPrimaryKey;
                    projectModel.ReqHeaders.RequestId = temp.Replace(projectModel.EncryptedKeys.Key1.Substring(0, 8), "").Replace(projectModel.EncryptedKeys.Key1.Substring(projectModel.EncryptedKeys.Key1.Length - (projectModel.EncryptedKeys.Key1.Length / 2)), "");
                    if (projectModel.ReqHeaders.RequestId.Length != 12)
                    {
                        return new Response().ReturnMsg(false, "Invalid RequestId.",null, projectModel);
                    }
                    else
                    {
                        string[] RequestIdArray = new string[2]
                        {
                                projectModel.ReqHeaders.RequestId.Substring(0,8),
                                projectModel.ReqHeaders.RequestId.Substring(projectModel.ReqHeaders.RequestId.Length-4)
                        };
                        if (projectModel.ReqHeaders.RequestId.Substring(0, 8) != RequestIdArray[0])
                        {
                            return new Response().ReturnMsg(false, "Invalid RequestId.",null, projectModel);
                        }
                        else
                        {
                            projectModel.Project_Api.ApiId = RequestIdArray[1];
                            projectModel.ResHeaders.ResponseId = "R"+projectModel.ReqHeaders.RequestId;
                            temp = projectModel.Project_Key.Substring(0, 8) + projectModel.ResHeaders.ResponseId + projectModel.Project_Key.Substring(projectModel.Project_Key.Length - (projectModel.Project_Key.Length / 2));
                            projectModel.ResHeaders.PrimaryKey = cryptography.EncryptString(temp);
                            Response.Headers.Add("BT-Primary-Key", projectModel.ResHeaders.PrimaryKey);
                        }
                    }
                }
                else
                {
                    return new Response().ReturnMsg(false, "Invalid BT-Primary-Key.",null, projectModel);
                }
            }
            else
            {
                return new Response().ReturnMsg(false, "Missing Primary Key.",null, projectModel);
            }
            //Get ApiID Information
            Dictionary<string, object> dic = new Dictionary<string, object>()
            {
                {"Session","Get" },
                {"ApiId",projectModel.Project_Api.ApiId }
            };
            using (DataTable data = database.selectSP("uspApi", dic))
            {
                if (data != null)
                {
                    DataRow row = data.Rows[0];
                    projectModel.Project_Api.ApiName = row["ApiIDName"].ToString() ?? string.Empty;
                    projectModel.Project_Api.ApiURL = row["ApiIDURL"].ToString() ?? string.Empty;
                    projectModel.Project_Api.ApiType = row["ApiType"].ToString() ?? string.Empty;
                    if (row["IdentityCheck"].ToString() == "1")
                    {
                        projectModel.ReqHeaders.IdentityCheck = true;
                    }
                }
            }
            //Verifying CustomHeader BT-Identity-Key
            if (projectModel.ReqHeaders.IdentityCheck)
            {
                if (Request.Headers.TryGetValue("BT-Identity-Key".ToLower(), out var EncryptedBTIdentityKey))
                {
                    string temp = new Session().Verify(EncryptedBTIdentityKey);
                    if (!string.IsNullOrEmpty(temp))
                    {
                        // Read the response content
                        return new Response().ReturnMsg(false, "Session Expired. You have been logout.", null, projectModel);
                    }
                    string IdentityKey = cryptography.DecryptString(EncryptedBTIdentityKey);
                    if (!string.IsNullOrEmpty(IdentityKey))
                    {
                        projectModel.ReqHeaders.IdentityKey = IdentityKey;
                        Response.Headers.Add("BT-Identity-Key", EncryptedBTIdentityKey);
                    }
                    else
                    {
                        return new Response().ReturnMsg(false, "Invalid Identity Key.", null, projectModel);
                    }
                }
                else
                {
                    return new Response().ReturnMsg(false, "Missing Identity Key.", null, projectModel);
                }
            }
            if(!string.IsNullOrEmpty(projectModel.ReqHeaders.PrimaryKey) || (projectModel.ReqHeaders.IdentityCheck && string.IsNullOrEmpty(projectModel.ReqHeaders.IdentityKey)))
            {
                //Verifying CustomHeader BT-Signature
                if (Request.Headers.TryGetValue("BT-Signature".ToLower(), out var BTSignature))
                {
                    dynamic tempHeader = new ExpandoObject();
                    tempHeader.ProjectId = projectModel.Project_Id;
                    tempHeader.PrimaryKey = projectModel.ReqHeaders.PrimaryKey;
                    if (projectModel.ReqHeaders.IdentityCheck)
                    {
                        tempHeader.IdentityKey = projectModel.ReqHeaders.IdentityKey;
                    }
                    string tempSign = JsonConvert.SerializeObject(tempHeader) + ":" + JsonConvert.SerializeObject(model);
                    //string temp = projectModel.Project_Api.currentDate + projectModel.EncryptedKeys.Key1.Substring(0, 8) + projectModel.ReqHeaders.RequestId + projectModel.EncryptedKeys.Key1.Substring(projectModel.EncryptedKeys.Key1.Length - (projectModel.EncryptedKeys.Key1.Length / 2)) + projectModel.Project_Api.currentDate;
                    tempSign = cryptography.ComputeSha256Hash(tempSign);

                    if (tempSign != BTSignature)
                    {
                        return new Response().ReturnMsg(false, "Invalid BT-Signature.", null, projectModel);
                    }
                    else
                    {
                        projectModel.ReqHeaders.Signature = BTSignature;
                        tempSign = projectModel.Project_Api.currentDate + projectModel.EncryptedKeys.Key1.Substring(0, 8) + projectModel.ResHeaders.ResponseId + projectModel.EncryptedKeys.Key1.Substring(projectModel.EncryptedKeys.Key1.Length - (projectModel.EncryptedKeys.Key1.Length / 2)) + projectModel.Project_Api.currentDate;
                        projectModel.ResHeaders.Signature = cryptography.ComputeSha256Hash(tempSign);
                        Response.Headers.Add("BT-Signature", projectModel.ResHeaders.Signature);
                    }
                }
                else
                {
                    return new Response().ReturnMsg(false, "Missing Signature.", null, projectModel);
                }
            }

            //Verified Header End
            string json = System.Text.Json.JsonSerializer.Serialize(model);
            projectModel.Project_Api.RequestContent = json;
            //return Ok(projectModel);
            using (HttpClient client = new HttpClient())
            {
                
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
                httpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
                //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                if (projectModel.ReqHeaders.IdentityCheck && !string.IsNullOrEmpty(projectModel.ReqHeaders.IdentityKey))
                {
                    httpRequestMessage.Headers.Add("IdentityKey", projectModel.ReqHeaders.IdentityKey);
                }
                if (projectModel.Project_Api.ApiType.Equals("POST"))//Insert or Get Data
                {
                    httpRequestMessage.Method = HttpMethod.Post;
                    //response = await client.PostAsync(url, content);
                }else if (projectModel.Project_Api.ApiType.Equals("GET"))//Get Only
                {
                    httpRequestMessage.Method = HttpMethod.Get;
                    //response = await client.GetAsync(url,content);
                }else if (projectModel.Project_Api.ApiType.Equals("PUT"))//Update Data
                {
                    httpRequestMessage.Method = HttpMethod.Put;
                    //response = await client.PutAsync(url, content);
                }else if (projectModel.Project_Api.ApiType.Equals("DELETE"))//Update Data
                {
                    httpRequestMessage.Method = HttpMethod.Delete;
                    //response = await client.PutAsync(url, content);
                }
                httpRequestMessage.RequestUri = new Uri(projectModel.Project_Api.ApiURL);
                HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
                if (response.Headers.TryGetValues("IdentityKey",out var values))
                {
                    //string temp = values.ToString();
                    projectModel.Project_Api.IdentityKeyResponse = true;
                    string EncryptedIdentityKey = cryptography.EncryptString(values.FirstOrDefault());
                    int Minutes = 0;
                    if(response.Headers.TryGetValues("IdentityTime",out var time))
                    {
                        Minutes = Convert.ToInt32(time.FirstOrDefault());
                        string temp = new Session().Insert(EncryptedIdentityKey, Minutes);
                        if (!string.IsNullOrEmpty(temp))
                        {
                            // Read the response content
                            return new Response().ReturnMsg(false, "Login Failed.", null, projectModel);
                        }
                    }
                }
                string responseString = await response.Content.ReadAsStringAsync();
                bool ObjectCheck = false;
                if (responseString.StartsWith("{")&& responseString.EndsWith("}"))
                {
                    ObjectCheck = true;
                }
                projectModel.Project_Api.ResponseContent = responseString;
                logger.Info($"{JsonConvert.SerializeObject(projectModel)}");
                dynamic tempHeader = new ExpandoObject();
                tempHeader.ProjectId = projectModel.Project_Id;
                tempHeader.PrimaryKey = projectModel.ReqHeaders.PrimaryKey;
                if (projectModel.ReqHeaders.IdentityCheck)
                {
                    tempHeader.IdentityKey = projectModel.ReqHeaders.IdentityKey;
                }
                string tempSign = JsonConvert.SerializeObject(tempHeader) + ":" + JsonConvert.SerializeObject(projectModel.Project_Api.ResponseContent);
                if (response.IsSuccessStatusCode)
                {
                    if (ObjectCheck)
                    {
                        // Read the response content
                        return new Response().ReturnMsg(true, "Success", projectModel.Project_Api.ResponseContent,projectModel);
                    }
                    else
                    {
                        // Read the response content
                        return new Response().ReturnMsg(true, responseString,null, projectModel);
                    }
                }
                else
                {
                    // Read the response content
                    return new Response().ReturnMsg(false, responseString, null, projectModel);
                }
            }
        }
    }
}

