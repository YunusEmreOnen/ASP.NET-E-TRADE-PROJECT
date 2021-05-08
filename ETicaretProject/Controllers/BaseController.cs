using ETicaretProject.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaretProject.Controllers
{
    //Bu controlerımda herhangi bir action olmayacak constructionımı miras alınabilir duruma getirmek için oluşturduğum bir controler olarak kullandım.
    public class BaseController : Controller
    {
        //Contructionımın propertysinin set komutunu private atadım ve bu class dışarısından herhangi bir değişklik yapılmasını engelledim.
        //Aynı zamanda access modifier özelliğini protected yaparak sadece bu sınııfı miras alanların erişmesini sağladım.
        protected ETicaretEntities context { get; private set; }
        
        public BaseController()
        {
            context = new ETicaretEntities();
            //Parent_Id si null olan categories nesnelerimi bir viewbag nesnesine atadım ve controllerim içerisine tanımladım.
            ViewBag.MenuCategories = context.Categories.Where(x => x.Parent_Id == null).ToList();
        }

        // DB ime kayıt yaparken herseferinden Session üzerinden erişim sağlamak yerine metot yazdım.
        protected DB.Members CurrentUser()
        {
            if (Session["LogonUser"] == null) return null;
            return (DB.Members)Session["LogonUser"];
        }
        protected int CurrentUserId()
        {
        
            if (Session["LogonUser"] == null) return 0;
            return ((DB.Members)Session["LogonUser"]).Id;
        }     
        protected bool IsLogon()
        {
            if (Session["LogonUser"] == null) return false;
            else return true;
         
        }
        /// <summary>
        /// Tüm alt kategorileri getirir.
        /// </summary>
        /// <param name="cat">Hangi categorinin alt katagroileri getirilsin.</param>
        /// <returns></returns>
        protected List<Categories> GetChildCategories(Categories cat)
        {
            var result =new List<Categories>();
            //Alt kategorilerini listeye attım.
            result.AddRange(cat.Categories1);
            //alt kategorilerin de alt kategorilerini almak için döndüm.
            foreach (var item in cat.Categories1)
            {
                //Metodumu her altcategori için  yineledim.
                var list = GetChildCategories(item);
                result.AddRange(list);
            }

            return result;
        }


    }
}