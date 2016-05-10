using CodeFluent.Model;
using CodeFluent.Model.Common.Design;
using CodeFluent.Producers.CodeDom;
using CodeFluent.Runtime.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace CodeFluent.Producers.WebApiController
{
    public class WebApiBaseProducer : CodeDomBaseProducer
    {
        private WebApiControllerProducer _webApiControllerProducer;
        private Dictionary<string, string> dictPackageBaseClasses;
        private Dictionary<string, string> dictPackageAuthorize;

        [DefaultValue(false)]
        [Category("Source Production")]
        [DisplayName("Use Default Base Package In Path")]
        [Description("If set to true, uses default package base in file generation")]
        [ModelLevel(ModelLevel.Normal)]
        public bool UsesDefaultBasePackageInPath
        {
            get
            {
                return XmlUtilities.GetAttribute(Element, "UsesDefaultBasePackageInPath", false, CultureInfo.InvariantCulture);
            }
            set
            {
                XmlUtilities.SetAttribute(Element, "UsesDefaultBasePackageInPath", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        [DefaultValue("")]
        [Category("Source Production")]
        [DisplayName("Packages Base Classes")]
        [Description("List of string defining base class for specific namespace. If no value provided, will use System.Web.Http.ApiController base class. Ex: RootPackage.package1=FedPeche33.Web.Controllers.Association.BaseAssociationController;RootPackage.package2=System.Web.Http.ApiController;")]
        [ModelLevel(ModelLevel.Normal)]
        public string PackagesBaseClasses
        {
            get
            {
                return XmlUtilities.GetAttribute(Element, "PackagesBaseClasses", (string)null);
            }
            set
            {
                XmlUtilities.SetAttribute(Element, "PackagesBaseClasses", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        [DefaultValue("")]
        [Category("Source Production")]
        [DisplayName("Namespace Authorization")]
        [Description("List of string defining the Authorize attribute to be used. Ex: FedPeche33.Models.Association=Association;FedPeche33.Models.Global=Administrateur;FedPeche33.Models.Global.Town=")]
        [ModelLevel(ModelLevel.Normal)]
        public string NamespaceAuthorization
        {
            get
            {
                return XmlUtilities.GetAttribute(Element, "NamespaceAuthorization", (string)null);
            }
            set
            {
                XmlUtilities.SetAttribute(Element, "NamespaceAuthorization", value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public WebApiBaseProducer()
        {
            this.dictPackageBaseClasses = new Dictionary<string, string>();
            this.dictPackageAuthorize = new Dictionary<string, string>();
        }

        public override void Initialize(Project project, Producer producer)
        {
            base.Initialize(project, producer);
            prepareBaseClassesForPackages();
            prepareAuthorizeForPackages();
        }

        private void prepareAuthorizeForPackages()
        {
            dictPackageAuthorize.Clear();
            if (this.NamespaceAuthorization != null && this.NamespaceAuthorization != "")
            {
                string[] packageAuthorizeList = this.NamespaceAuthorization.Split(';');
                foreach (string currentPackageBaseClassList in packageAuthorizeList)
                {
                    if (currentPackageBaseClassList.Contains("="))
                    {
                        string[] packageAndAuthorize = currentPackageBaseClassList.Split('=');
                        this.dictPackageAuthorize.Add(packageAndAuthorize[0], packageAndAuthorize[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Split the string to have a dictionnary of package = class
        /// </summary>
        protected void prepareBaseClassesForPackages()
        {
            dictPackageBaseClasses.Clear();
            if (this.PackagesBaseClasses != null && this.PackagesBaseClasses != "")
            {
                string[] packageBaseClassList = this.PackagesBaseClasses.Split(';');
                foreach (string currentPackageBaseClassList in packageBaseClassList)
                {
                    if (currentPackageBaseClassList.Contains("="))
                    {
                        string[] packageAndClass = currentPackageBaseClassList.Split('=');
                        this.dictPackageBaseClasses.Add(packageAndClass[0], packageAndClass[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to find a matching package in dictPackageBaseClasses.
        /// While not found, remove a level
        /// If found, returns the value
        /// If not returns base default class System.Web.Htpp.ApiController
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string getAssociatedBaseClassForEntity(Entity entity)
        {
            string defaultBaseClass = "System.Web.Http.ApiController";
            bool found = false;
            string entityNamespace = entity.ClrFullTypeName;
            while (!found && entityNamespace.Contains("."))
            {
                if (this.dictPackageBaseClasses.ContainsKey(entityNamespace))
                {
                    found = true;
                    defaultBaseClass = this.dictPackageBaseClasses[entityNamespace];
                }
                else
                {
                    entityNamespace = entityNamespace.Substring(0, entityNamespace.LastIndexOf("."));
                }
            }
            return defaultBaseClass;
        }

        public string getAssociatedAuthorizeForEntity(Entity entity)
        {
            string defaultAuthorize = "";
            bool found = false;
            string entityNamespace = entity.ClrFullTypeName;
            while (!found && entityNamespace.Contains("."))
            {
                if (this.dictPackageAuthorize.ContainsKey(entityNamespace))
                {
                    found = true;
                    defaultAuthorize = this.dictPackageAuthorize[entityNamespace];
                }
                else
                {
                    entityNamespace = entityNamespace.Substring(0, entityNamespace.LastIndexOf("."));
                }
            }
            return defaultAuthorize;
        }

        protected override string NamespaceUri
        {
            get
            {
                return Constants.NamespaceUri;
            }
        }

        //public override void Initialize(Project project, Producer producer)
        //{
        //    base.Initialize(project, producer);

        //}

        public override void Produce()
        {
            //            Entity roleClaimEntity = ProjectUtilities.FindByEntityType(project, EntityType.RoleClaim);

            foreach (Entity e in Producer.Project.Entities)
            {
                _webApiControllerProducer = new WebApiControllerProducer(this, e, getAssociatedBaseClassForEntity(e), getAssociatedAuthorizeForEntity(e));
                _webApiControllerProducer.Produce(true);
            }
        }
    }

    public class Constants
    {
        public const string NamespaceUri = "http://www.ixcys.fr/codefluent/producers.webApiControllerProducer/2016/1";
    }
}
