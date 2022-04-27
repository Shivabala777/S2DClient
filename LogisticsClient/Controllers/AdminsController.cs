using LogisticsClient.AppLogger;
using LogisticsClient.Models;
using LogisticsClient.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace LogisticsClient.Controllers
{
    public class AdminsController : Controller
    {
        private readonly ILoggerManager logger;
        public static string baseURL;
        private readonly IConfiguration _configuration;

        public AdminsController(IConfiguration configuration, ILoggerManager logger)
        {
            _configuration = configuration;
            baseURL = _configuration.GetValue<string>("BaseURL");
            this.logger = logger;

        }
        public IActionResult Login()
        {
            this.logger.LogInformation("Passing to the login page");
            HttpContext.Session.Clear();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Admin admin)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Inputs are not valid. Please give Email and Password.";
                return View();
            }

            List<Admin> admins = new List<Admin>();
            admins = await GetAdmin();
            var obj = admins.Where(a => a.Username.Equals(admin.Username) && a.Password.Equals(admin.Password)).FirstOrDefault();

            if (obj != null) //User found
            {
                HttpContext.Session.SetString("AdminId", obj.Id.ToString());
                HttpContext.Session.SetString("UserName", obj.Username.ToString());
                return RedirectToAction("AdminDashBoard");
            }
            else
            {
                ViewBag.Message = "User not found for given Email and Password";
                return View();
            }

        }
        [HttpGet]
        public async Task<List<Admin>> GetAdmin()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            HttpClient client = new HttpClient(clientHandler);

            string JsonStr = await client.GetStringAsync(baseURL + "/api/admins");
            var result = JsonConvert.DeserializeObject<List<Admin>>(JsonStr);
            return result;
        }
        public async Task<IActionResult> AdminDashBoard()
        {
            if (HttpContext.Session.GetString("AdminId") != null)
            {
                this.logger.LogInformation($"{HttpContext.Session.GetString("UserName")} logged in");
                ViewBag.Email = HttpContext.Session.GetString("UserName");
                List<Customer> customers = await GetCustomer();
                return View(customers);
            }
            else
                return RedirectToAction("Login");
        }
        public async Task<List<Customer>> GetCustomer()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            HttpClient client = new HttpClient(clientHandler);

            string JsonStr = await client.GetStringAsync(baseURL + "/api/Customers");
            var result = JsonConvert.DeserializeObject<List<Customer>>(JsonStr);
            return result;
        }
        [HttpGet]
        public ActionResult CreateCustomer()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(Customer customer)
        {
            //order.CustomerId = Convert.ToInt16(HttpContext.Session.GetString("Id"));
            //var AccessMail = HttpContext.Session.GetString("Email");
            Customer receivedCustomer = new Customer();

            HttpClientHandler clientHandler = new HttpClientHandler();


            var httpClient = new HttpClient(clientHandler);


            StringContent content = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");

            using (var response = await httpClient.PostAsync(baseURL + "/api/customers", content))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                receivedCustomer = JsonConvert.DeserializeObject<Customer>(apiResponse);
                if (receivedCustomer != null)
                {
                    ViewBag.Message = "Registration Successful";
                    return RedirectToAction("Login","Customers");
                }
            }


            ViewBag.Message = "Your Record not Created!!! Please try again";
            return View();


        }
        public async Task<List<Order>> GetOrder()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            HttpClient client = new HttpClient(clientHandler);

            string JsonStr = await client.GetStringAsync(baseURL + "/api/orders");
            var result = JsonConvert.DeserializeObject<List<Order>>(JsonStr);
            return result;
        }
        public async Task<IActionResult> Approve(int id)
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("UserName")} passing to order approval page");
            List<Order> customers=await GetOrder();
            return View(customers.FirstOrDefault(t => t.OrderId == id));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(int id,Order updatedOrder)
        {
            //Order updatedOrder = (Order)collection;
            updatedOrder = await GetOrder(id); 
            //updatedOrder.OrderId = id;
            updatedOrder.ApprovalStatus = "Approved";

            updatedOrder.Charges=CalculateCharges(updatedOrder);
            updatedOrder.DeliveryStatus = "OrderApproved";
            //Console.WriteLine("Id:" + id);
            //Console.WriteLine("Charges:" + updatedOrder.Charges);
            using (var httpClient = new HttpClient())
            {
                StringContent contents = new StringContent(JsonConvert.SerializeObject(updatedOrder), Encoding.UTF8, "application/json");



                using (var response = await httpClient.PutAsync(baseURL + "/api/orders/" + id, contents))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();




                    if (apiResponse != null)
                    {
                       // Console.WriteLine(apiResponse);
                        ViewBag.Message = "orders Updated Successfully";
                        return RedirectToAction("Orders","Customers");
                    }
                    else
                        ViewBag.Message = "order updation Failed";
                }



            }



            return View();



        }
        [NonAction]
        public static int CalculateCharges(Order order)
        {
            int price = order.Kilometres * 5 + order.Weight * 5;
            return price;
        }
        public async Task<IActionResult> Decline(int id)
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("UserName")} passing to order rejection page");
            List<Order> customers = await GetOrder();
            return View(customers.FirstOrDefault(t => t.OrderId == id));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Decline(int id, Order updatedOrder)
        {
            //Order updatedOrder = (Order)collection;
            Console.WriteLine(id);
            updatedOrder = await GetOrder(id);
            //updatedOrder.OrderId = id;
            updatedOrder.ApprovalStatus = "Declined";

            //updatedOrder.Charges = CalculateCharges(updatedOrder);
            using (var httpClient = new HttpClient())
            {
                StringContent contents = new StringContent(JsonConvert.SerializeObject(updatedOrder), Encoding.UTF8, "application/json");



                using (var response = await httpClient.PutAsync(baseURL + "/api/orders/" + id, contents))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();




                    if (apiResponse != null)
                    {
                        ViewBag.Message = "orders Updated Successfully";
                        return RedirectToAction("Orders", "Customers");
                    }
                    else
                        ViewBag.Message = "order updation Failed";
                }



            }



            return View();



        }
        public async Task<IActionResult> Tracking()
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("UserName")} passing to orders list page for which tracking is to be updated");
            List<Customer> customers = await GetCustomer();
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            List<Order> orders = await GetOrder();
            List<Order> orderfiltered = orders.Where(o => o.ApprovalStatus == "Approved").ToList();
            foreach (var order in orderfiltered)
            {
                Customer cust = customers.FirstOrDefault(c => c.Id == order.CustomerId);
                OrderDetails order_details = new OrderDetails()
                {
                    OrderId = order.OrderId,
                    ApprovalStatus = order.ApprovalStatus,
                    Charges = order.Charges,
                    CustomerName = cust.Name,
                    DeliveryStatus = order.DeliveryStatus,
                    Destination = order.Destination,
                    Kilometres = order.Kilometres,
                    ProductName = order.ProductName,
                    Source = order.Source,
                    Weight = order.Weight
                };
                orderDetails.Add(order_details);
            }
            return View(orderDetails);
        }
        public async Task<IActionResult> UpdateTracking(int id)
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("UserName")} passing to update tracking page");
            Order order= await GetOrder(id);
            return View(order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateTracking(int id,Order updatedOrder)
        {

            updatedOrder.OrderId = id;

            Console.WriteLine("Name:" + updatedOrder.ApprovalStatus);

            using (var httpClient = new HttpClient())
            {
                StringContent contents = new StringContent(JsonConvert.SerializeObject(updatedOrder), Encoding.UTF8, "application/json");



                using (var response = await httpClient.PutAsync(baseURL + "/api/orders/" + id, contents))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();




                    if (apiResponse != null)
                    {
                        Console.WriteLine(apiResponse);
                        ViewBag.Message = "Customer Updated Successfully";
                        return RedirectToAction("Tracking");
                    }
                    else
                        ViewBag.Message = "Customer updation Failed";
                }



            }



            return View();



        }
        public async Task<Order> GetOrder(int id)
        {

            var accessEmail = HttpContext.Session.GetString("Email");
            Order receivedOrder = new Order();

            HttpClientHandler clientHandler = new HttpClientHandler();

            var httpClient = new HttpClient(clientHandler);

            using (var response = await httpClient.GetAsync(baseURL + "/api/orders/" + id))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    receivedOrder = JsonConvert.DeserializeObject<Order>(apiResponse);
                }
                else
                    ViewBag.StatusCode = response.StatusCode;
            }
            return receivedOrder;
        }
       
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("UserName")} passing to delete customer detail page");
            List<Customer> customers = await GetCustomer();
            return View(customers.FirstOrDefault(t => t.Id == id));
        }



        [HttpPost]
        public async Task<ActionResult> DeleteCustomer(int id, Customer customer)
        {

            HttpClientHandler clientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(clientHandler);
            var response = await httpClient.DeleteAsync(baseURL + "/api/customers/" + id);
            string apiResponse = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(AdminDashBoard));
        }
       


    }
}
