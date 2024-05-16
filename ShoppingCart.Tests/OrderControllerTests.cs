using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Tests.Datasets;
using ShoppingCart.Utility;
using ShoppingCart.Web.Areas.Admin.Controllers;
using System.Linq.Expressions;
using Xunit;

namespace ShoppingCart.Tests
{
    public class OrderControllerTests
    {
        [Theory]
        [InlineData(5)]
        public void GetOrderDetails_ReturnOrderDetailsById(int id)
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = id };
            var orderDetails = new List<OrderDetail>
            {
                new OrderDetail { Id = 0, OrderHeaderId = id },
                new OrderDetail { Id = 1, OrderHeaderId = id },
                new OrderDetail { Id = 2, OrderHeaderId = id },
                new OrderDetail { Id = 3, OrderHeaderId = id },
            };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.OrderHeader.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>())).Returns(orderHeader);
            unitOfWorkMock.Setup(uow => uow.OrderDetail.GetAll(It.IsAny<string>())).Returns(orderDetails);
            var controller = new OrderController(unitOfWorkMock.Object);

            // Act
            var result = controller.OrderDetails(id);

            // Assert
            Assert.Equal(orderHeader, result.OrderHeader);
            Assert.Equal(orderDetails, result.OrderDetails.ToList());
        }

        [Theory]
        [InlineData(6)]
        public void SetToInProcess_UpdatesOrderStatus(int id)
        {
            // Arrange            
            var orderVM = new OrderVM { OrderHeader = new OrderHeader { Id = id } };

            var orderHeaderRepositoryMock = new Mock<IOrderHeaderRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var controller = new OrderController(unitOfWorkMock.Object);
            unitOfWorkMock.SetupGet(uow => uow.OrderHeader).Returns(orderHeaderRepositoryMock.Object);

            // Act
            controller.SetToInProcess(orderVM);

            // Assert            
            unitOfWorkMock.Verify(uow => uow.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, OrderStatus.StatusInProcess, It.IsAny<string>()), Times.Once);
            unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void SetToShipped_UpdatesOrderStatus()
        {
            // Arrange                  
            var orderHeader = new OrderHeader { Id = 3, Carrier = "Test carrier", TrackingNumber = "Test number" };
            var orderVm = new OrderVM { OrderHeader = orderHeader };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var orderHeaderRepositoryMock = new Mock<IOrderHeaderRepository>();
            unitOfWorkMock.SetupGet(uow => uow.OrderHeader).Returns(orderHeaderRepositoryMock.Object);
            orderHeaderRepositoryMock.Setup(repo => repo.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>())).Returns(orderHeader);
            var controller = new OrderController(unitOfWorkMock.Object);

            // Act
            controller.SetToShipped(orderVm);

            // Assert
            orderHeaderRepositoryMock.Verify(ohr => ohr.Update(It.Is<OrderHeader>(oh =>
                oh.Id == orderHeader.Id &&
                oh.Carrier == orderHeader.Carrier &&
                oh.TrackingNumber == orderHeader.TrackingNumber &&
                oh.OrderStatus == OrderStatus.StatusShipped)), Times.Once);
            unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void SetToCancelOrder_CancelsOrder()
        {
            // Arrange
            var orderVm = new OrderVM { OrderHeader = new OrderHeader { PaymentStatus = PaymentStatus.StatusPending } };
            var orderHeader = new OrderHeader { PaymentStatus = PaymentStatus.StatusPending };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.OrderHeader.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>())).Returns(orderHeader);
            var controller = new OrderController(unitOfWorkMock.Object);

            // Act
            controller.SetToCancelOrder(orderVm);

            // Assert
            unitOfWorkMock.Verify(uow => uow.OrderHeader.UpdateStatus(orderVm.OrderHeader.Id, OrderStatus.StatusCancelled, It.IsAny<string>()), Times.Once);
            unitOfWorkMock.Verify(uow => uow.Save(), Times.Once);
        }
    }
}
