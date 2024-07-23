using System;

namespace API.Responses
{
    public class AccessResponse
    {
        public bool IsCreate { get; set; }
        public bool IsRead { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }
        public String Description { get; set; }
        public AccessResponse(bool isCreate, bool isRead, bool isUpdate, bool isDelete, String description)
        { 
            IsCreate = isCreate;
            IsRead = isRead;
            IsUpdate = isUpdate;
            IsDelete = isDelete;
            Description = description;
        }
    }
}
