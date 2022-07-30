using Cores.Utilities;
using Cores.Helpers;
using Server.Common;
using Database.Models;
using MongoDB.Entities;
using Newtonsoft.Json;
using Common.Services;
using jsreport.Local;
using jsreport.Binary;
using jsreport.Types;

namespace PaymentWeb.Services
{
    public class ReportService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private string _certificateFolder = "";
        private string _reportTemplateContent = "";
        //
        public ReportService(IHttpClientFactory httpClientFactory,
                          IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            Init();
        }

        void Init()
        {
            try
            {
                //Create folder
                _certificateFolder = @$"{_hostingEnvironment.WebRootPath}/BHVCertificates";
                if (!Directory.Exists(_certificateFolder))
                {
                    Directory.CreateDirectory(_certificateFolder);
                }

                //Load report template content
                string templateFilename = @$"{_hostingEnvironment.WebRootPath}/ReportTemplates/FlashCare.html";
                _reportTemplateContent = MyFile.Load_ToStringUTF8(templateFilename);
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "ReportService", "Init", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
        }

        #region FlashCare
        public string Create_FlashCareCertificate(mdSaleOrder saleOrder)
        {
            try
            {
                //Validate
                if (string.IsNullOrWhiteSpace(_reportTemplateContent))
                {
                    MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "ReportService", "FlashCare", "Exception", ReturnCode.Error_ByServer, "Chưa setting report template cho FlashCare");
                    return "";
                }
                //make report content
                var reportConent = Make_FlashCareContent(_reportTemplateContent, saleOrder);

                //Create report
                //var rs = new LocalReporting()
                //                .KillRunningJsReportProcesses()
                //                .UseBinary(JsReportBinary.GetBinary())
                //                .AsUtility()
                //                .Create();
                ////
                //var report = rs.RenderAsync(new RenderRequest()
                //{
                //    Template = new jsreport.Types.Template()
                //    {
                //        Recipe = Recipe.ChromePdf,
                //        Engine = Engine.None,
                //        Content = reportConent  //"<h1>Hello world</h1>"
                //    }
                //}).Result;

                //Write to file
                //string fileName = $"{_certificateFolder}/{saleOrder.PolicyNo}.pdf";
                //using (var fs = File.Create(fileName))
                //{
                //    report.Content.CopyTo(fs);
                //}
                string filename = $"{_certificateFolder}/{saleOrder.PolicyNo}.html";
                MyFile.Write_ToString_Unicode(filename, reportConent);

                //Return web link
                //return @$"{MyData.BaseUrl}/BHVCertificates/{saleOrder.PolicyNo}.pdf";
                return @$"{MyData.BaseUrl}/BHVCertificates/{saleOrder.PolicyNo}.html";
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "ReportService", "Create_FlashCareCertificate", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            return "";
        }

        public string Make_FlashCareContent(string templateContent, mdSaleOrder saleOrder)
        {
            try
            {
                //Skip check
                if (string.IsNullOrWhiteSpace(templateContent)) return "";

                //PolicyNo
                templateContent = templateContent.Replace("{PolicyNo}", saleOrder.PolicyNo);
                //CusFullname
                templateContent = templateContent.Replace("{CusFullname}", saleOrder.CusFullname);
                //CusCitizenID
                templateContent = templateContent.Replace("{CusCitizenID}", saleOrder.CusCitizenID);
                //CusEmail
                templateContent = templateContent.Replace("{CusEmail}", saleOrder.CusEmail);
                //CusPhone
                templateContent = templateContent.Replace("{CusPhone}", saleOrder.CusPhone);
                //Address
                templateContent = templateContent.Replace("{Address}", saleOrder.Address);
                //Fullname
                templateContent = templateContent.Replace("{Fullname}", saleOrder.Fullname);
                //Sex
                templateContent = templateContent.Replace("{Sex}", saleOrder.Sex == "1" ? "Name" : "Nữ");
                //DateOfBirth
                templateContent = templateContent.Replace("{DateOfBirth}", saleOrder.DateOfBirth.ToString("dd/MM/yyyy"));
                //CitizenID
                templateContent = templateContent.Replace("{CitizenID}", saleOrder.CitizenID);
                //BenefitAmount
                templateContent = templateContent.Replace("{BenefitAmount}", saleOrder.BenefitAmount.ToString("N0"));
                //PaymentAmount
                templateContent = templateContent.Replace("{PaymentAmount}", saleOrder.PaymentAmount.ToString("N0"));
                //EffectiveSttTime
                templateContent = templateContent.Replace("{EffectiveSttTime}", saleOrder.EffectiveSttDate.ToLocalTime().ToString("HH:mm"));
                //EffectiveSttDate
                templateContent = templateContent.Replace("{EffectiveSttDate}", saleOrder.EffectiveSttDate.ToLocalTime().ToString("dd/MM/yyyy"));
                //EffectiveEndTime
                templateContent = templateContent.Replace("{EffectiveEndTime}", saleOrder.EffectiveEndDate.ToLocalTime().ToString("HH:mm"));
                //EffectiveEndDate
                templateContent = templateContent.Replace("{EffectiveEndDate}", saleOrder.EffectiveEndDate.ToLocalTime().ToString("dd/MM/yyyy"));
                //
                return templateContent;
            }
            catch (Exception ex)
            {
                MyAppLog.WriteLog(MyConstant.LogLevel_Critical, "ReportService", "Make_FlashCareContent", "Exception", ReturnCode.Error_ByServer, ex.Message);
            }
            return "";
        }
        #endregion





    }




}
