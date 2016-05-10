using CodeFluent.Model;
using CodeFluent.Model.Common.Templating;
using CodeFluent.Producers.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeFluent.Producers.WebApiController
{
    public class WebApiControllerProducer : SimpleTemplateProducer
    {

        public Entity Entity { get; set; }
        public WebApiBaseProducer Producer { get; set; }

        public string AssociatedBaseClass { get; set; }
        public string AssociatedAuthorize { get; set; }

        public WebApiControllerProducer(WebApiBaseProducer producer, Entity entity, string associatedBaseClass, string associatedAuthorize) : base(producer)
        {
            this.Entity = entity;
            this.Producer = producer;
            this.AssociatedBaseClass = associatedBaseClass;
            this.AssociatedAuthorize = associatedAuthorize;
        }
        public override bool IsWebType
        {
            get
            {
                return true;
            }
        }

        public override string TargetPath
        {
            get
            {
                string finalPath = "";
                if (Producer.UsesDefaultBasePackageInPath)
                {
                    finalPath = Path.Combine(base.Producer.FullTargetDirectory, Entity.Namespace.Replace(".", "\\"), Entity.Name) + "Controller.cs";                //string path = ConvertUtilities.Nullify(XmlUtilities.GetAttribute(Producer.Element, ConvertUtilities.Camel(this.TargetName) + "TargetPath", (string)null), true);
                }
                else
                {
                    string shortenedNamespace = Entity.Namespace.Substring(base.Producer.TargetBaseNamespace.Length + 1).Replace(".", "\\");
                    finalPath = Path.Combine(base.Producer.FullTargetDirectory, shortenedNamespace, Entity.Name) + "Controller.cs";                //string path = ConvertUtilities.Nullify(XmlUtilities.GetAttribute(Producer.Element, ConvertUtilities.Camel(this.TargetName) + "TargetPath", (string)null), true);
                }
                return finalPath;
                //if (path == null)
                //    return BaseType.GetFilePath(Producer.TargetBaseNamespace, TypeName, Namespace, Producer.FullTargetDirectory, null);

                //return Producer.GetFullRelativeDirectoryPath(path);
            }
        }

        protected override string DefaultNamespace
        {
            get { return base.Producer.Project.DefaultNamespace + base.Producer.WebNamespaceSuffix + ".Security"; }
        }

        protected override string DefaultTypeName
        {
            get
            {
                return "WebApiController";
            }
        }

        protected override Template CreateTemplate()
        {
            var template = base.CreateTemplate();
            template.AddReferenceDirective(typeof(CodeDomBaseProducer));
            template.AddReferenceDirective(typeof(WebApiControllerProducer));

            template.AddNamespaceDirective(typeof(Path));
            template.AddNamespaceDirective(typeof(StringBuilder));

            return template;
        }

        protected override void RaiseProduced()
        {
        }

        protected override bool RaiseProducing(IDictionary dictionary)
        {
            return true;
        }

        #region Method 
        /// <summary>
        /// Shouldn't generate method LoadByObject or having any parameter that are Object related
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool isMethodSuported(CodeFluent.Model.Code.Method method)
        {
            int nbObjectParam = 0;
            if (method != null)
            {
                foreach (CodeFluent.Model.Code.MethodParameter parameter in method.Parameters)
                {
                    if (parameter.DbType.ToString() == "Object")
                    {
                        nbObjectParam++;
                    }
                }

            }
            return method != null && nbObjectParam == 0;
        }

        #endregion

        public string getTypedParamForMethod(CodeFluent.Model.Code.Method method)
        {
            string result = "";
            if (method != null)
            {
                List<string> parameterArray = new List<string>();
                foreach (CodeFluent.Model.Code.MethodParameter parameter in method.Parameters)
                {
                    parameterArray.Add(parameter.ClrFullTypeName + " " + parameter.Name);
                }

                result = string.Join(",", parameterArray);
            }
            return result;
        }
        public string getUrlParamForMethod(CodeFluent.Model.Code.Method method)
        {
            string result = "";
            if (method != null)
            {
                List<string> parameterArray = new List<string>();
                foreach (CodeFluent.Model.Code.MethodParameter parameter in method.Parameters)
                {
                    if (parameter.ClrFullTypeName == "System.String")
                    {
                        parameterArray.Add("{" + parameter.Name + "}");
                    }
                    else
                    {
                        parameterArray.Add("{" + parameter.Name + ":" + WebApiUtils.getWebApiUrlTypeFromDbType(parameter.DbType) + "}");
                    }
                }

                result = string.Join("/", parameterArray);
            }
            return result;
        }
        public string getParamForMethod(CodeFluent.Model.Code.Method method)
        {
            string result = "";
            if (method != null)
            {
                List<string> parameterArray = new List<string>();
                foreach (CodeFluent.Model.Code.MethodParameter parameter in method.Parameters)
                {
                    parameterArray.Add(parameter.Name);
                }

                result = string.Join(",", parameterArray);
            }
            return result;
        }

        /// <summary>
        /// Returns the most probable location of the method 
        /// Either entityName or entityNameCollection
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public string getLocationForMethod(CodeFluent.Model.Code.Method method)
        {
            //TODO find a way to have the method from the correct class
            //here we take the assumption it would be in EntityNameCollection (most likely user defined function)
            string result = "";
            if (method != null)
            {
                if (method.ReturnTypeName != null)
                {
                    result = method.ReturnTypeName;
                }
                else
                {
                    result = Entity.SetFullTypeName;
                }
            }

            return result;
        }

        public IEnumerable<CodeFluent.Model.Code.Method> getUserDefinedMethods()
        {
            List<string> defaultMethods = new List<string>()
            {
                "LoadAll",
                "Sort",
                "Remove",
                "IndexOf",
                "GetEnumerator",
                "Contains",
                "Clear",
                "AddByEntityKey",
                "Add",
                "Trace",
                "Insert",
                "Reload",
                "GetHashCode",
                "Equals",
                "CompareTo",
                "CopyTo",
                "CopyFrom",
                "Clone",
                "Validate",
                "DeleteByKey",
                "Delete",   //we want to have a little bit more logic over this one
                "SaveByRef",
                "SaveAll",
                "Save",
                "Load",
                "LoadBy"+Entity.Name,
            };
            //IEnumerable<CodeFluent.Model.Code.Method> filteredMethods = Entity.Methods.Where(m => !defaultMethods.Contains(m.Name));
            return Entity.Methods.Where(m => !defaultMethods.Contains(m.Name));
        }

        public string getParamKeyTypedForEntity()
        {
            //result should be Guid accountId, Guid defaultId
            string result = "";
            List<string> parameterArray = new List<string>();
            foreach (CodeFluent.Model.Property property in Entity.AllKeyProperties)
            {
                parameterArray.Add(property.TypeName + " " + property.Name);
            }

            result = string.Join(",", parameterArray);
            return result;
        }

        public string getParamKeyForEntity()
        {
            //result should be Guid accountId, Guid defaultId
            string result = "";
            List<string> parameterArray = new List<string>();
            foreach (CodeFluent.Model.Property property in Entity.AllKeyProperties)
            {
                parameterArray.Add(property.Name);
            }

            result = string.Join(",", parameterArray);
            return result;
        }

        public string getUrlParamKeyForEntity()
        {
            string result = "";
            List<string> parameterArray = new List<string>();
            foreach (CodeFluent.Model.Property property in Entity.AllKeyProperties)
            {
                //TODO if typename is string 
                parameterArray.Add("{" + property.Name + ":" + property.TypeName.ToLower() + "}");
            }

            result = string.Join("/", parameterArray);
            return result;

        }
    }

}
