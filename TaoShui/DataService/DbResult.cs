using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaoShui.DataService
{
    public class DbResult
    {
        private readonly bool _isSuccess;
        private readonly IEnumerable<DbEntityValidationResult> _validationErrors;
        private readonly string _msg;

        public DbResult(bool isSuccess, IEnumerable<DbEntityValidationResult> validationErrors, string msg)
        {
            _validationErrors = validationErrors;
            _msg = msg ?? string.Empty;
            _isSuccess = isSuccess;
        }

        public bool IsSuccess
        {
            get { return _isSuccess; }
        }

        public bool IsValidationFailed
        {
            get { return !string.IsNullOrEmpty(ValidationErrorMsg); }
        }

        public IEnumerable<DbEntityValidationResult> ValidationErrors
        {
            get { return _validationErrors; }
        }

        public string Msg
        {
            get { return _msg; }
        }

        public string ValidationErrorMsg
        {
            get { return GetValidationErrors(ValidationErrors); }
        }

        public string CombinedMsg
        {
            get
            {
                if (IsSuccess)
                {
                    return Msg;
                }
                else
                {
                    return Msg + ":\r\n" + ValidationErrorMsg;
                }
            }
        }

        public static string GetValidationErrors(IEnumerable<DbEntityValidationResult> errors)
        {
            var errorMsg = string.Empty;
            if (errors == null)
            {
                return errorMsg;
            }
            var resultList = errors.ToList();
            if (resultList.Any())
                foreach (var result in resultList)
                    if (!result.IsValid)
                        foreach (var error in result.ValidationErrors)
                            errorMsg += error.ErrorMessage + "\r\n";
            if (!string.IsNullOrEmpty(errorMsg) && errorMsg.Length > 1)
            {
                errorMsg = errorMsg.Substring(0, errorMsg.Length - 2);
            }
            return errorMsg;
        }
    }
}
