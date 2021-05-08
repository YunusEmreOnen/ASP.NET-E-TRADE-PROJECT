using ETicaretProject.DB;
using ETicaretProject.Filter;
using ETicaretProject.Models.Account;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaretProject.Controllers
{
    //Hesaplara yönelik sayfalar oluşturacağımdan dolayı Account controler adında yeni bir controler oluşturdum ve BaseControlerdan miras aldım.
    public class AccountController : BaseController
    {

        // GET: Account
        //Sayfaya ilk girdiğimde çalışacak kısım.
        [HttpGet]
        //Add view diyerek actionımla çağıracağım yeni bir view oluşturdum.
        public ActionResult Register()
        {
            return View();
        }
        //Sayfada kayıt ol butonuna bastığımda çalışacak kısım.
        [HttpPost]
        public ActionResult Register(Models.Account.RegisterModels user)
        {   //Bir try catch oluşturarak password ile rePassword html nesneme girelen değerlerin aynı olup olmadığını if ile modelim üzerinden kontrol ettim.
            try
            {
                if (user.rePassword != user.Member.Password)
                {
                    throw new Exception("Şifreler aynı değildir.");
                }
                if (context.Members.Any(x => x.Email == user.Member.Email))
                {
                    throw new Exception("Bu Email adresi kullanıyor.");
                }
                user.Member.MemberType = DB.MemberType.Customer;
                user.Member.AddedDate = DateTime.Now;
                context.Members.Add(user.Member);
                context.SaveChanges();
                //Kayıt işlemi başarılı olursa Login sayfama yönlendir.
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.ReError = ex.Message;
                return View();
                throw;
            }

        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(Models.Account.LoginModels model)
        {
            try
            //databaseimdeki password ve maili modelimle aynı ise user nesneme o elemanı at yada null geç.
            {
                var user = context.Members.FirstOrDefault(x => x.Password == model.Member.Password && x.Email == model.Member.Email);
                //nesnem null değilse Oturumuma user nesnemi ver ve index sayfama yönlendir. 
                if (user != null)
                {
                    Session["LogonUser"] = user;
                    return RedirectToAction("index", "i");
                }
                else
                {
                    ViewBag.ReError = "Kullanıcı bilgileriniz yanlış.";
                    return View();
                }

            }
            catch (Exception ex)
            {
                ViewBag.ReError = ex.Message;
                return View();
            }
        }
        public ActionResult Logout()
        {
            // Oturumumu null a çektim ve Login sayfama yönlendirdim.
            Session["LogonUser"] = null;
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public ActionResult Profil(int id = 0, string ad = "")
        {
            //Adreslerim için bir list oluşturdum.
            List<DB.Addresses> addresses = null;
            //Kullanıcı addreslerini tutmak için veritabanımın tipinde bir nesne türettim.
            DB.Addresses currentAddress = new DB.Addresses();
            //Oturum açılmadıysa 
            if (id == 0)
            {
                // Oturumumdan id çektim ve addreses içerisine linq sorgusu ile kullanıcı Id si aynı olan kullanıcının Addreslerini liste çevirip addreses listeme attım.
                id = base.CurrentUserId();
                addresses = context.Addresses.Where(x => x.Member_Id == id).ToList();
                //düzenleme işleminden gelen ad parametremi karşıladım ve boşmu diye kontrol ettikten sonra guide çevirip Db imden karşılaştırdım ve oluşturduğum nesneme aktardım.
                if (string.IsNullOrEmpty(ad) == false)
                {
                  
                    var guid = new Guid(ad);
                    currentAddress = context.Addresses.FirstOrDefault(x => x.Id == guid);
                }
            }
            //Parametre olarak aldığım id veritabanından karşılaştırdım ilk gelen nesneyi çektim.
            var user = context.Members.FirstOrDefault(x => x.Id == id);
            //user eğer nullsa index sayfama yönlendirdim.
            if (user == null) return RedirectToAction("index", "i");
            //Modelime userı verdim ve sayfama gönderdim.
            ProfilModels model = new ProfilModels()
            {
                Members = user,
                Addresses = addresses,
                CurrentAddress = currentAddress
            };
            return View(model);
        }
        [HttpGet]
        //oturum kontrolü  ekledim.
        [MyAuthorization]
        public ActionResult ProfilEdit()
        {
            int id = base.CurrentUserId();
            var user = context.Members.FirstOrDefault(x => x.Id == id);
            if (user == null) return RedirectToAction("index", "i");
            ProfilModels model = new ProfilModels()
            {
                Members = user
            };
            return View(model);
        }

        [HttpPost]
        //oturum kontrolü  ekledim.
        [MyAuthorization]
        public ActionResult ProfilEdit(ProfilModels model,string Password="")
        {
            try
            {
                int id = CurrentUserId();
                //oturum açan kullanıcı nesnesini aldık
                var updateMember = context.Members.FirstOrDefault(x => x.Id == id);
                updateMember.ModifiedDate = DateTime.Now;
                updateMember.Bio = model.Members.Bio;
                updateMember.Name = model.Members.Name;
                updateMember.Surname = model.Members.Surname;
                //Password girildiyse değişklik yap.
                if (!string.IsNullOrEmpty(Password))
                {
                    updateMember.Password = Password;
                }
  
                if (Request.Files != null && Request.Files.Count > 0)
                {
                    //Dosya isteklerinden zaten tek bir tane olduğu için ilkini yakaladım ve Dosya olarak nereye kaydedeceğimi belirledim.
                    var file = Request.Files[0];
                    if (file.ContentLength > 0)
                    {
                        var folder = Server.MapPath("~/Content/img/");
                        var fileName = Guid.NewGuid() + ".jpg";
                        //Dosyamın fiziksel olarak kaydını yapmak için Path.Combine metodumla iki nesnemi birleştirdim.
                        file.SaveAs(Path.Combine(folder, fileName));

                        // Bu kısım database yazacağım kısım olduğu için fiziksel olarak kayıt ettiğim kısım değil proje içerisinde buludurduğum path i bu nesneme aktardım.
                        var filePath = "Content/img/" + fileName;
                        updateMember.ProfileImageName = filePath;
                    }
                }
                context.SaveChanges();
                return RedirectToAction("Profil", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.MyError = ex.Message;
                int id = CurrentUserId();
                var viewModel = new Models.Account.ProfilModels()
                {
                    Members = context.Members.FirstOrDefault(x => x.Id == id)
                };
                return View(viewModel);
            }
        }

        [HttpPost]
        //oturum kontrolü  ekledim.
        [MyAuthorization]
        public ActionResult Address(DB.Addresses address)
        {
            //Null bir address nesnesi oluşturdum var olan kayıtları güncellemek için.
            DB.Addresses _address = null;
            //Eğer parametremin Id özleliği boş gelmiş ise yeni bir guid oluştur ve parmetreme ver.
            if (address.Id == Guid.Empty)
            {
                address.Id = Guid.NewGuid();
                address.AddedDate = DateTime.Now;
                //yeni kayıt olduğu için adresin kime ait olduğunu belirttim.
                address.Member_Id = base.CurrentUserId();
                context.Addresses.Add(address);
            }
            else
            // boş gelmemiş ise oluşturduğum nesneye gelen değerdeki Id ti DB imden bul ve oluşturduğum nesneye aktar.
            {
                _address = context.Addresses.FirstOrDefault(x => x.Id == address.Id);
                _address.ModifiedDate = DateTime.Now;
                _address.Name = address.Name;
                _address.AdresDescription = address.AdresDescription;


            }
            context.SaveChanges();
            //Account controlümdeki profile acctionına yönlendir.
            return RedirectToAction("Profil", "Account");
        }

        //Adres Silme işlemim.      
        [HttpGet]
        //oturum kontrolü  ekledim.
        [MyAuthorization]
        public ActionResult RemoveAddress(string id)
        {
            var guid = new Guid(id);
            var address = context.Addresses.FirstOrDefault(x => x.Id == guid);
            context.Addresses.Remove(address);
            context.SaveChanges();
            return RedirectToAction("Profil", "Account");
        }
        //Şifre Unuttum sayfası Actionu
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        //Şifre Unuttum sayfası Mail doğrulama işlemim.
        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            //Mail adresine sahip olan kişiyi çektim ve kontrolleri saplayıp yanıtları döndüm.
            var member = context.Members.FirstOrDefault(x => x.Email == email);
            if (member==null)
            {
                ViewBag.MyError = "Kayıtlı bir hesap bulunamadı.";
                return View();
            }
            else
            {
                var body = "Şifreniz:" + member.Password;
                MyMail mail = new MyMail(member.Email,"Şifremi Unuttum",body);
                mail.SendMail();
                TempData["Info"] = email + "mail adresinize şifre gönderilmiştir.";
                return RedirectToAction("Login");
            }

        }
    }
}