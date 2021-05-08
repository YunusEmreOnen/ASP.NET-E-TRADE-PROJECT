using ETicaretProject.DB;
using ETicaretProject.Filter;
using ETicaretProject.Models;
using ETicaretProject.Models.i;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaretProject.Controllers
{
    // Anasayfam için iControler adında bir controler oluşturdum ve otomatik olarak gerekli olan view klasörümüde kendsi oluşturdu.
    // Index i me Add View işlemi ile bir yeni bir layout ekledim.Aynı zamanda sitem için gerekli olan Site.css dosyasınıda content altına oluşturmuş oldum.
    public class iController : BaseController
    {

        // GET: i
        public ActionResult Index(int id = 0)
        {
            //IQueryable sayesinde Databesimize gerekli sorguları yapıp çagırma işlemini en sona bırakırız.
            IQueryable<DB.Products> products = context.Products.OrderByDescending(x=>x.AddedDate).Where(x => x.IsDeleted != true);
            DB.Categories category = null;
            //Id degeri 0 dan büyükse
            if (id > 0)
            {
                //category nesneme bağlantımdan id değeri varsa ata yoksa null bırak.
                category = context.Categories.FirstOrDefault(x => x.Id == id);
                //Alt kategorilerimi çektim.
                var allcategories = GetChildCategories(category);
                allcategories.Add(category);
               // categorilerin Id lerini çektim.
                var catIntList = allcategories.Select(x => x.Id).ToList();

                //Selecet * from Product Where Category_ıd in (catIntList)
                products = products.Where(x => catIntList.Contains(x.Category_Id));

            }
            //Modelimin tipinde bir nesne oluşturdum.
            var viewModel = new Models.i.IndexModel();
            //Nesneme databaseimden gerekli ürünleri aktardım.
            List<Products> lists = products.ToList();
            viewModel.Products = lists;
            viewModel.Category = category;
            return View(viewModel);
        }
        //Product sayfam için bir action oluşturdum ve id isimli parametresini default olarak 0  verdim.Add view diyerek product isimli bir view ekledim.
        [HttpGet]
        public ActionResult Product(int id = 0)
        {
            //pro değişkenime bağlantımı kullanarak ürünlerden actionımın parametresi ile aynı olanı atamasını istedim toksa default deger olark null verecek.
            var pro = context.Products.FirstOrDefault(x => x.Id == id);
            //Eğer değişkenim null ise index sayfama yönlendirecek.
            if (pro == null)
            {
                return RedirectToAction("index", "i");
            }
            // modelimin tipinde bir nesne tanımladım ve özelliklerime değerlerimi verdim.
            ProductModels model = new ProductModels()
            {
                Product = pro,
                Comments = pro.Comments.ToList()
            };
            //Modelimi sayfama verdim.
            return View(model);
        }
        //Productımın post işlemine parametre olarak comment verdim.
        [HttpPost]
        //oturum kontrolü  ekledim.
        [MyAuthorization]
        public ActionResult Product(DB.Comments comment)
        {
            //try içerisinde  yorumum nesneme gerekli atamaları yaptım ve DB e kayıt ettim.
            try
            {

                comment.Member_Id = base.CurrentUserId();
                comment.AddedDate = DateTime.Now;
                context.Comments.Add(comment);
                context.SaveChanges();

            }
            catch (Exception ex)
            {

                ViewBag.MyError = ex.Message;
            }
            //Post işlemi yaptığımda  sayfamın get işlemine yönlendirme yaptım ve sayfam yenilendi bu sayede yeni eklediğim yorumu gördüm.
            return RedirectToAction("Product", "i");
        }
        //Sepettime ekleme yaptığım action.
        [HttpGet]
        public ActionResult AddBasket(int id, bool remove=false)
        {
            //Modelimin tipinde bir list oluşturdum ve null atadım.
            List<Models.i.BasketModels> basket  = null;
            //Eğer Basket oturumum null ise basket listeme boş değer ata.
            if (Session["Basket"]==null)
            {
                basket = new List<Models.i.BasketModels>();
            }
            //Değilse baket nesneme oturumumdaki nesneleri at.
            else
            {
                basket = (List<Models.i.BasketModels>)Session["Basket"];
            }
            // Listemde eğer ürünüm varsa spetimdeki ürünü 1 arttır.
            if (basket.Any(x=>x.Product.Id==id))
            {
                var pro = basket.FirstOrDefault(x => x.Product.Id == id);
                if (remove && pro.Count>0)
                {
                    pro.Count -= 1;
                }
                else
                {   //ürün stoğum ürün countumdan büyükse Countu artır yoksa artırma işlemini yapma.
                    if (pro.Product.UnitsInStock> pro.Count)
                    {
                        pro.Count += 1; 
                    }
                    else
                    {
                        //ViewBag kullanmadım çünkü sonraki return de Action çağırdığımdan veri siliniyor.
                        //TempData geçerli istekten sonraki isteğe veri aktarmak için kullanılabilir.
                        TempData["MyError"] = "Yeterli stok yok.";
                    }
                }
               
            }
            //yoksa sepete id si ve değeriyle ekle
            else
            {
                var pro = context.Products.FirstOrDefault(x => x.Id == id);
                if (pro!=null && pro.IsContinued && pro.UnitsInStock>0)
                {
                    basket.Add(new Models.i.BasketModels()
                    {
                        Count = 1,
                        Product = pro
                    });
                }
                else 
                {
                    TempData["MyError"] = "Bu ürünün satışı yoktur.";
                }
           
            }
            //Sepetimdeki ürünlerden Countu 1 den küçükleri sil.
            basket.RemoveAll(x => x.Count < 1);
            //listemi oturumuma at.
            Session["Basket"] = basket;
            //Sepet sayfama yönlendir.
            return RedirectToAction("Basket", "i");
        }
        // Sepet sayfamı görmek için oluşturduğum action.
        [HttpGet]
        public ActionResult Basket()
        {
            //Sepetimdeki ürünleri oluşturduğum listeme aktardım.
            List<Models.i.BasketModels> model = (List<Models.i.BasketModels>)Session["Basket"];
            //Listem eğer null ise listeme boş bir değer atayarak null hatası alamızı engellemiş olduk.
            if (model==null)
            {
                model = new List<Models.i.BasketModels>();
            }
            //Eğer oturum açıldıysa edreslerimi Selectlistitem a çevirdim ve ViewBagime aktardım.
            if (base.IsLogon())
            {
                int currentId = CurrentUserId();
                ViewBag.CurrentAddresses = context.Addresses
                                                    .Where(x => x.Member_Id == currentId)
                                                    .Select(x => new SelectListItem()
                                                    {
                                                        Text = x.Name,
                                                        Value = x.Id.ToString()
                                                    }).ToList();
            }
           
            //Toplam fiyatı linq ile öğrendiğim ve oluşturduğum ViewBag e aktardım.
            ViewBag.TotalPrice = model.Select(x => x.Product.Price * x.Count).Sum();
            return View(model);
        }
        //Sepetten Silme işlemim.
        [HttpGet]
        public ActionResult RemoveBasket(int id)
        {
            List<Models.i.BasketModels> basket = (List<Models.i.BasketModels>)Session["Basket"];
            if (basket!=null)
            {
                if (id>0)
                {
                    basket.RemoveAll(x => x.Product.Id == id);
                }
                else if (id==0)
                {
                    basket.Clear();
                }
                Session["Basket"] = basket;
            }
            return RedirectToAction("Basket", "i");

        }

        //Siparişlerim sayfası için post ettiğim verileri tablolara bastığım  metodum.
        [HttpPost]
        //oturum kontrolü  ekledim.
        [MyAuthorization]
        public ActionResult Buy(string Address)
        {
            if (IsLogon())
            {
                try
                {
                    //Sepetteki değerleri aldım.
                    var basket = (List<Models.i.BasketModels>)Session["Basket"];
                    //Linq sorgum içerisinde metot kullanamıyacağımdan gelen address nesnemi Guid tipine çevirdim.
                    var guid = new Guid(Address);
                    var _address = context.Addresses.FirstOrDefault(x => x.Id == guid);
                    //Sipariş Verildi = SV
                    //Ödeme Bildirimi = OB
                    //Ödeme Onaylandı = OO
                    //Siparişlerimi almak için bir order nesnesi oluşturdum.
                    var order = new DB.Orders()
                    {
                        AddedDate = DateTime.Now,
                        Address = _address.AdresDescription,
                        Member_Id = CurrentUserId(),
                        Status = "SV",
                        //Id tablomda güvenlik amaçlı olarak uniqidentifier olduğı için Guid atadım.
                        Id = Guid.NewGuid()

                    };
                    //Sesionımdaki Basket listemi döndüm.
                    foreach (Models.i.BasketModels item in basket)
                    {
                        //DataBase imin tipinde bir nesne türettim ve Sipariş detaylarımı bu nesneye aktardım.
                        var orderDetail = new DB.OrderDetails();
                        orderDetail.AddedDate = DateTime.Now;
                        orderDetail.Price = item.Product.Price * item.Count;
                        orderDetail.Product_Id = item.Product.Id;
                        orderDetail.Quantity = item.Count;
                        //aynı şekilde hem güvenlik amaçlı hemde her Orderın(Siparişin) birden fazla Detailsı olabileceği için Id lerini kendim Guid olarak atadım.
                        orderDetail.Id = Guid.NewGuid();

                        //tukarda oluştruduğum order nesnemin OrderDetails özelliğine ekleme yaptım.
                        order.OrderDetails.Add(orderDetail);

                        //DataBaseimden productları çektim.
                        var _product = context.Products.FirstOrDefault(x => x.Id == item.Product.Id);
                        //Eğer product null değilse ve alınan ürün sayısı stoktan düşük veya eşitse.
                        if (_product!=null && _product.UnitsInStock >= item.Count)
                        {
                            //stoktan ürünleri düş.
                            _product.UnitsInStock = _product.UnitsInStock - item.Count;
                        }
                        //Değilse hatasını dönder.
                        else
                        {
                            throw new Exception(string.Format("{0} Ürünü için yeterli stok yoktur", item.Product.Name));
                        }

                    }
                    //Order Tabloma oluşturduğum order nesnemi ekledim ve alttada değişiklikleri kaydettim.
                    context.Orders.Add(order);
                    context.SaveChanges();
                    Session["Basket"] = null;
                }
                catch (Exception ex)
                {
                    //Hata mesajımı tempData ya  attım.
                    TempData["MyError"]= ex.Message;
                }
                return RedirectToAction("Buy","i");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
         
        }
        //Siparişlerim sayfası için get metodum.
        [HttpGet]
        //oturum kontrolü  ekledim.
        [MyAuthorization]
        public ActionResult Buy()
        {
            //Kullanıcı giriş yaptıysa sayfama 
            if (IsLogon())
            {
                //Kullanıcımın Id sini çektim.
                var currentId = CurrentUserId();  
                IQueryable<DB.Orders> orders;
                //Kullanıcı Admin ve üzeri ise Siparişlerinde ödeme bildirimi yapılmışları çek.
                if (((int)CurrentUser().MemberType)>8)
                {
                    orders = context.Orders.Where(x => x.Status == "OB");
                }
                //Kullanıcımın Id sine ait  Siparişlerini çektim.
                else
                {
                    orders = context.Orders.Where(x => x.Member_Id == currentId);
                }
                
                //Modelimin tipinde bir list oluşturdum.
                List<Models.i.BuyModels> model = new List<BuyModels>();
                //Siparişlerimi döndüm
                foreach (var item in orders)
                {
                    //byModel adında modelimin tipinde bir nesne oluşturdum ve bu nesneye değer atamaları yaptım.
                    var byModel = new BuyModels();
                    byModel.TotalPrice = item.OrderDetails.Sum(y => y.Price);
                    //Dönen nesnem liste olduğu için Joinle birleştirdim ve string halinde tuttum.
                    byModel.OrderName = string.Join(",", item.OrderDetails.Select(y => y.Products.Name + "(" + y.Quantity + ")"));
                    byModel.AddedDate = item.AddedDate.Date;
                    byModel.OrderStatus = item.Status;
                    byModel.Member = item.Members;
                    //nesneme Sipariş Id mi atmadığımdan dolayı Bildirim işleminde Id ye erişemedim.
                    byModel.OrderId = item.Id.ToString();
                    //Değerleri atadığım nesnemi modelime ekledim.
                    model.Add(byModel);
                }
                //Modelimi sayfama gönderdim.
                return View(model);
            }
            //Kullanıcı giriş yapmadıysa giriş acitonıma  yönlendir.
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }

        [HttpPost]
        //oturum kontrolü  ekledim.
        [MyAuthorization]
        //Gelen modelim Ajax ile gönderildiğinden JsonResult ile karşıladım.
        public JsonResult OrderNotification(OrderNotificationModel model)
        {
            //Siparişim gelmişmi diye kontrol ettim.
            if (string.IsNullOrEmpty(model.OrderId)==false)
            {
                //Sipariş Id mi veritabanından kontrol etmek için  Guid e çevirdim ve kotnrolü sağlayıp değerimi nesneme attadım.
                var guid = new Guid(model.OrderId);
                var order=context.Orders.FirstOrDefault(x => x.Id == guid);
                //order nesnem null değil ise model.OrderDescription ını ve Status u veritabanıma attım ve save işlemini yaptım.
                if (order!=null)
                {
                    order.Description = model.OrderDescription;
                    order.Status = "OB";
                    context.SaveChanges();
                }
            }
            //Herhangi bir yere değer göndermediğimden boş bıraktım.
            return Json("");
        }

        //Buy sayfamdan gelen Json isteğim.
        [HttpGet]
        public JsonResult GetOrder(string id)
        {
            var guid = new Guid(id);
            var order = context.Orders.FirstOrDefault(x => x.Id == guid);
            return Json(new { 
                Description=order.Description,
                Address=order.Address
            },JsonRequestBehavior.AllowGet);
        }
        //Ödeme onaylama İçin oluşturduğum POST işlemi
        [HttpPost]
        //oturum kontrolü  ekledim.
        [MyAuthorization]
        public JsonResult OrderComplated(string id,string text)
        {
            var guid = new Guid(id);
            var order = context.Orders.FirstOrDefault(x => x.Id == guid);
            order.Description = text;
            order.Status = "OO";
            context.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}