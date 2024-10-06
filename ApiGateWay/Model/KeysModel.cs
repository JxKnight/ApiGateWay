using System.Data;
using ApiGateWay.Utility;

namespace ApiGateWay.Model
{
    public class Keys
	{
		Database database = new Database();
		private string _Key1 = string.Empty, _Key2 = string.Empty;
		public Keys(string ProjectId)
		{
            DateTime currentDatetime = DateTime.Now;
            if (ProjectId.Equals("ACCESS"))
            {
                _Key1 = currentDatetime.ToString("yyyyMMddyyyyMMdd");
                _Key2 = currentDatetime.ToString("yyyyMMddyyyyMMdd");
            }
            else
            {
                string StoredProcedure = "uspKey";
                Dictionary<string, object> dic = new Dictionary<string, object>()
            {
                {"Session","GetKey" },
                {"ProjectId",ProjectId }
            };
                using (DataTable data = database.selectSP(StoredProcedure, dic))
                {
                    if (data != null)
                    {
                        DataRow row = data.Rows[0];
                        if (!string.IsNullOrEmpty(row["Key1"].ToString()))
                        {
                            _Key1 = row["Key1"].ToString() ?? string.Empty;
                            _Key2 = row["Key2"].ToString() ?? string.Empty;
                        }
                    }
                }
            }
        }
		public string Key1 { get { return _Key1; } }
        public string Key2 { get { return _Key2; } }
    }
}

