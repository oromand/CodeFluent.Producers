using System.Data;

namespace CodeFluent.Producers.WebApiController
{
    class WebApiUtils
    {
        /// <summary>
        /// Tries to guess webapi type given a dbType
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static string getWebApiUrlTypeFromDbType(DbType dbType)
        {
            string result = "";
            switch (dbType)
            {
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    result = "int";
                    break;
                case DbType.Boolean:
                    result = "bool";
                    break;
                case DbType.Date:
                    result = "datetime:regex(\\\\d{4}-\\\\d{2}-\\\\d{2})";
                    break;
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    result = "datetime";
                    break;
                case DbType.Decimal:
                    result = "decimal";
                    break;
                case DbType.Guid:
                    result = "guid";
                    break;
                default:
                    throw new System.Exception("CodeFluent Web Api Producer, unable to map type " + dbType.ToString());
            }

            return result;
        }
    }
}
