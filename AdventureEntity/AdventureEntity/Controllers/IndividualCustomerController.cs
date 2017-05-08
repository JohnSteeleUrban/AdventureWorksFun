using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using AdventureEntity.Models;
using AdventureEntity.ViewModel;
using PagedList;

namespace AdventureEntity.Controllers
{
    public class IndividualCustomerController : Controller
    {
        private AdventureWorksModel db = new AdventureWorksModel();
        private const int SALT_SIZE = 8;
        private const int NUM_ITERATIONS = 1000;

        private static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        private string _salt;
        private string _hash;

        // GET: IndividualCustomer
        public ActionResult Index(string sortingOrder, string searchData, string filterValue, int? pageNum)
        {
            var customers = db.Customers.Include(c => c.Person).Include(c => c.SalesTerritory).Include(c => c.Store);
            var individualCustomers = customers.Where(o => o.Person.PersonType.Equals("IN"));
            ViewBag.CurrentSortOrder = sortingOrder;
            ViewBag.SortingLast = String.IsNullOrEmpty(sortingOrder) ? "Last_Name" : "";
            ViewBag.SortingFirst = sortingOrder == "First_Name" ? "First_Name" : "Account";
            if (searchData != null)
            {
                if (searchData.Length > 0)
                {
                    pageNum = 1;
                    individualCustomers = individualCustomers.Where(o => o.Person.FirstName.ToUpper().Contains(searchData.ToUpper())
                                                                         || o.Person.LastName.ToUpper().Contains(searchData.ToUpper()));
                }
            }
            else
            {
                searchData = filterValue;
            }

            ViewBag.FilterValue = searchData;
            
            switch (sortingOrder)
            {
                case "Last_Name":
                    individualCustomers = individualCustomers.OrderByDescending(o => o.Person.LastName);
                    break;
                case "First_Name":
                    individualCustomers = individualCustomers.OrderBy(o => o.Person.FirstName);
                    break;
                default:
                    individualCustomers = individualCustomers.OrderBy(o => o.Person.LastName);
                    break;
            }
            int SizeOfPage = 8;
            int NumOfPage = (pageNum ?? 1);
            return View(individualCustomers.ToPagedList(NumOfPage, SizeOfPage));
        }

        // GET: IndividualCustomer/Details/5
        public ActionResult Details(int id)
        {
            IndividualViewModel individualViewModel = GetEditAndDetailData(id);
            return View(individualViewModel);
        }

        // GET: IndividualCustomer/Create
        public ActionResult Create()
        {
            IndividualViewModel vm = new IndividualViewModel();
            return View(vm);
        }

        // POST: IndividualCustomer/Create
        [HttpPost]
        public ActionResult Create([Bind(Include = "FirstName,LastName,PhoneNumber,BusinessEntityID,CardType,CardNumber,ExpMonth,ExpYear,Password,EmailAddress")] IndividualViewModel newCustomer)
        {
            try
            {
                BusinessEntity business = new BusinessEntity();
                business.rowguid = Guid.NewGuid();
                business.ModifiedDate = DateTime.Now;
                db.BusinessEntities.Add(business);
                db.SaveChanges(); //insert business into database then grab it and relate everything.
                var id = business.BusinessEntityID;

                Person person = new Person();
                person.ModifiedDate = DateTime.Now;
                person.PersonType = "IN";
                person.NameStyle = false;
                person.FirstName = newCustomer.FirstName;
                person.LastName = newCustomer.LastName;
                person.EmailPromotion = 1;
                person.rowguid = Guid.NewGuid();

                PersonPhone personPhone = new PersonPhone();
                personPhone.PhoneNumber = newCustomer.PhoneNumber;
                personPhone.PhoneNumberTypeID = 1;// 1 2 or 3.  Defaulting for now because limited time and dropdown lazy. May fix if there is time.
                personPhone.ModifiedDate = DateTime.Now;
                personPhone.BusinessEntityID = business.BusinessEntityID;

                CreditCard creditCard = new CreditCard();
                creditCard.CardType = newCustomer.CardType;
                creditCard.CardNumber = newCustomer.CardNumber;
                creditCard.ExpMonth = newCustomer.ExpMonth;
                creditCard.ExpYear = newCustomer.ExpYear;
                creditCard.ModifiedDate = DateTime.Now;

                PersonCreditCard personCreditCard = new PersonCreditCard();
                personCreditCard.ModifiedDate = DateTime.Now;
                personCreditCard.CreditCard = creditCard;

                Password password = new Password();
                HashPassword(newCustomer.Password);
                password.PasswordHash = _hash;
                password.PasswordSalt = _salt;
                password.rowguid = Guid.NewGuid();
                password.ModifiedDate = DateTime.Now;

                EmailAddress emailAddress = new EmailAddress();
                emailAddress.EmailAddress1 = newCustomer.EmailAddress;
                emailAddress.rowguid = Guid.NewGuid();
                emailAddress.ModifiedDate = DateTime.Now;
                emailAddress.BusinessEntityID = business.BusinessEntityID;
                emailAddress.Person = person;
                //db.Entry(person).State = EntityState.Modified;
                //db.SaveChanges();

                //emailAddress.BusinessEntityID = business.BusinessEntityID;
                //db.EmailAddresses.Add(emailAddress);
                //db.SaveChanges();
                //emailAddress.Person = person;
                password.Person = person;
                personCreditCard.Person = person;
                personPhone.Person = person;

                business.Person = person;
                var ble = person.EmailAddresses;


                db.Entry(business).State = EntityState.Modified;
                db.SaveChanges();//this will create a person as well.
                password.BusinessEntityID = id;
                db.CreditCards.Add(creditCard);
                db.SaveChanges();//need to create an id for personCreditCard

                personCreditCard.BusinessEntityID = id;
                personCreditCard.CreditCardID = creditCard.CreditCardID;
                db.PersonCreditCards.Add(personCreditCard);

               
                db.SaveChanges();
                //db.Passwords.Add(password);
                //db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                //TODO
                //var feature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();
                //return View("~/Views/Shared/Error.cshtml", feature?.Error);
                return View();
            }
        }

