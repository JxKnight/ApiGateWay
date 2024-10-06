using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiGateWay.Model
{
	public class ProjectModel
	{
		public string Project_Id { get; set; } = string.Empty;
        public string Project_Id_Encrypted { get; set; } = string.Empty;
        public string Project_Name { get; set; } = string.Empty;
        public string Project_Key { get; set; } = string.Empty;
		public Keys EncryptedKeys { get; set; } = null;
		
        public RequestHeaders ReqHeaders { get; set; } = null;
        public APIModel Project_Api { get; set; } = null;
        public ResponseHeaders ResHeaders { get; set; } = null;
    }

	public class RequestHeaders
	{
		public string Signature { get; set; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public bool IdentityCheck { get; set; } = false;

        public string IdentityKey { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;//get from Session
    }

    public class ResponseHeaders
    {
        public string Signature { get; set; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public string ResponseId { get; set; } = string.Empty;
        public string IdentityKey { get; set; } = string.Empty;
    }
    public class APIModel
    {
        private DateTime Now = DateTime.Now;
        private string _API_URL = string.Empty;
        private string GuidString = Guid.NewGuid().ToString();
        public APIModel()
        {
            RequestLog = Now.ToString("yyyyMMdd-HHmmss") + GuidString;
            currentDate = Now.ToString("yyyyddMM");
        }
        public string RequestLog { get; set; }
        public string currentDate { get; set; }
        public string ApiId { get; set; } = string.Empty;//get from request headers
        public string ApiName { get; set; } = string.Empty;//get from DB
        public string ApiURL { get { return _API_URL + "?logs=" + RequestLog; } set { _API_URL = value; } }
        public string ApiType { get; set; } = string.Empty;
        public string RequestContent { get; set; }
        public bool IdentityKeyResponse { get; set; } = false;
        public string ResponseContent { get; set; }
    }
}

