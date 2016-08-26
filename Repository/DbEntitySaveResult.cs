using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class DbEntitySaveResult
    {
        public bool IsSaveSuccess { get; set; }

        public IList<DbEntityValidationResult> ValidationResults { get; set; }

        public string ValidationErrors
        {
            get
            {
                var errorMsg = string.Empty;
                if (ValidationResults != null && ValidationResults.Any())
                {
                    foreach (var result in ValidationResults)
                    {
                        if (!result.IsValid)
                        {
                            foreach (var error in result.ValidationErrors)
                            {
                                errorMsg += error.ErrorMessage + "\r\n";
                            }
                        }
                    }
                }
                return errorMsg;
            }
        }
    }
}
