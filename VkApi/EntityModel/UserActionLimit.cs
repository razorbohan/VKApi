//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VkApi
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserActionLimit
    {
        public int Id { get; set; }
        public int ActionTypeId { get; set; }
        public System.DateTime LimitDateTime { get; set; }
        public int ProfileId { get; set; }
    
        public virtual UserActionType UserActionTypes { get; set; }
        public virtual Profile Profile { get; set; }
    }
}