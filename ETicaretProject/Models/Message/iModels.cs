using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETicaretProject.Models.Message
{
    public class iModels
    {
        //Enum Değerleri karşılamak için kullandım.
        public List<System.Web.Mvc.SelectListItem> Users { get; set; }
        public List<DB.Messages> Messages { get; set; }
    }
}