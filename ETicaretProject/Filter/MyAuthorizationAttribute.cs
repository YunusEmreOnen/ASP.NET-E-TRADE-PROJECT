using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaretProject.Filter
{
    //Yetki kontrol işlemlerim için bir class oluşturdum ve bu classa gerekli sınıfları ve arayüzleri  ekledim.
    public class MyAuthorizationAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        //Kullanıcı tiplerini tutumak için bir property oluşturdum ve set işlemini engelledim.
        public int ActionMemberType { get; private set; }
        //Sadece giriş yapılıp yapılmadığını kontrol eder.
        public MyAuthorizationAttribute()
        {

        }
        /// <summary>
        /// verilen numara ve üzeri  kontrol yapar.
        /// </summary>
        /// <param name="_memberType">yetki numarası</param>
        public MyAuthorizationAttribute(int _memberType)
        {
            this.ActionMemberType = _memberType;
        }
        //
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var member = (DB.Members)HttpContext.Current.Session["LogonUser"];
            if (member==null)
            {
                filterContext.Result = new RedirectResult("/i/Index");
            }
            else
            {
                var memberType = (int)member.MemberType;
                if (memberType < ActionMemberType)
                {

                    filterContext.Result = new RedirectResult("/i/Index");
                }
                else
                {
                }
            }
        }
    }
}