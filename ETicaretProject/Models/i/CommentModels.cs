using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETicaretProject.Models.i
{
    public class CommentModels
    {
        public List<DB.Comments> Comments { get; set; }
        public DB.Products Product { get; set; }
    }
}