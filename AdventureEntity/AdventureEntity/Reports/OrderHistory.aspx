<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OrderHistory.aspx.cs" Inherits="AdventureEntity.Reports.OrderHistory" %>
<%@ Register TagPrefix="rsweb" Namespace="Microsoft.Reporting.WebForms" Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>History Order Report</title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="margin: 0 auto;">
        
            <h4>Order History Report</h4>
            <br/>
            <br/>
        <asp:ScriptManager runat="server" EnablePageMethods="True"/>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" SizeToReportContent="True" ConsumeContainerWhitespace="true">
        </rsweb:ReportViewer>

    </div>
    </form>
</body>
</html>
