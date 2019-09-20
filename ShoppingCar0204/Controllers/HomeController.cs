using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShoppingCar0204.Models;

namespace ShoppingCar0204.Controllers
{
    public class HomeController : Controller
    {
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        // GET: Home
        public ActionResult Index()
        {
            var products = db.tProduct.ToList();
            if (Session["Member"] == null)
            {
                return View("Index","_Layout",products);
            }
            return View("Index", "_LayoutMember", products);
        }
        //GET:Home/Login
        public ActionResult Login()
        {
            return View();
        }

        //POST:Home/Login
        [HttpPost]
        public ActionResult Login(string fUserId, string fPwd)
        {
            var member = db.tMember
                .Where(m => m.fUserId == fUserId && m.fPwd == fPwd)
                .FirstOrDefault();

            if (member == null)
            {
                ViewBag.Message = "帳密錯誤，登入失敗";
                return View();

            }

            Session["Welcome"] = member.fName + "Welcome";
            Session["Member"] = member;

            return RedirectToAction("Index");

        }


        //GET:Home/Register

        public ActionResult Register()
        {

            return View();
        }

        //POST:Home/Register

        [HttpPost]
        public ActionResult Register(tMember pMember)
        {
            if (ModelState.IsValid == false)
            {
                return View();
            }

            var member = db.tMember
                .Where(m => m.fUserId == pMember.fUserId)
                .FirstOrDefault();
            if (member==null)
            {
                db.tMember.Add(pMember);
                db.SaveChanges();
                return RedirectToAction("Login");

            }

            ViewBag.Message = "此帳號已有人使用，註冊失敗";


            return View();

        }
    


        //GET:Index/ShoppingCar
        public ActionResult ShoppingCar() /*會員購物車清單*/
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            var orderDetails = db.tOrderDetail.Where
            (m => m.fUserId == fUserId && m.fIsApproved == "否")
            .ToList();
            return View("ShoppingCar", "_LayoutMember", orderDetails);
        }

        public ActionResult AddCar(string fPId)
        {
            string fUserId = (Session["Member"] as tMember).fUserId;

            var currentCar = db.tOrderDetail
                .Where(m => m.fPId == fPId && m.fIsApproved == "否"
                 && m.fUserId == fUserId)
                 .FirstOrDefault();
                  if (currentCar==null)
                  {
                    var product = db.tProduct
                    .Where(m => m.fPId == fPId).FirstOrDefault();
                    tOrderDetail orderDetail = new tOrderDetail();
                    
                       orderDetail.fUserId = fUserId;
                       orderDetail.fPId = product.fPId;
                       orderDetail.fName = product.fName;
                       orderDetail.fPrice = product.fPrice;
                       orderDetail.fQty = 1;
                       orderDetail.fIsApproved = "否";
                       db.tOrderDetail.Add(orderDetail);
            }
            else
            {
                currentCar.fQty += 1;

            }
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }

        public ActionResult DeleteCar(int fid)
        {

            var orderDetail=db.tOrderDetail.
                Where(m => m.fId == fid).FirstOrDefault();

            db.tOrderDetail.Remove(orderDetail);
            db.SaveChanges();

            return RedirectToAction("ShoppingCar");

        }
        //結束購物流程，按下確認訂購按鈕並建立訂單主單
        [HttpPost]
        public ActionResult ShoppingCar(string fReceiver,string fEmail,string fAddress)
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            string guid = Guid.NewGuid().ToString();
            tOrder order = new tOrder();
            order.fOrderGuid = guid;
            order.fUserId = fUserId;
            order.fReceiver = fReceiver;
            order.fEmail = fEmail;
            order.fAddress = fAddress;
            order.fDate = DateTime.Now;
            db.tOrder.Add(order);

            //同時將購物車商品轉換成訂單明細並存入資料庫中。

            var carList = db.tOrderDetail
            .Where(m => m.fIsApproved == "否" && m.fUserId == fUserId)
            .ToList();
            foreach(var item in carList)
            {
                item.fOrderGuid = guid;   //給予現在產生的guid
                item.fIsApproved = "是";  //將訂單明細，fisApproved改成是

            }

            db.SaveChanges();
            return RedirectToAction("OrderList"); //返回訂單查詢功能列表

        }

        public ActionResult OrderList() //訂單查詢功能列表
        {
            string fUserId = (Session["Member"] as tMember).fUserId;  //顯示出目前使用者在系統中的歷史交易紀錄資料


            var orders = db.tOrder.Where(m => m.fUserId == fUserId).OrderByDescending(m => m.fDate).ToList();
            return View("orderList", "_LayoutMember", orders);
        }

        public ActionResult OrderDetail(string fOrderGuid) //在訂單查詢功能列表，裡面訂單明細的button
        {
            var orderDetails = db.tOrderDetail
                .Where(m => m.fOrderGuid == fOrderGuid).ToList();
            return View("OrderDetail", "_LayoutMember", orderDetails);
        }

        //GET :Index/Logout
        public ActionResult Logout()
        {

            Session.Clear();
            return RedirectToAction("Index");
        }

    }
}