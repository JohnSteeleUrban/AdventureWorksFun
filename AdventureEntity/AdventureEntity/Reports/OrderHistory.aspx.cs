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
    public partial class OrderHistory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int id = int.Parse(Request.QueryString["id"]);
                var sql = @"SELECT c.[CustomerID]
                                          ,c.[PersonID]
	                                      ,h.[OrderDate]
                                          ,h.[DueDate] 
                                          ,h.[CustomerID]  
                                          ,d.[LineTotal]
                                          ,h.[Comment]   
	                                      ,p.[Name]
	                                      ,n.[LastName]
	                                      ,n.[FirstName]
	                                      ,h.[TotalDue]
                                      FROM [AdventureWorks2012].[Sales].[Customer] c
                                      JOIN [AdventureWorks2012].[Person].[Person] n
                                      ON c.PersonID = n.BusinessEntityID
                                      JOIN [AdventureWorks2012].[Sales].SalesOrderHeader h
                                      ON c.CustomerID = h.CustomerID
                                      JOIN [AdventureWorks2012].[Sales].SalesOrderDetail d
                                      ON h.SalesOrderID = d.SalesOrderID
                                      JOIN [AdventureWorks2012].[Production].Product p
                                      ON  d.ProductID = p.ProductID
                                      WHERE PersonID = @id";

                String ConnString = ConfigurationManager.ConnectionStrings["AdventureWorksModel"].ConnectionString;
                SqlConnection conn = new SqlConnection(ConnString);
                SqlDataAdapter adapter = new SqlDataAdapter();
                var command = new SqlCommand(sql, conn);
                command.Parameters.AddWithValue("@id", id);
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
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/OrderHistory.rdlc");
                ReportDataSource datasource = new ReportDataSource("DataSet1", detailDataTable);
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.DataSources.Add(datasource);
            }
        }
    }
}