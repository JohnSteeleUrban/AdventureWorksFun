using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;

namespace AdventureEntity.Reports
{
    public partial class SalesTerritory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var sql = @"SELECT [TerritoryID]
                                          ,[Name]
                                          ,[CountryRegionCode]
                                          ,[Group]
                                          ,[SalesYTD]
                                          ,[SalesLastYear]
                                          ,[CostYTD]
                                          ,[CostLastYear]
                                          ,[rowguid]
                                          ,[ModifiedDate]
                                      FROM [AdventureWorks2012].[Sales].[SalesTerritory]";

                String ConnString = ConfigurationManager.ConnectionStrings["AdventureWorksModel"].ConnectionString;
                SqlConnection conn = new SqlConnection(ConnString);
                SqlDataAdapter adapter = new SqlDataAdapter();
                var command = new SqlCommand(sql, conn);
                adapter.SelectCommand = command;

                DataTable detailDataTable = new DataTable();

                conn.Open();
                try
                {
                    adapter.Fill(detailDataTable);
                }
                finally
                {
                    conn.Close();
                }

                ReportViewer1.ProcessingMode = ProcessingMode.Local;
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/SalesTerritory.rdlc");
                ReportDataSource datasource = new ReportDataSource("DataSet1", detailDataTable);
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.DataSources.Add(datasource);
            }
        }
    }
}