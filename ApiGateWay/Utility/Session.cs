using System;
using System.Data;

namespace ApiGateWay.Utility 
{
	public class Session
	{
		Database database = new Database();
		private string StoredProcedure = "uspSession";
		public string Verify(string IdentityKey)
		{
			Dictionary<string, object> dic = new Dictionary<string, object>()
			{
				{"Session","VerifySession" },
				{"IdentityKey",IdentityKey }
			};
			using (DataTable data = database.selectSP(StoredProcedure, dic))
			{
				if (data != null)
				{
					DataRow row = data.Rows[0];
					if (!row["VerifySession"].ToString().Equals("1"))
					{
						return row["Error"].ToString();
					}
				}
			}
			return string.Empty;
		}

		public string Insert(string IdentityKey,int Minutes)
		{
            Dictionary<string, object> dic = new Dictionary<string, object>()
            {
                {"Session","InsertSession" },
                {"IdentityKey",IdentityKey },
                 {"IdentityTime", Minutes}
            };
            using (DataTable data = database.selectSP(StoredProcedure, dic))
            {
                if (data != null)
                {
                    DataRow row = data.Rows[0];
                    if (!row["InsertSession"].ToString().Equals("1"))
                    {
                        return row["Error"].ToString();
                    }
                }
            }
            return string.Empty;
        }

        public string Update(string IdentityKey, int Minutes)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>()
            {
                {"Session","UpdateSession" },
                {"IdentityKey",IdentityKey },
                 {"IdentityTime", Minutes}
            };
            using (DataTable data = database.selectSP(StoredProcedure, dic))
            {
                if (data != null)
                {
                    DataRow row = data.Rows[0];
                    if (!row["InsertSession"].ToString().Equals("1"))
                    {
                        return row["Error"].ToString();
                    }
                }
            }
            return string.Empty;
        }
    }
}