        private void HashPassword(string password)
        {
            byte[] buf = new byte[SALT_SIZE];
            rng.GetBytes(buf);
            _salt = Convert.ToBase64String(buf);
            Rfc2898DeriveBytes deriver2898 = new Rfc2898DeriveBytes(password.Trim(), buf, NUM_ITERATIONS);
            _hash = Convert.ToBase64String(deriver2898.GetBytes(16));
        }

        // GET: IndividualCustomer/Edit/5
        public ActionResult Edit(int id)
        {
            //TODO unhash password  Low on time right now.
            IndividualViewModel individualViewModel = GetEditAndDetailData(id);
            return View(individualViewModel);

        }

        // POST: IndividualCustomer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FirstName,LastName,PhoneNumber,BusinessEntityID,CardType,CardNumber,ExpMonth,ExpYear,Password,EmailAddress,Key")] IndividualViewModel newCustomer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Person person = db.People.Find(newCustomer.Key);//businessEntityId === Customer.PersonID
                    PersonCreditCard personCreditCard = db.PersonCreditCards.FirstOrDefault(o => o.BusinessEntityID == person.BusinessEntityID);
                    EmailAddress emailAddress = db.EmailAddresses.FirstOrDefault(o => o.BusinessEntityID == person.BusinessEntityID);
                    PersonPhone personPhone = db.PersonPhones.FirstOrDefault(o => o.BusinessEntityID == person.BusinessEntityID);
                    CreditCard creditCard = db.CreditCards.First(o => o.CreditCardID == personCreditCard.CreditCardID);
                 

                    person.LastName = newCustomer.LastName;
                    person.FirstName = newCustomer.FirstName;
                    emailAddress.EmailAddress1 = newCustomer.EmailAddress;
                    personPhone.PhoneNumber = newCustomer.PhoneNumber;
                    creditCard.CardNumber = newCustomer.CardNumber;
                    creditCard.CardType = newCustomer.CardType;
                    creditCard.ExpMonth = newCustomer.ExpMonth;
                    creditCard.ExpYear = newCustomer.ExpYear;

