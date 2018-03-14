using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DALLayer.DALConcrete;
using DALLayer.DALInterfaces;
using DALLayer.Entities;
using DALLayer.Repositories;
using DALLayer.IRepositories;
using DALLayer.Recources;

namespace DALLayer.Tests
{
    [TestClass]
    public class OrderRepositoryTests
    {
        private static readonly IUnitOfWork Uow = new UnitOfWork(new DatabaseContextFactory());
        private readonly IOrderRepository _repo = new OrderRepository(Uow);
        private readonly string _testString = "Test";
        private readonly string _testCustomerID = "ANATR";
        private readonly int _testOrderID = 11077;

        private readonly Order _testNewOrder = new Order
        {
            CustomerID = "ANATR",
            EmployeeID = 2,
            ShipAddress = "Another Adress",
            ShipVia = 1
        };




        [TestMethod]
        public void CanGetAll()
        {
            var orders = _repo.GetAll().ToList();
            foreach (var order in orders)
            {
                Console.WriteLine(order);
            }

            Assert.IsNotNull(orders);
            Assert.IsTrue(orders.Any());
        }

        [TestMethod]
        public void CanGetAllFromStoredProcedureCustOrderHist()
        {
            var orderHist = _repo.GetAllStoredProc<CustOrderHist>(_testCustomerID).ToList();
            foreach (var order in orderHist)
            {
                Console.WriteLine(order);
            }

            Assert.IsNotNull(orderHist);
            Assert.IsTrue(orderHist.Any());
        }

        [TestMethod]
        public void CanGetAllFromStoredProcedureCustOrderDetail()
        {
            var orderDetails = _repo.GetAllStoredProc<CustOrdersDetail>(_testOrderID).ToList();
            foreach (var order in orderDetails)
            {
                Console.WriteLine(order);
            }

            Assert.IsNotNull(orderDetails);
            Assert.IsTrue(orderDetails.Any());
        }

        [TestMethod]
        public void CanGetById()
        {
            var order = _repo.GetById(_testOrderID);
            if (order != null)
            {
                Console.WriteLine(order);
                foreach (var orderDetail in order.Details)
                {
                    Console.WriteLine(orderDetail);
                }
            }
            else
            {
                Console.WriteLine(ExceptionMessage.NoOrderWithId, _testOrderID);
            }

            Assert.IsNotNull(order);
            Assert.AreEqual(_testOrderID, order.OrderID);
        }

        [TestMethod]
        public void CanInsertOrder()
        {
            var status = 0;
            Uow.BeginTransaction();
            try
            {
                status = _repo.Insert(_testNewOrder, Uow.Transaction);
                Uow.Commit();
            }
            catch (Exception)
            {
                Uow.Transaction.Rollback();
                Uow.Dispose();
            }

            var newOrder = _repo.GetAll().Last();

            Assert.AreEqual(1, status);
            Assert.AreEqual(_testNewOrder.CustomerID, newOrder.CustomerID);
            Assert.AreEqual(_testNewOrder.ShipAddress, newOrder.ShipAddress);
        }

        [TestMethod]
        public void CanUpdateNewOrder()
        {
            var newOrder = _repo.GetAll().FirstOrDefault(_ => _.Status == StatusType.New);
            if (newOrder == null) return;
            var newAdress = newOrder.ShipAddress + _testString;
            newOrder.ShipAddress = newAdress;
            var status = UpdateOrder(ref newOrder);

            Assert.AreEqual(1, status);
            Assert.AreEqual(newAdress, newOrder.ShipAddress);
        }

        [TestMethod]
        public void CanNotUpdateInWorkOrder()
        {
            var inWorkOrder = _repo.GetAll().FirstOrDefault(_ => _.Status == StatusType.InWork);
            if (inWorkOrder == null) return;
            var newAdress = inWorkOrder.ShipAddress + _testString;
            inWorkOrder.ShipAddress = newAdress;
            var status = UpdateOrder(ref inWorkOrder);

            Assert.AreEqual(0, status);
            Assert.AreNotEqual(newAdress, inWorkOrder.ShipAddress);
        }

        [TestMethod]
        public void CanNotUpdateCompletedOrder()
        {
            var completedOrder = _repo.GetAll().FirstOrDefault(_ => _.Status == StatusType.Completed);
            if (completedOrder == null) return;
            var newAdress = completedOrder.ShipAddress + _testString;
            completedOrder.ShipAddress = newAdress;
            var status = UpdateOrder(ref completedOrder);

            Assert.AreEqual(0, status);
            Assert.AreNotEqual(newAdress, completedOrder.ShipAddress);
        }

        private int UpdateOrder(ref Order order)
        {
            var status = 0;
            Uow.BeginTransaction();
            try
            {
                status = _repo.Update(
                    order,
                    Uow.Transaction);
                Uow.Commit();
            }
            catch (Exception)
            {
                Uow.Transaction.Rollback();
                Uow.Dispose();
            }

            order = _repo.GetById(order.OrderID);
            return status;
        }

        [TestMethod]
        public void CanDeleteInWorkOrder()
        {
            var inWorkOrder = _repo.GetAll().FirstOrDefault(_ => _.Status == StatusType.InWork);
            var status = 0;
            Uow.BeginTransaction();
            try
            {
                if (inWorkOrder != null)
                    status = _repo.Delete(inWorkOrder.OrderID, Uow.Transaction);
                Uow.Commit();
            }
            catch (Exception)
            {
                Uow.Transaction.Rollback();
                Uow.Dispose();
            }

            Assert.IsTrue(status > 0);
        }

        [TestMethod]
        public void CanNotDeleteCompletedOrder()
        {
            var inWorkOrder = _repo.GetAll().FirstOrDefault(_ => _.Status == StatusType.Completed);
            var status = 0;
            Uow.BeginTransaction();
            try
            {
                if (inWorkOrder != null)
                    status = _repo.Delete(inWorkOrder.OrderID, Uow.Transaction);
                Uow.Commit();
            }
            catch (Exception)
            {
                Uow.Transaction.Rollback();
                Uow.Dispose();
            }

            Assert.IsTrue(status == 0);
        }

        [TestMethod]
        public void CanUpdateStatusToInwork()
        {
            var order = _repo.GetAll().FirstOrDefault(_ => _.Status == StatusType.New);
            if (order == null) return;
            var status = 0;
            Uow.BeginTransaction();
            try
            {
                status = _repo.UpdateOrderStatus(order.OrderID, Uow.Transaction, StatusType.InWork);
                Uow.Commit();
            }
            catch (Exception)
            {
                Uow.Transaction.Rollback();
                Uow.Dispose();
            }

            order = _repo.GetById(order.OrderID);

            if (order == null) return;
            Assert.AreEqual(1, status);
            Assert.AreEqual(StatusType.InWork, order.Status);
        }

        [TestMethod]
        public void CanUpdateStatusToCompleted()
        {
            var order = _repo.GetAll().FirstOrDefault(_ => _.Status == StatusType.InWork);
            if (order == null) return;
            var status = 0;
            Uow.BeginTransaction();
            try
            {
                status = _repo.UpdateOrderStatus(order.OrderID, Uow.Transaction, StatusType.Completed);
                Uow.Commit();
            }
            catch (Exception)
            {
                Uow.Transaction.Rollback();
                Uow.Dispose();
            }

            order = _repo.GetById(order.OrderID);

            if (order == null) return;
            Assert.AreEqual(1, status);
            Assert.AreEqual(StatusType.Completed, order.Status);
        }
    }
}
