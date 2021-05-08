using ETicaretProject.Filter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaretProject.Controllers
{ //Yetki kontrol Attributumu ekledim.
    [MyAuthorization(_memberType: 8)]
    public class ProductController : BaseController
    {
        // GET: Product
        public ActionResult i()
        {
         
            var products = context.Products.Where(x=>x.IsDeleted!=true).ToList();
            return View(products.OrderByDescending(x=>x.AddedDate).ToList());
        }
       
        //Ürün Düzenleme Sayfam.
        public ActionResult Edit(int id=0)
        {
           
            var product = context.Products.FirstOrDefault(x => x.Id == id);
            //Edit yaptığımız ürünün categori Id sini çektik.
            var categories = context.Categories.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()

            }).ToList();
            ViewBag.Categories = categories;
            return View(product);
        }
        //Ürün Update ve Insert işlemlerini yaptım.
        [HttpPost]
        public ActionResult Edit(DB.Products product)
        {
            var productImagePath = string.Empty;
            if (Request.Files != null && Request.Files.Count > 0)
            {
                //Dosya isteklerinden zaten tek bir tane olduğu için ilkini yakaladım ve Dosya olarak nereye kaydedeceğimi belirledim.
                var file = Request.Files[0];
                if (file.ContentLength > 0)
                {
                    var folder = Server.MapPath("~/Content/img/Product/");
                    var fileName = Guid.NewGuid() + ".jpg";
                    //Dosyamın fiziksel olarak kaydını yapmak için Path.Combine metodumla iki nesnemi birleştirdim.
                    file.SaveAs(Path.Combine(folder, fileName));

                    // Bu kısım database yazacağım kısım olduğu için fiziksel olarak kayıt ettiğim kısım değil proje içerisinde buludurduğum path i bu nesneme aktardım.
                    var filePath = "Content/img/Product/" + fileName;
                    productImagePath = filePath;
                }
            }
            if (product.Id>0)
            {
                //DataBaseimden ürünle bağlantımı kurudum.
                var dbproduct = context.Products.FirstOrDefault(x => x.Id == product.Id);
                dbproduct.Category_Id = product.Category_Id;
                dbproduct.ModifiedDate = DateTime.Now;
                dbproduct.Description = product.Description;
                dbproduct.IsContinued = product.IsContinued;
                dbproduct.Name = product.Name;
                dbproduct.Price = product.Price;
                dbproduct.UnitsInStock = product.UnitsInStock;
                product.ModifiedDate = DateTime.Now;
                dbproduct.IsDeleted = false;
                if (string.IsNullOrEmpty(productImagePath)==false)
                {
                    dbproduct.ProductImageName = productImagePath;
                }
            }
            else
            {
                product.AddedDate = DateTime.Now;
                product.IsDeleted = false;
                product.ProductImageName = productImagePath;
                context.Entry(product).State = System.Data.Entity.EntityState.Added;
            }
           
            context.SaveChanges();
            return RedirectToAction("i");
        }
        public ActionResult Delete(int id)
        {
           
            var pro = context.Products.FirstOrDefault(x => x.Id == id);
            pro.IsDeleted = true;
            context.SaveChanges();
            return RedirectToAction("i");
        }
    }
}