                    db.Entry(person).State = EntityState.Modified;
                    db.Entry(emailAddress).State = EntityState.Modified;
                    db.Entry(personPhone).State = EntityState.Modified;
                    db.Entry(personCreditCard).State = EntityState.Modified;
                    db.Entry(creditCard).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                return View(newCustomer);

            }
            catch
            {
                //TODO
                return View();
            }
        }

        // GET: IndividualCustomer/Delete/5
        public ActionResult Delete(int id)
        {
            Customer customer = db.Customers.Find(id);
            Person person = db.People.Find(customer.PersonID);
            PersonCreditCard personCreditCard = db.PersonCreditCards.FirstOrDefault(o => o.BusinessEntityID == person.BusinessEntityID);
            EmailAddress emailAddress = db.EmailAddresses.FirstOrDefault(o => o.BusinessEntityID == person.BusinessEntityID);
            PersonPhone personPhone = db.PersonPhones.FirstOrDefault(o => o.BusinessEntityID == person.BusinessEntityID);
            CreditCard creditCard = db.CreditCards.First(o => o.CreditCardID == personCreditCard.CreditCardID);
            IndividualViewModel individualViewModel = new IndividualViewModel();
            individualViewModel.LastName = person.LastName;
            individualViewModel.FirstName = person.FirstName;
            individualViewModel.EmailAddress = emailAddress.EmailAddress1;
            individualViewModel.PhoneNumber = personPhone.PhoneNumber;
            individualViewModel.CardNumber = creditCard.CardNumber;
            individualViewModel.CardType = creditCard.CardType;
            individualViewModel.ExpMonth = creditCard.ExpMonth;
            individualViewModel.ExpYear = creditCard.ExpYear;
            individualViewModel.Key = person.BusinessEntityID;

            return View(individualViewModel);
        }

        // POST: IndividualCustomer/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                Customer customer = db.Customers.Find(id);
                Person person = db.People.Find(customer.PersonID);
                var businessEntityID = person.BusinessEntityID;
                var salesOrderHeader = db.SalesOrderHeaders.Where(o => o.CustomerID == id);
                
                if (salesOrderHeader != null) db.SalesOrderHeaders.RemoveRange(salesOrderHeader);
                db.SaveChanges();
                EmailAddress emailAddress = db.EmailAddresses.FirstOrDefault(o => o.BusinessEntityID == businessEntityID);
                Password password = db.Passwords.FirstOrDefault(o => o.BusinessEntityID == businessEntityID);
                PersonCreditCard personCreditCard = db.PersonCreditCards.FirstOrDefault(o => o.BusinessEntityID == businessEntityID);
                PersonPhone personPhone = db.PersonPhones.FirstOrDefault(o => o.BusinessEntityID == businessEntityID);
                //TODO if any of the above are null redirect to 'corrupt' page

                db.EmailAddresses.Remove(emailAddress);
                db.Passwords.Remove(password);
                db.PersonCreditCards.Remove(personCreditCard);
                db.PersonPhones.Remove(personPhone);
                db.People.Remove(person);
                //lastly delete customer
                db.Customers.Remove(customer);//need to delete salesOrderHeader where CustomerID is
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                //TODO
                return View();
            }
        }

        // GET: IndividualCustomer/HistoryReport/5
        public ActionResult HistoryReport(int id)
        {
            Customer customer = db.Customers.Find(id);
            Response.Redirect(@"~/Reports/OrderHistory.aspx?id=" + customer.PersonID);
            return new EmptyResult();
        }

        private IndividualViewModel GetEditAndDetailData(int id)
        {
            Customer customer = db.Customers.Find(id);
            Person person = db.People.Find(customer.PersonID);
            PersonCreditCard personCreditCard = db.PersonCreditCards.FirstOrDefault(o => o.BusinessEntityID == person.BusinessEntityID);
            EmailAddress emailAddress = db.EmailAddresses.FirstOrDefault(o => o.BusinessEntityID == person.BusinessEntityID);
            PersonPhone personPhone = db.PersonPhones.FirstOrDefault(o => o.BusinessEntityID == person.BusinessEntityID);
            CreditCard creditCard = db.CreditCards.First(o => o.CreditCardID == personCreditCard.CreditCardID);
            IndividualViewModel individualViewModel = new IndividualViewModel();
            individualViewModel.LastName = person.LastName;
            individualViewModel.FirstName = person.FirstName;
            individualViewModel.EmailAddress = emailAddress.EmailAddress1;
            individualViewModel.PhoneNumber = personPhone.PhoneNumber;
            individualViewModel.CardNumber = creditCard.CardNumber;
            individualViewModel.CardType = creditCard.CardType;
            individualViewModel.ExpMonth = creditCard.ExpMonth;
            individualViewModel.ExpYear = creditCard.ExpYear;
            individualViewModel.Key = person.BusinessEntityID;
            return individualViewModel;
        }
    }
}
