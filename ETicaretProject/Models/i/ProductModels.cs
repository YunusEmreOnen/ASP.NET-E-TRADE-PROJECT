using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETicaretProject.Models.i
{
    //Ürünlerim için bir model oluşturdum ve içerisine property olarak Product ve Comments tanımladım.
    public class ProductModels
    {
        public DB.Products Product { get; set; }
        public List<DB.Comments> Comments { get; set; }
    }
}