using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using JobSearchAppBackend.DTOs;
using JobSearchAppBackend.Interfaces;
using JobSearchAppBackend.Models;
using JobSearchAppBackend.Services;
using JobSearchAppBackend.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace JobSearchTests
{
    [TestFixture]
    public class CompanyServiceTests
    {
        private Mock<ICompanyRepository> _mockRepository;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<CompanyService>> _mockLogger;
        private CompanyService _companyService;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<ICompanyRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<CompanyService>>();
            _companyService = new CompanyService(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
        }

        #region GetAllCompaniesAsync Tests

        [Test]
        public async Task GetAllCompaniesAsync_Returns_Mapped_CompanyDTO_List()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { Id = 1, Name = "Test Company" }
            };
            var companyDTOs = new List<CompanyDTO>
            {
                new CompanyDTO { Id = 1, Name = "Test Company" }
            };

            _mockRepository
                .Setup(repo => repo.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(companies);

            _mockMapper
                .Setup(m => m.Map<List<CompanyDTO>>(companies))
                .Returns(companyDTOs);

            // Act
            var result = await _companyService.GetAllCompaniesAsync();

            // Assert
            Assert.AreEqual(companyDTOs, result);
            _mockRepository.Verify(repo => repo.GetAllCompaniesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetAllCompaniesAsync_Throws_ApplicationException_When_Repository_Fails()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Repository failure"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.GetAllCompaniesAsync());

            StringAssert.Contains("An error occurred while retrieving companies", exception.Message);
        }

        #endregion

        #region GetCompanyByIdAsync Tests

        [Test]
        public async Task GetCompanyByIdAsync_Returns_CompanyDTO_When_Company_Exists()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Existing Company" };
            var companyDTO = new CompanyDTO { Id = 1, Name = "Existing Company" };

            _mockRepository
                .Setup(repo => repo.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(company);

            _mockMapper
                .Setup(m => m.Map<CompanyDTO>(company))
                .Returns(companyDTO);

            // Act
            var result = await _companyService.GetCompanyByIdAsync(1);

            // Assert
            Assert.AreEqual(companyDTO, result);
        }

        [Test]
        public void GetCompanyByIdAsync_Throws_ApplicationException_When_Company_Not_Found()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Company)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.GetCompanyByIdAsync(1));

            Assert.IsInstanceOf(typeof(KeyNotFoundException), exception.InnerException);
            StringAssert.Contains("Company not found", exception.InnerException.Message);
        }

        [Test]
        public void GetCompanyByIdAsync_Throws_ApplicationException_When_Repository_Fails()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Repository failure"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.GetCompanyByIdAsync(1));

            StringAssert.Contains("An error occurred while retrieving the company", exception.Message);
        }

        #endregion

        #region AddCompanyAsync Tests

        [Test]
        public async Task AddCompanyAsync_Returns_New_Company_Id_When_Valid_CompanyDTO()
        {
            // Arrange
            var companyDTO = new CompanyDTO { Id = 0, Name = "New Company" };
            var company = new Company { Id = 0, Name = "New Company" };

            _mockMapper
                .Setup(m => m.Map<Company>(companyDTO))
                .Returns(company);

            _mockRepository
                .Setup(repo => repo.AddCompanyAsync(company, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _companyService.AddCompanyAsync(companyDTO);

            // Assert
            Assert.AreEqual(1, result);
        }

        [Test]
        public void AddCompanyAsync_Throws_ApplicationException_When_CompanyDTO_Is_Null()
        {
            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.AddCompanyAsync(null));

            Assert.IsInstanceOf(typeof(ArgumentNullException), exception.InnerException);
        }

        [Test]
        public void AddCompanyAsync_Throws_ApplicationException_When_Repository_Fails()
        {
            // Arrange
            var companyDTO = new CompanyDTO { Id = 0, Name = "New Company" };
            var company = new Company { Id = 0, Name = "New Company" };

            _mockMapper
                .Setup(m => m.Map<Company>(companyDTO))
                .Returns(company);

            _mockRepository
                .Setup(repo => repo.AddCompanyAsync(company, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Repository failure"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.AddCompanyAsync(companyDTO));

            StringAssert.Contains("An error occurred while adding the company", exception.Message);
        }

        #endregion

        #region UpdateCompanyAsync Tests

        [Test]
        public async Task UpdateCompanyAsync_Updates_Company_Successfully()
        {
            // Arrange
            var companyDTO = new CompanyDTO { Id = 1, Name = "Updated Company" };
            var existingCompany = new Company { Id = 1, Name = "Old Company" };

            _mockRepository
                .Setup(repo => repo.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCompany);

            // Setup mapping (void method)
            _mockMapper
                .Setup(m => m.Map(companyDTO, existingCompany));

            _mockRepository
                .Setup(repo => repo.UpdateCompanyAsync(existingCompany, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _companyService.UpdateCompanyAsync(companyDTO);

            // Assert
            _mockRepository.Verify(repo => repo.UpdateCompanyAsync(existingCompany, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void UpdateCompanyAsync_Throws_ApplicationException_When_CompanyDTO_Is_Null()
        {
            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.UpdateCompanyAsync(null));

            Assert.IsInstanceOf(typeof(ArgumentNullException), exception.InnerException);
        }

        [Test]
        public void UpdateCompanyAsync_Throws_ApplicationException_When_Company_Not_Found()
        {
            // Arrange
            var companyDTO = new CompanyDTO { Id = 1, Name = "Updated Company" };

            _mockRepository
                .Setup(repo => repo.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Company)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.UpdateCompanyAsync(companyDTO));

            Assert.IsInstanceOf(typeof(KeyNotFoundException), exception.InnerException);
            StringAssert.Contains("Company not found", exception.InnerException.Message);
        }

        [Test]
        public void UpdateCompanyAsync_Throws_ApplicationException_When_Repository_Fails()
        {
            // Arrange
            var companyDTO = new CompanyDTO { Id = 1, Name = "Updated Company" };
            var existingCompany = new Company { Id = 1, Name = "Old Company" };

            _mockRepository
                .Setup(repo => repo.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCompany);

            _mockMapper
                .Setup(m => m.Map(companyDTO, existingCompany));

            _mockRepository
                .Setup(repo => repo.UpdateCompanyAsync(existingCompany, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Repository failure"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.UpdateCompanyAsync(companyDTO));

            StringAssert.Contains("An error occurred while updating the company", exception.Message);
        }

        #endregion

        #region DeleteCompanyAsync Tests

        [Test]
        public async Task DeleteCompanyAsync_Deletes_Company_Successfully()
        {
            // Arrange
            var existingCompany = new Company { Id = 1, Name = "Company To Delete" };

            _mockRepository
                .Setup(repo => repo.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCompany);

            _mockRepository
                .Setup(repo => repo.DeleteCompanyAsync(1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _companyService.DeleteCompanyAsync(1);

            // Assert
            _mockRepository.Verify(repo => repo.DeleteCompanyAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void DeleteCompanyAsync_Throws_ApplicationException_When_Company_Not_Found()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Company)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.DeleteCompanyAsync(1));

            Assert.IsInstanceOf(typeof(KeyNotFoundException), exception.InnerException);
            StringAssert.Contains("Company not found", exception.InnerException.Message);
        }

        [Test]
        public void DeleteCompanyAsync_Throws_ApplicationException_When_Repository_Fails()
        {
            // Arrange
            var existingCompany = new Company { Id = 1, Name = "Company To Delete" };

            _mockRepository
                .Setup(repo => repo.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCompany);

            _mockRepository
                .Setup(repo => repo.DeleteCompanyAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Repository failure"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ApplicationException>(
                async () => await _companyService.DeleteCompanyAsync(1));

            StringAssert.Contains("An error occurred while deleting the company", exception.Message);
        }

        #endregion
    }
}

