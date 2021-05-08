using ETicaretProject.Filter;
using ETicaretProject.Models.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaretProject.Controllers
{//oturum kontrolü  ekledim.
    [MyAuthorization]
    public class MessageController : BaseController
    {
        // GET: Message
        public ActionResult i()
        {
            int id = base.CurrentUserId();
            var user = context.Members.FirstOrDefault(x => x.Id == id);
            if (user == null) return RedirectToAction("Login", "Account");
            //Modelimin tipinde bir nesne oluşturdum null olmasın diye tanımladım.
            Models.Message.iModels model = new Models.Message.iModels();
            #region - Select List Item -
            model.Users = new List<SelectListItem>();
            //Member type sıfırdan büyük olanları listele
            var users = context.Members.Where(x => ((int)x.MemberType) > 0 && x.Id != id).ToList();
            //modelime DataBase imden gerekli özelliklere atamalar yaptım.
            model.Users = users.Select(x => new SelectListItem()
            {

                Value = x.Id.ToString(),
                Text = string.Format("{0} {1} ({2})", x.Name, x.Surname, x.MemberType.ToString())

            }).ToList();
            #endregion
            #region - Messages List-
            //Linq sorgumla mesaj gönderilen kişinin Idsi Oturmu kullanansa veya  gönderen  kişinin ID si Oturumu kullanansa Messejları çektim.
            var mList = context.Messages.Where(x => x.ToMemberId == id || x.MessageReplies.Any(y => y.Member_Id == id)).ToList();
            //Mlistemi modelimin mesajlar özelliğine attım.
            model.Messages = mList;
            #endregion
            return View(model);
        }
        //Parametre olarak modelimi aldım.
        public ActionResult SendMessage(Models.Message.SendMessageModel message)
        {
            int id = base.CurrentUserId();
            var user = context.Members.FirstOrDefault(x => x.Id == id);
            if (user == null) return RedirectToAction("Login", "Account");
            //DataBaseimden bir nesne türettim ve gerekli atamalarını yaptım.
            DB.Messages mesaj = new DB.Messages()
            {

                Id = Guid.NewGuid(),
                AddedDate = DateTime.Now,
                IsRead = false,
                Subject=message.Subject,
                ToMemberId=message.ToUserId
                

            };
            //Replece tablomun tipinde bir nesne türettim.
            var mRep = new DB.MessageReplies()
            {
                Id = Guid.NewGuid(),
                AddedDate=DateTime.Now,
                Member_Id=CurrentUserId(),
                Text=message.MessageBody
            };
            mesaj.MessageReplies.Add(mRep);
            context.Messages.Add(mesaj);
            context.SaveChanges();
            return RedirectToAction("i", "Message");
        }
        
        [HttpGet]
        public ActionResult MessageReplies(string id)
        {
           
            if (IsLogon() == false) return RedirectToAction("Login", "Account");
            var guid = new Guid(id);
            var currentId = CurrentUserId();
            //Databaseimdeki messages tablomun tipinde bir nesne oluşturdum ve içerisine Id si MessagesReplice ıd ile aynı olanı mesajı çektim. 
            DB.Messages message = context.Messages.FirstOrDefault(x => x.Id == guid);
            //Mesajın gönderildiği Id oturum açan kişinin ıd si ise görüldü olarak işaretle.
            if (message.ToMemberId==currentId)
            {
                message.IsRead = true;
                context.SaveChanges();
            }
                
            //modelimin tipinde bir nesne oluşturdum parametremi guid tipine çevirdim.
            MessageRepliesModel model = new MessageRepliesModel();
       

            //DB imden messageId si benim verdiğim parameterye eşit olanları Eklenme tarihine göre sıralayıp liste çevirdim ve modelime aktardım.
            model.MReplies = context.MessageReplies.Where(x => x.MessageId == guid).OrderBy(x=>x.AddedDate).ToList();
            return View(model);
        }
        //Mesajlara Yanıt oluşturduğum kısım.
        [HttpPost]
        public ActionResult MessageReplies(DB.MessageReplies message)
        {
            int usr = base.CurrentUserId();
            var user = context.Members.FirstOrDefault(x => x.Id == usr);
            if (user == null) return RedirectToAction("Login", "Account");

            message.AddedDate = DateTime.Now;
            message.Id = Guid.NewGuid();
            message.Member_Id = CurrentUserId();
            context.MessageReplies.Add(message);
            context.SaveChanges();
            return RedirectToAction("MessageReplies", "Message",new { id =message.MessageId });
        }

        //Layoutta drop downda görünecek mesajlar için oluşturdum.
        [HttpGet]
        public ActionResult RenderMessage()
        {
            RenderMessageModel model = new RenderMessageModel();
            var id = CurrentUserId();
            var mList = context.Messages.Where(x => x.ToMemberId == id || x.MessageReplies.Any(y => y.Member_Id == id)).OrderByDescending(x=>x.AddedDate);
            //modelime listemdki mesajlardan okunmamış olanlardan dört tanesini liste çevirip attım.
            model.Messages=mList.Where(x=>x.IsRead==false).Take(4).ToList();
            model.Count = mList.Where(x => x.IsRead == false).Count();
            //Modelime gerekli atamaları yapıp Partial View e yönlendirdim.
            return PartialView("_Messages", model);
        }

        public ActionResult RemoveMessageReplies(string id)
        {
            var guid = new Guid(id);
            //mesaj cevaplarını sildim.
            var mReplace = context.MessageReplies.Where(x => x.MessageId == guid);
            context.MessageReplies.RemoveRange(mReplace);
            //mesajın kendisini sildim.
            var message = context.Messages.FirstOrDefault(x => x.Id == guid);
            context.Messages.Remove(message);
            context.SaveChanges();

            //Silme işleminden sonra mesjlarcontrolırındaki i  actionına  yönlendirdim.
            return RedirectToAction("i", "Message");

        }
    }
}