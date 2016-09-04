using System.Collections.Generic;
using System.Data.Entity.Validation;

namespace TaoShui.DataService
{
    public class DbResult<TDto> : DbResultBase
        where TDto : new()
    {
        private readonly TDto _dto;

        public DbResult(bool isSuccess, IEnumerable<DbEntityValidationResult> validationErrors, string msg, TDto dto)
            : base(isSuccess, validationErrors, msg)
        {
            _dto = dto;
        }

        public TDto Dto
        {
            get { return _dto; }
        }
    }
}