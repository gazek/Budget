using System.Collections.Generic;
using System.Web.Http;
using Budget.API.Models;
using Budget.DAL;
using Budget.API.Services;
using Budget.DAL.Models;
using System.Linq;
using Microsoft.AspNet.Identity;
using System;
using Budget.API.Services.OFXClient;
using System.Security.Principal;
using System.Linq.Expressions;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace Budget.API.Controllers
{
    [RoutePrefix("api/FinancialInstitution")]
    public class FinancialInstitutionController : ControllerBase<FinancialInstitutionModel>
    {
        public FinancialInstitutionController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        [Route("", Name = "CreateFI")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(FinancialInstitutionCreateBindingModel model)
        {
            Func<int, string> location = x => Url.Link("GetFiById", new { id = x });
            return base.Create<FinancialInstitutionCreateBindingModel, IPrincipal>(model, User);
        }

        [Route("{id}", Name = "GetFiById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            return base.Get(id);
        }

        [Route("", Name = "GetAllFi")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult GetAll()
        {
            string userId = User.Identity.GetUserId();

            // filters
            var filter = new List<Expression<Func<FinancialInstitutionModel, bool>>>();
            filter.Add(fi => fi.UserId == userId);

            return base.GetAll<FinancialInstitutionModel, string>(filter, null, fi => fi.Name);
        }
        
        [Route("{id}", Name = "UpdateFI")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult Update(int id, FinancialInstitutionUpdateBindingModel model)
        {
            model.Id = id;
            return base.Update(id, model);
        }

        [Route("{id}/credentials", Name = "UpdateFILogin")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult UpdateLogin(int id, FinancialInstitutionUpdateLoginBindingModel model)
        {
            model.Id = id;
            VerifyModel();
            return Update<FinancialInstitutionUpdateLoginBindingModel>(id, model);
        }

        [Route("{id}/GetAccountList", Name = "GetAccountListFromBank")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetAccountList(int id)
        {
            GetRecordAndIsAuthorized(id);

            if (!_requestIsOk)
            {
                return _errorResponse;
            }

            // configure the ofx account list request
            OfxClient.RequestConfig.OfxFid = _record.OfxFid;
            OfxClient.RequestConfig.OfxOrg = _record.OfxOrg;
            OfxClient.RequestConfig.Password = AesService.DecryptStringFromBytes(_record.PasswordHash);
            OfxClient.RequestConfig.RequestType = OFXRequestConfigRequestType.AccountList;
            OfxClient.RequestConfig.URL = new Uri(_record.OfxUrl);
            OfxClient.RequestConfig.Username = _record.Username;
            Guid clientId;
            if (Guid.TryParse(_record.CLIENTUID, out clientId))
            {
                OfxClient.RequestConfig.ClientUID = clientId;
            }

            // Build request
            OfxClient.BuildRequest();

            // Make request
            OfxClient.ExecuteRequest();

            // check request status
            if (OfxClient.Requestor.Status && OfxClient.Requestor.OFX != null)
            {
                OfxClient.ParseResponse();

                if (!OfxClient.Parser.SignOnRequest.Status)
                {
                    return BadRequest(OfxClient.Parser.SignOnRequest.Code + ": " + OfxClient.Parser.SignOnRequest.Message);
                }

                return Ok(OfxClient.Parser.Accounts.Select(x => ModelMapper.EntityToListView(x, id)));
            }

            if (!OfxClient.Requestor.Status)
            {
                return BadRequest(OfxClient.Requestor.ErrorMessage);
            }

            return InternalServerError();
        }

        [Route("OfxDump", Name = "OfxDump")]
        [HttpGet]
        public IHttpActionResult OfxDump()
        {
            // need to pull listing of OFX  here because OFX Home API does
            // not implement CORS
            string xml = String.Empty;
            string url = "http://www.ofxhome.com/api.php?dump=yes";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                xml = reader.ReadToEnd();
            }

            // xml response is malformed, it has multiple roots
            // adding enclosing tag to xml
            int index = xml.IndexOf('>');
            string fixedXml = xml.Substring(0, index+2) + "<institutionlist>" + xml.Substring(index+1) + "</institutionlist>";

            // further malformed
            // has & instead of &amp;
            string pattern = @"&(?!amp;)";
            fixedXml = Regex.Replace(fixedXml, pattern, "&amp;");

            // parse xml
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fixedXml);
            XmlNodeList fiNodes = doc.GetElementsByTagName("institution");
            List<OfxDumpViewModel> fiList = new List<OfxDumpViewModel>();
            foreach (XmlNode n in fiNodes)
            {
                int ofxId;
                int.TryParse(n.SelectSingleNode("fid").InnerText, out ofxId);
                OfxDumpViewModel fi = new OfxDumpViewModel
                {
                    Name = n.SelectSingleNode("name").InnerText,
                    OfxId = ofxId,
                    OfxOrg = n.SelectSingleNode("org").InnerText,
                    OfxUrl = n.SelectSingleNode("url").InnerText
                };
                fiList.Add(fi);
            }

            // sort by name
            fiList.Sort(delegate (OfxDumpViewModel x, OfxDumpViewModel y)
            {
                return x.Name.CompareTo(y.Name);
            });

            return Ok(fiList);
        }

    }
}
