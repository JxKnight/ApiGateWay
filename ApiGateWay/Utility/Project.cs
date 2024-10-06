using System;
using System.Data;
using ApiGateWay.Model;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace ApiGateWay.Utility
{
	public class Project
	{
        Database database = new Database();
        public ProjectModel GetProject(string ProjectId,string ProjectName,ProjectModel model)
		{
            string StoredProcedure = "uspProjects";
            Dictionary<string, object> dic = new Dictionary<string, object>()
            {
                {"Session","Get" },
            };
            if (!string.IsNullOrEmpty(ProjectId))
            {
                dic.Add("ProjectId", ProjectId);
            }
            if (!string.IsNullOrEmpty(ProjectName))
            {
                dic.Add("ProjectId", ProjectName);
            }
            using (DataTable data = database.selectSP(StoredProcedure, dic))
            {
                if (data != null)
                {
                    DataRow row = data.Rows[0];
                    if (!string.IsNullOrEmpty(row["ProjectName"].ToString()))
                    {
                        model.Project_Name = row["ProjectName"].ToString() ?? string.Empty;
                    }
                }
            }
            return model;
        }
    }
}

