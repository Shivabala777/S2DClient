using LogisticsClient.AppLogger;
using LogisticsClient.Models;
using LogisticsClient.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace LogisticsClient.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ILoggerManager logger;
        public static string baseURL;
        private readonly IConfiguration _configuration;
        public CustomersController(IConfiguration configuration, ILoggerManager logger)
        {
            _configuration = configuration;
            baseURL = _configuration.GetValue<string>("BaseURL");
            this.logger = logger;

        }
        public IActionResult Login()
        {
            //HttpContext.Session.Clear();
            this.logger.LogInformation("Passing to the login page");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Customer admin)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Inputs are not valid. Please give Email and Password.";
                return View();
            }

            List<Customer> customers = new List<Customer>();
            customers = await GetCustomer();
            var obj = customers.Where(a => a.Email.Equals(admin.Email) && a.Password.Equals(admin.Password)).FirstOrDefault();

            if (obj != null) //User found
            {
                HttpContext.Session.SetString("Id", obj.Id.ToString());
                HttpContext.Session.SetString("Email", obj.Email.ToString());
                return RedirectToAction("CustomerDashBoard");
            }
            else
            {
                ViewBag.Message = "User not found for given Email and Password";
                return View();
            }

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
        public async Task<IActionResult> CustomerDashBoard()
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("Email")} logged in");
            if (HttpContext.Session.GetString("Id") != null)
            {
                ViewBag.Email = HttpContext.Session.GetString("UserName");
                int id = Convert.ToInt16(HttpContext.Session.GetString("Id"));
                this.logger.LogInformation($"{HttpContext.Session.GetString("Email")} logged in");
                List<Order> orders= await GetOrder();
                var order = orders.Where(o => o.CustomerId == id).FirstOrDefault();
                if (order != null)
                {
                    this.logger.LogInformation($"{HttpContext.Session.GetString("Email")} passing to thier order page");
                    return View(order);
                }
                else
                {
                    this.logger.LogInformation($"{HttpContext.Session.GetString("Email")} passing to request order page");
                    return RedirectToAction("CreateOrder");
                }
            }
            else
                return RedirectToAction("Login");
            
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
       
        public ActionResult CreateOrder()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            order.CustomerId = Convert.ToInt16(HttpContext.Session.GetString("Id"));
            var AccessMail = HttpContext.Session.GetString("Email");
            Order receivedOrder = new Order();

            HttpClientHandler clientHandler = new HttpClientHandler();


            var httpClient = new HttpClient(clientHandler);


            StringContent content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");

            using (var response = await httpClient.PostAsync(baseURL + "/api/orders", content))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                receivedOrder = JsonConvert.DeserializeObject<Order>(apiResponse);
                if (receivedOrder != null)
                {
                    return RedirectToAction("CustomerDashBoard");
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
        public async Task<IActionResult> Orders()
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("UserName")} passing to orders page");
            List<Customer> customers=new List<Customer>();
            customers = await GetCustomer();
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            List<Order> orders = new List<Order>();
            orders = await GetOrder();
            foreach(var order in orders)
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
        public async Task<IActionResult> DeleteOrder(int id)
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("Email")} passing to delete order page");
            Order order = await GetOrder(id);
            return View(order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteOrder(int id, IFormCollection collection)
        {

            var accessEmail = HttpContext.Session.GetString("Email");
            HttpClientHandler clientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(clientHandler);
            var response = await httpClient.DeleteAsync(baseURL + "/api/orders/" + id);
            string apiResponse = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(CustomerDashBoard));


        }
        public async Task<IActionResult> TrackOrder()
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("Email")} passing to order tracking page");
            int id = Convert.ToInt16(HttpContext.Session.GetString("Id"));
            List<Order> orders =await GetOrder();
            Order order=orders.FirstOrDefault(o=>o.CustomerId==id);
            return View(order);
        }
        public async Task<IActionResult> CustomerDetails()
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("Email")} passing to their details page");
            int id = Convert.ToInt16(HttpContext.Session.GetString("Id"));
            List<Customer> customers = await GetCustomer();
            Customer customer = customers.FirstOrDefault(c => c.Id == id);
            return View(customer);
        }
        public async Task<ActionResult> EditCustomer(int id)
        {
            this.logger.LogInformation($"{HttpContext.Session.GetString("Email")} passing to their details update page");
            List<Customer> customers = await GetCustomer();
            return View(customers.FirstOrDefault(t => t.Id == id));
            /*Customer customer = await GetCustomer(id);
            return View(customer);*/
        }



        // POST: ProductsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCustomer(int id, Customer updatedCustomer)
        {
            updatedCustomer.Id = id;



            using (var httpClient = new HttpClient())
            {
                StringContent contents = new StringContent(JsonConvert.SerializeObject(updatedCustomer), Encoding.UTF8, "application/json");



                using (var response = await httpClient.PutAsync(baseURL + "/api/customers/" + id, contents))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();




                    if (apiResponse != null)
                    {
                        ViewBag.Message = "Customer Updated Successfully";
                        return RedirectToAction("CustomerDetails", "Customers");
                    }
                    else
                        ViewBag.Message = "Customer updation Failed";
                }



            }



            return View();



        }

    }
}
