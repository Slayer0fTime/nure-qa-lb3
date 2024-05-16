using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Tests.Datasets;
using ShoppingCart.Web.Areas.Admin.Controllers;
using System.Linq.Expressions;
using Xunit;

namespace ShoppingCart.Tests
{
    public class CategoryControllerTests
    {
        [Fact]
        public void GetCategories_All_ReturnAllCategories()
        {
            // Arrange
            Mock<ICategoryRepository> repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetAll(It.IsAny<string>()))
                .Returns(() => CategoryDataset.Categories);
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.Equal(CategoryDataset.Categories, result.Categories);
        }

        [Theory]
        [InlineData(5)]
        public void GetCategory_ReturnCategoryById(int id)
        {
            // Arrange
            var category = new Category { Id = id };
            Mock<ICategoryRepository> repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>())).Returns(category);
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = controller.Get(id);

            // Assert
            Assert.Equal(category, result.Category);
        }

        [Fact]
        public void CreateUpdate_AddCategory()
        {
            // Arrange
            var category = new Category { Id = 0 };
            var categoryVM = new CategoryVM { Category = category };
            var repositoryMock = new Mock<ICategoryRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            mockUnitOfWork.Setup(uow => uow.Save());
            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            controller.CreateUpdate(categoryVM);

            // Assert
            repositoryMock.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);
            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Theory]
        [InlineData(5)]
        public void DeleteData_DeletesCategoryById(int id)
        {
            // Arrange
            var category = new Category { Id = id };
            var repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>())).Returns(category);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            mockUnitOfWork.Setup(uow => uow.Save());

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            controller.DeleteData(id);

            // Assert
            repositoryMock.Verify(r => r.Delete(category), Times.Once);
            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

    }
}